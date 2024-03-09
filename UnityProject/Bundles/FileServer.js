/**
 * @fileOverview 服务器的主要文件。
 */

// 引入必要的模块
const express = require("express");
const app = express();
const path = require("path");
const fs = require("fs");

/**
 * @constant {string} BuildPackage - 构建包的查询参数名称
 */
const BuildPackage = "BuildPackage";

/**
 * @constant {string} BuildPlatform - 构建平台的名称
 */
const BuildPlatform = "StandaloneWindows64";

// 设置静态资源目录
// app.use(express.static(path.join(__dirname)));

/**
 * 处理所有其他路由并返回index.html如果存在
 * @param {express.Request} req - 请求对象
 * @param {express.Response} res - 响应对象
 */
app.get("*", (req, res) => {
  // 读取构建包名称和版本
  var p1 = readPackageName(req);
  var p2 = readVersion();
  Promise.all([p1, p2]).then((Responses) => {
    console.log("begin router");
    // 创建索引文件路径
    var indexFilePath = packageRouter(Responses[0], Responses[1]);
    console.log(`--indexFilePath: ${indexFilePath}`);
    // 下载索引文件
    downloadFile(res, indexFilePath);
  });

});

/**
 * 读取StandaloneWindows64目录下的version.text文件中的版本
 * @returns {string} 版本字符串
 */
function readVersion() {
  // 默认版本号
  var version = "0.0.1";
  // 从文件中读取版本号
  version = fs.readFileSync(path.join(__dirname, BuildPlatform, "version.text"), "utf-8").trim();
  console.log(`--version ${version}`);
  return version;
}

/**
 * 读取请求查询中的构建包名称
 * @param {express.Request} req - 请求对象
 * @returns {string} 构建包名称
 */
function readPackageName(req) {
  // 从请求中获取构建包名称
  var packageName = req.query[BuildPackage];
  console.log(`--BuildPackage: ${packageName}`);
  return packageName.toString();
}

/**
 * 根据构建包名称和版本创建索引文件路径
 * @param {string} packageName - 构建包名称
 * @param {string} version - 版本字符串
 * @returns {string} 索引文件路径
 */
function packageRouter(packageName, version) {
  // 处理构建包路径
  packageName = packageName.toString();
  version = version.toString();
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

/**
 * 下载索引文件，如果存在，否则返回404 Not Found响应
 * @param {express.Response} res - 响应对象
 * @param {string} indexFilePath - 索引文件路径
 */
function downloadFile(res, indexFilePath) {
  // 判断文件是否存在并处理下载
  if (!fs.existsSync(indexFilePath)) {
    return res.status(404).send("Not Found");
  } else {
    var fileName = path.basename(indexFilePath);
    console.log("Download file: " + fileName);
    res.download(indexFilePath, fileName);
  }
}

/**
 * 服务器配置 - 端口号
 */
const port = process.env.PORT || 8081;

/**
 * 服务器配置 - IP地址
 */
const serverIp = process.env.IP || "127.0.0.1";

// 启动服务器
app.listen(port, () =>
  console.log(`Server is running on http://${serverIp}:${port}`)
);