using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Huatuo.Editor.ThirdPart.ICSharpCode.SharpZipLib.Zip;
using UnityEngine;
using UnityEngine.Networking;


namespace Huatuo.Editor
{
    /// <summary>
    /// 这个类是Huatuo编辑器中使用到的各种小工具
    /// </summary>
    public static class HTEditorUtility
    {
        /// <summary>
        /// Compares two versions to see which is greater.
        /// </summary>
        /// <param name="a">Version to compare against second param</param>
        /// <param name="b">Version to compare against first param</param>
        /// <returns>-1 if the first version is smaller, 1 if the first version is greater, 0 if they are equal</returns>
        public static int CompareVersions(string a, string b)
        {
            var versionA = VersionStringToInts(a);
            var versionB = VersionStringToInts(b);
            for (var i = 0; i < Mathf.Max(versionA.Length, versionB.Length); i++)
            {
                if (VersionPiece(versionA, i) < VersionPiece(versionB, i))
                    return -1;
                if (VersionPiece(versionA, i) > VersionPiece(versionB, i))
                    return 1;
            }

            return 0;
        }

        private static int VersionPiece(IList<int> versionInts, int pieceIndex)
        {
            return pieceIndex < versionInts.Count ? versionInts[pieceIndex] : 0;
        }

        private static int[] VersionStringToInts(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                return new[] { 0 };
            }

            int piece;
            return version.Split('.')
                .Select(v => int.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out piece) ? piece : 0)
                .ToArray();
        }

        /// <summary>
        /// 异步解压zip文件
        /// </summary>
        /// <param name="zipFile">zip文件</param>
        /// <param name="destDir">解压后的目录</param>
        /// <param name="begin">开始解压</param>
        /// <param name="progress">解压中的进度</param>
        /// <param name="complete">解压结束</param>
        /// <param name="failure">解压失败</param>
        /// <returns>协程</returns>
        public static IEnumerator UnzipAsync(string zipFile, string destDir, Action<int> begin,
            Action<int> progress, Action complete, Action failure)
        {
            if (Directory.Exists(destDir))
            {
                Directory.Delete(destDir, true);
            }

            Debug.Log($"[UnzipAsync]----:{zipFile} {destDir}");
            var tmpCnt = 0;

            var itor = FastZip.ExtractZip(zipFile, destDir, count =>
            {
                tmpCnt = count;
                Debug.Log($"[UnzipAsync] begin:{zipFile} {destDir}");
                begin?.Invoke(count);
                progress?.Invoke(0);
            }, progress, () =>
            {
                Debug.Log($"[UnzipAsync] complete:{zipFile} {destDir}");
                progress?.Invoke(tmpCnt);
                complete?.Invoke();
            }, () =>
            {
                Debug.Log($"[UnzipAsync] failure:{zipFile} {destDir}");
                failure?.Invoke();
            });
            while (itor.MoveNext())
            {
                yield return itor.Current;
            }
        }

        /// <summary>
        /// 拷贝
        /// </summary>
        /// <param name="src">源目录</param>
        /// <param name="dst">目标目录</param>
        public static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            try
            {
                //创建所有新目录
                foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
                }

                //复制所有文件 & 保持文件名和路径一致
                foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// 移动目录
        /// </summary>
        /// <param name="src">源目录</param>
        /// <param name="dst">目标目录</param>
        /// <returns>错误信息</returns>
        public static string Mv(string src, string dst)
        {
            Debug.Log($"[MV] {src} {dst}");
            var ret = "";
            if (!Directory.Exists(src))
            {
                ret = $"Can't Find {src}";
            }
            else if (Directory.Exists(dst))
            {
                ret = $"{dst} Already Exists!";
            }
            else
            {
                Directory.Move(src, dst);
            }

            return ret;
        }

        /// <summary>
        /// 对https的请求不做证书验证
        /// </summary>
        private class IgnoreHttps : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData)
            {
                return true;
            }
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="strUrl">来源url</param>
        /// <param name="strDstFile">解压后的目录</param>
        /// <param name="progress">下载进度</param>
        /// <param name="done">下载完成，参数是错误信息</param>
        /// <param name="bValidateCertificate">是否选择忽略证书检查</param>
        /// <returns>协程</returns>
        public static IEnumerator DownloadFile(string strUrl, string strDstFile, Action<int, int> begin,
            Action<float> progress, Action<string> done, int retryCnt = 3, bool bValidateCertificate = false)
        {
            var nPos = 0;
            retryCnt++;
            retryCnt = Math.Max(1, retryCnt);

            var err = "";
            do
            {
                begin?.Invoke(nPos, retryCnt - 1);

                nPos++;

                Debug.Log($"[DownloadFile]{strUrl}\ndest:{strDstFile}");
                if (File.Exists(strDstFile))
                {
                    File.Delete(strDstFile);
                }

                yield return null;

                /*
                ulong totalLength = 0;
                
                using var headRequest = UnityWebRequest.Head(strUrl);
                yield return headRequest.SendWebRequest();
                if (!string.IsNullOrEmpty(headRequest.error))
                {
                    Debug.LogError("获取下载的文件大小失败");
                    break;
                }
                
                totalLength = ulong.Parse(headRequest.GetResponseHeader("Content-Length"));//获取文件总大小
                Debug.Log("获取大小" + totalLength);
*/
                var www = new UnityWebRequest(strUrl)
                {
                    downloadHandler = new DownloadHandlerFile(strDstFile),
                    timeout = 1000
                };

                if (!bValidateCertificate)
                {
                    www.certificateHandler = new IgnoreHttps();
                }

                progress?.Invoke(0f);
                var req = www.SendWebRequest();
                while (!req.isDone)
                {
                    progress?.Invoke(req.progress);
                    yield return null;
                }

                progress?.Invoke(100f);
                err = www.error;
                if (string.IsNullOrEmpty(www.error))
                {
                    break;
                }
            } while (nPos < retryCnt);


            done?.Invoke(err);
        }
        public static IEnumerator HttpRequest<T>(string url, Action<T, string> callback, int retryCnt = 3,
            bool bValidateCertificate = false)
        {
            var nPos = 0;
            retryCnt++;
            retryCnt = Math.Max(1, retryCnt);

            var err = "";
            T ret = default;
            do
            {
                nPos++;
                var msg = $"Fetching {url}";
                if (nPos > 1)
                {
                    msg = $"{msg} retry:{nPos - 1}";
                }

                Debug.Log(msg);
                var www = new UnityWebRequest(url)
                {
                    downloadHandler = new DownloadHandlerBuffer(),
                    timeout = 100
                };

                if (!bValidateCertificate)
                {
                    www.certificateHandler = new IgnoreHttps();
                }

                yield return www.SendWebRequest();
                do
                {
                    if (!string.IsNullOrEmpty(www.error))
                    {
                        Debug.LogError(www.error);
                        err = $"【1】Http error。\n[{www.error}]";

                        break;
                    }

                    var json = www.downloadHandler.text;
                    if (string.IsNullOrEmpty(json))
                    {
                        Debug.LogError("Unable to retrieve SDK version manifest.  Showing installed SDKs only.");
                        err = $"【2】Http error。";

                        break;
                    }
                    if (json.StartsWith("["))
                    {
                        json = $"{{\"items\":{json}}}";
                    }

                    ret = JsonUtility.FromJson<T>(json);
                } while (false);

                if (!ret.Equals(default(RemoteConfig)))
                {
                    break;
                }
            } while (nPos < retryCnt);

            callback?.Invoke(ret, err);
        }
    }
}
