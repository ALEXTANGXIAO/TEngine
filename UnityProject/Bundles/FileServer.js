const express = require("express");
const app = express();
const path = require("path");
const fs = require("fs");

const BuildPackage = "BuildPackage";
const BuildPlatform = "StandaloneWindows64";
// 设置静态资源目录为当前项目根目录下的public文件夹
// app.use(express.static(path.join(__dirname)));
console.log(`--__dirname: ${__dirname}`);
// 处理所有其他路由请求并返回index.html页面（如果存在）
app.get("*", (req, res) => {
  var p1 = new Promise((resolve, reject) => {
    resolve(readPackageName(req));
  });
  var p2 = new Promise((resolve, reject) => {
    resolve(readVersion());
  });
  Promise.all([p1, p2]).then((Responses) => {
    console.log("begin router");
    var indexFilePath = packageRouter(Responses[0], Responses[1]);
    console.log(`--indexFilePath: ${indexFilePath}`);
    downloadFile(res,indexFilePath);
  });
});
function readVersion() {
  var version = "0.0.1";
  version = fs.readFileSync(path.join(__dirname, BuildPlatform, "version.text"), "utf-8")
    .trim();
  console.log(`--version ${version}`);
  return version;
}
function readPackageName(req) {
  console.log(req.query);
  var packageName = req.query[BuildPackage];
  console.log(`--BuildPackage: ${packageName}`);
  return packageName.toString();
}
function packageRouter(packageName, version) {
  var packageDirName = path.dirname(packageName);
  var packageFileName = path.basename(packageName);
  const indexFilePath = path.join(
    __dirname,
    BuildPlatform,
    packageDirName,
    version,
    packageFileName
  );
  return indexFilePath;
}
function downloadFile(res,indexFilePath) {
  // 判断index.html文件是否存在
  if (!fs.existsSync(indexFilePath)) {
    return res.status(404).send("Not Found");
  } else {
    var fileName = path.basename(indexFilePath);
    console.log("Download file: " + fileName);
    res.download(indexFilePath, fileName);
  }
}
// 指定监听的端口号
const port = process.env.PORT || 8081;
const serverIp = process.env.IP || "127.0.0.1";
app.listen(port, () =>
  console.log(`Server is running on http://${serverIp}:${port}`)
);
