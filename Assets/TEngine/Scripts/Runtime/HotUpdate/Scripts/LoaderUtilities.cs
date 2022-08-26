using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using TEngine.Runtime.HotUpdate;
using UnityEngine;

namespace TEngine.Runtime
{
    public class LoaderUtilities
    {
        /// <summary>
        /// 获取文件的md5码
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetMd5Hash(string fileName)
        {
            if (!File.Exists(fileName))
            {
                TLogger.LogWarning($"not exit file,path:{fileName}");
                return string.Empty;
            }

            try
            {
                using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    MD5 md5 = new MD5CryptoServiceProvider();
                    byte[] retVal = md5.ComputeHash(file);
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < retVal.Length; i++)
                    {
                        sb.Append(retVal[i].ToString("x2"));
                    }

                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                TLogger.LogError("GetMD5Hash() fail,error:" + ex.Message);
                return string.Empty;
            }
        }

        #region WebRequest

        public class NetContent
        {
            public bool Statue;
            public bool Result;
            public string MsgContent;
            public byte[] data;
            public Action<bool, string> SuccessCallback;
            public Action<bool, string> ErrorCallback;

            public void SetResult(bool result, string content)
            {
                Statue = true;
                Result = result;
                MsgContent = content;
            }

            public void SetParam(byte[] param)
            {
                data = param;
            }

            public void SetAction(Action<bool, string> success, Action<bool, string> faile)
            {
                SuccessCallback = success;
                ErrorCallback = faile;
            }

            public void Clear()
            {
                Statue = false;
                Result = false;
                MsgContent = "";
            }
        }

        private static NetContent _msgContent = new NetContent();
        private static Coroutine _coroutine;
        private static HttpWebRequest webReq;

        /// <summary>
        /// 网络请求
        /// </summary>
        /// <param name="postUrl"></param>
        /// <param name="paramData"></param>
        /// <param name="dataEncode"></param>
        /// <param name="msgId"></param>
        /// <param name="appid"></param>
        /// <param name="appSecret"></param>
        /// <param name="successCallback"></param>
        /// <param name="errorCallback"></param>
        public static void PostWebRequest(
            string postUrl,
            string paramData,
            Action<bool, string> successCallback = null,
            Action<bool, string> errorCallback = null
        )
        {
            TLogger.LogInfo($"TEngine.LoaderUtilities.PostWebRequest url:{postUrl},paramData:{paramData}");
            _msgContent.Clear();

            byte[] byteArray = Encoding.UTF8.GetBytes(paramData);

            if (webReq != null)
            {
                webReq.Abort();
                webReq = null;
                GC.Collect();
            }

            webReq = (HttpWebRequest)WebRequest.Create(new Uri(postUrl));

            webReq.Method = "POST";
            webReq.ContentType = "application/json";
            webReq.ContentLength = byteArray.Length;
            _msgContent.SetParam(byteArray);
            _msgContent.SetAction(successCallback, errorCallback);

            var result = webReq.BeginGetRequestStream(RequestComplete, webReq);
            ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, TimeoutCallback, result, 2000, true);

            if (_coroutine != null)
            {
                MonoUtility.StopCoroutine(_coroutine);
            }

            _coroutine = MonoUtility.StartCoroutine(_Response(_msgContent));
        }

        private static IEnumerator _Response(NetContent content)
        {
            while (content.Statue == false)
            {
                yield return null;
            }

            if (content.Result)
            {
                _msgContent.SuccessCallback?.Invoke(content.Result, content.MsgContent);
            }
            else
            {
                _msgContent.ErrorCallback?.Invoke(content.Result, "");
            }

            if (_coroutine != null)
            {
                MonoUtility.StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }

        private static void RequestComplete(IAsyncResult result)
        {
            if (result != null && result.IsCompleted)
            {
                var request = result.AsyncState as HttpWebRequest;
                try
                {
                    Stream stream = request.EndGetRequestStream(result);
                    stream.Write(_msgContent.data, 0, _msgContent.data.Length);
                    stream.Close();
                    stream.Dispose();
                    var rspResult = webReq.BeginGetResponse(ResponseComplete, webReq);
                    ThreadPool.RegisterWaitForSingleObject(rspResult.AsyncWaitHandle, TimeoutCallback, result, 2000,
                        true);
                }
                catch (Exception e)
                {
                    _msgContent.SetResult(false, e.ToString());
                    throw;
                }
            }
        }

        private static void ResponseComplete(IAsyncResult asyncResult)
        {
            HttpWebRequest request;
            HttpWebResponse response;
            StreamReader reader;
            Stream recive;

            request = (asyncResult.AsyncState as HttpWebRequest);
            var res = request.EndGetResponse(asyncResult);
            response = res as HttpWebResponse;
            if (response != null && response.StatusCode == HttpStatusCode.OK)
            {
                recive = response.GetResponseStream();
                reader = new StreamReader(recive);
                try
                {
                    string result = reader.ReadToEnd();
                    _msgContent.SetResult(true, result);
                    reader.Close();
                    reader.Dispose();
                    recive.Close();
                    recive.Dispose();
                    response.Close();
                    request.Abort();
                }
                catch (Exception e)
                {
                    _msgContent.SetResult(false, e.ToString());
                    throw;
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }

                    if (request != null)
                    {
                        reader.Dispose();
                    }

                    if (response != null)
                    {
                        response.Close();
                    }

                    if (request != null)
                    {
                        request.Abort();
                    }
                }
            }
            else
            {
                if (response == null)
                {
                    _msgContent.SetResult(false, "HttpResponse:response is null");
                }
                else
                {
                    _msgContent.SetResult(false, "HttpResponse:" + response.StatusCode.ToString());
                }
            }
        }

        private static void TimeoutCallback(object state, bool timedOut)
        {
            if (!timedOut) return;
            _msgContent.SetResult(false, "timeout");
            IAsyncResult result = state as IAsyncResult;
            if (result != null)
            {
                try
                {
                    Stream stream = webReq.EndGetRequestStream(result);
                    stream.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                webReq.Abort();
            }
        }

        /// <summary>
        /// 延迟执行
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="time"></param>
        public static Coroutine DelayFun(Action callback, YieldInstruction time)
        {
            return MonoUtility.StartCoroutine(DelayFunIEnumerator(callback, time));
        }

        public static IEnumerator DelayFunIEnumerator(Action callback, YieldInstruction time)
        {
            yield return time;
            callback();
        }

        #endregion

        /// <summary>
        /// 数据格式转换
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public static string FormatData(long data)
        {
            string result = "";
            if (data < 0)
                data = 0;

            if (data > 1024 * 1024)
            {
                result = ((int)(data / (1024 * 1024))).ToString() + "MB";
            }
            else if (data > 1024)
            {
                result = ((int)(data / 1024)).ToString() + "KB";
            }
            else
            {
                result = data + "B";
            }

            return result;
        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static long GetFileSize(string path)
        {
            using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return file.Length;
            }
        }

        /// <summary> 
        /// GET请求与获取结果 
        /// </summary> 
        public static string HttpGet(string url, string postDataStr = "")
        {
            HttpWebRequest request =
                (HttpWebRequest)WebRequest.Create(url + (postDataStr == "" ? "" : "?") + postDataStr);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

        /// <summary> 
        /// POST请求与获取结果 
        /// </summary> 
        public static string HttpPost(string url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postDataStr.Length;
            StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.ASCII);
            writer.Write(postDataStr);
            writer.Flush();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string encoding = response.ContentEncoding;
            if (encoding == null || encoding.Length < 1)
            {
                encoding = "UTF-8"; //默认编码 
            }

            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
            string retString = reader.ReadToEnd();
            return retString;
        }

        /// <summary>
        /// 删除文件夹
        /// </summary>
        /// <param name="dir"></param>
        public static void DeleteFolder(string dir)
        {
            try
            {
                if (!Directory.Exists(dir))
                    return;

                foreach (string d in Directory.GetFileSystemEntries(dir))
                {
                    if (File.Exists(d))
                    {
                        FileInfo fi = new FileInfo(d);
                        if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        {
                            fi.Attributes = FileAttributes.Normal;
                        }

                        LoaderUtilities.DeleteFile(d); //直接删除其中的文件 
                    }
                    else
                    {
                        DirectoryInfo d1 = new DirectoryInfo(d);
                        if (d1.GetFiles().Length != 0)
                        {
                            DeleteFolder(d1.FullName); ////递归删除子文件夹
                        }

                        Directory.Delete(d, true);
                    }
                }
            }
            catch (Exception e)
            {
                TLogger.LogError(e.StackTrace.ToString());
                throw;
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public static void DeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception e)
            {
                TLogger.LogError(e.ToString());
            }
        }

        /// <summary>
        /// 显示提示框，目前最多支持三个按钮
        /// </summary>
        /// <param name="desc">描述</param>
        /// <param name="showtype">类型（MessageShowType）</param>
        /// <param name="OnOk">点击事件</param>
        /// <param name="OnCancle">取消事件</param>
        /// <param name="OnPackage">更新事件</param>
        public static void ShowMessageBox(string desc, MessageShowType showtype = MessageShowType.OneButton,
            LoadStyle.StyleEnum style = LoadStyle.StyleEnum.Style_Default,
            Action OnOk = null,
            Action OnCancle = null,
            Action OnPackage = null)
        {
            UILoadMgr.Show(UIDefine.UILoadTip, desc);
            var ui = UILoadMgr.GetActiveUI(UIDefine.UILoadTip) as UILoadTip;
            if (ui == null) return;
            ui.OnOk = OnOk;
            ui.OnCancle = OnCancle;
            ui.Showtype = showtype;
            ui.OnEnter(desc);

            var ls = ui.GetComponent<LoadStyle>();
            if (ls)
            {
                ls.SetStyle(style);
            }
        }

        /// <summary>
        /// 拷贝文件夹
        /// </summary>
        /// <param name="srcPath">原目录地址</param>
        /// <param name="destPath">目标目录地址</param>
        public static void CopyDirectory(string srcPath, string destPath)
        {
            try
            {
                if (string.IsNullOrEmpty(srcPath) || !Directory.Exists(srcPath))
                {
                    Debug.LogError("不存在的原目录地址：" + srcPath);
                    return;
                }

                if (!Directory.Exists(destPath))
                    Directory.CreateDirectory(destPath);

                var dir = new DirectoryInfo(srcPath);
                var fileinfo = dir.GetFileSystemInfos();
                foreach (var i in fileinfo)
                {
                    string sub_name = i.Name;
                    if (i is DirectoryInfo)
                    {
                        if (!Directory.Exists($"{destPath}/{sub_name}"))
                        {
                            Directory.CreateDirectory($"{destPath}/{sub_name}");
                        }

                        CopyDirectory(i.FullName, $"{destPath}/{sub_name}");
                    }
                    else
                    {
                        File.Copy(i.FullName, $"{destPath}/{sub_name}", true);
                    }
                }
            }
            catch (Exception e)
            {
                TLogger.LogError(e.ToString());
                throw;
            }
        }

        private static readonly char[] DirectorySeperators =
        {
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar,
        };

        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="path">目标目录地址</param>
        /// <param name="is_last_file"></param>
        public static void MakeAllDirectory(string path, bool is_last_file = true)
        {
            try
            {
                if (is_last_file)
                {
                    path = Path.GetDirectoryName(path);
                }

                if (path == null)
                    return;

                var pathFragments = path.Split(DirectorySeperators);
                if (pathFragments.Length <= 0)
                    return;

                path = pathFragments[0];
                if (path != string.Empty && !Directory.Exists(path))
                    Directory.CreateDirectory(path);

                for (var i = 1; i < pathFragments.Length; i++)
                {
                    path += Path.DirectorySeparatorChar;

                    string pathFragment = pathFragments[i];
                    if (string.IsNullOrEmpty(pathFragment))
                        continue;

                    path += pathFragment;
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                }
            }
            catch (Exception e)
            {
                TLogger.LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// 清除Meta文件
        /// </summary>
        /// <param name="targePath"></param>
        public static void ClearMeta(string targePath)
        {
            try
            {
                var infos = new DirectoryInfo(targePath);
                FileSystemInfo[] files = infos.GetFileSystemInfos("*", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    //是文件
                    if (files[i] is FileInfo file && file.FullName.EndsWith(".meta"))
                    {
                        File.Delete(file.FullName);
                    }
                }
            }
            catch (Exception e)
            {
                TLogger.LogError(e.ToString());
                throw;
            }
        }
    }
}