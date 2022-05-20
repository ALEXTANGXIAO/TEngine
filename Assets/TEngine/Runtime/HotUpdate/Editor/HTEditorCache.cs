using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Huatuo.Editor
{
    enum EFILE_NAME
    {
        NONE,
        HUATUO,
        IL2CPP,
        HUATUO_MAIN,
    }

    internal class HTEditorCache
    {
        private string libil2cppTagPrefix;
        private string huatuoTagPrefix;
        private int m_nDownloadTotal;
        private int m_counter;
        private int m_nSuccessCount;
        private int m_nFailedCount;

        private static HTEditorCache instance = null;

        public static HTEditorCache Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new HTEditorCache();
                }

                return instance;
            }
        }

        private static string CacheDirName = ".huatuo_cache";
        public string CacheBasePath;

        HTEditorCache()
        {
            libil2cppTagPrefix = HTEditorConfig.libil2cppTagPrefixGithub;
            huatuoTagPrefix = HTEditorConfig.huatuoTagPrefixGithub;
        }

        public void SetDownloadCount(int count)
        {
            m_nDownloadTotal = count;
            m_counter = 0;
            m_nSuccessCount = 0;
            m_nFailedCount = 0;
        }

        public bool DownloadDone()
        {
            return m_nDownloadTotal == m_nFailedCount + m_nSuccessCount;
        }

        public bool DownloadSuccess()
        {
            return m_nDownloadTotal == m_nSuccessCount;
        }

        public void SetCacheDirectory(string path)
        {
            var tmp = "";
            if (path == null || path.Length == 0)
            {
                tmp = Path.Combine(Path.GetFullPath("Library"), CacheDirName);
            }
            else
            {
                tmp = path;
            }

            try
            {
                Directory.CreateDirectory(tmp);
                CacheBasePath = tmp;
                HTEditorInstaller.Instance.SaveCacheDir();
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.LogError("缓存设置失败，请不要使用C盘路径做缓存");
            }
        }

        public string GetDownUrlWithTagHuatuo(string tag)
        {
            return $"{huatuoTagPrefix}/{tag}.zip";
        }

        public string GetDownUrlWithTagIl2cpp(string tag)
        {
            return $"{libil2cppTagPrefix}/{tag}.zip";
        }

        public string GetZipName(EFILE_NAME nameType, string tag)
        {
            var zipFileName = "";
            switch (nameType)
            {
                case EFILE_NAME.HUATUO:
                    zipFileName = $"huatuo-{tag}";
                    break;
                case EFILE_NAME.IL2CPP:
                    zipFileName = $"il2cpp_huatuo-{tag}";
                    break;
                default:
                    throw new Exception($"no support file type{nameof(nameType)}");
            }

            return zipFileName;
        }
        public static string GetHuatuoZipInnerFolder(EFILE_NAME nameType, string tag)
        {
            switch (nameType)
            {
                case EFILE_NAME.HUATUO_MAIN:
                    return $"/huatuo-main/huatuo";
                case EFILE_NAME.HUATUO:
                    return $"/huatuo-{tag}/huatuo";
            }
            return "error param";
        }
        public string GetZipPath(EFILE_NAME nameType, string tag)
        {
            var zipFileName = "";
            switch (nameType)
            {
                case EFILE_NAME.HUATUO_MAIN:
                    zipFileName = $"huatuo-{tag}";
                    break;
                case EFILE_NAME.HUATUO:
                    zipFileName = $"huatuo-{tag}";
                    break;
                case EFILE_NAME.IL2CPP:
                    zipFileName = $"il2cpp_huatuo-{tag}";
                    break;
                default:
                    throw new Exception($"no support file type{nameof(nameType)}");
            }

            return Path.Combine(CacheBasePath, $"{zipFileName}.zip");
        }

        public IEnumerator GetCache(EFILE_NAME nameType, string tag, string hashCode)
        {
            m_counter++;
            var downloadUrl = "";
            var zipFileName = "";
            switch (nameType)
            {
                case EFILE_NAME.HUATUO_MAIN:
                    zipFileName = $"huatuo-{tag}";
                    downloadUrl = $"{HTEditorConfig.huatuoPrefixGithub}/main.zip";
                    break;
                case EFILE_NAME.HUATUO:
                    zipFileName = $"huatuo-{tag}";
                    downloadUrl = $"{huatuoTagPrefix}/{tag}.zip";
                    break;
                case EFILE_NAME.IL2CPP:
                    zipFileName = $"il2cpp_huatuo-{tag}";
                    downloadUrl = $"{libil2cppTagPrefix}/{tag}.zip";
                    break;
                default:
                    throw new Exception($"no support file type{nameof(nameType)}");
            }

            var downloadErr = false;
            var zipPath = Path.Combine(CacheBasePath, $"{zipFileName}.zip");
            if (File.Exists(zipPath))
            {
                // TODO 校验文件MD5
                Debug.Log($"Download {zipFileName}, use cache file: {zipPath}");
                yield return null;
            }
            else
            {
                var curRetryCnt = 0;
                var maxRetryCnt = 0;
                var itor = HTEditorUtility.DownloadFile(downloadUrl, zipPath,
                    (curCnt, maxCnt) =>
                    {
                        curRetryCnt = curCnt;
                        maxRetryCnt = maxCnt;
                    },
                    p =>
                    {
                        var msg = $"下载中{(curRetryCnt > 0 ? $"[重试{curRetryCnt}/{maxRetryCnt}]" : "...")}";
                        EditorUtility.DisplayProgressBar(msg, $"{m_counter}/{m_nDownloadTotal}", p);
                    },
                    ret =>
                    {
                        EditorUtility.ClearProgressBar();
                        if (!string.IsNullOrEmpty(ret))
                        {
                            downloadErr = true;
                            EditorUtility.DisplayDialog("错误", $"下载{zipFileName}出错.\n{ret}", "ok");
                        }
                    });
                while (itor.MoveNext())
                {
                    yield return itor.Current;
                }

                if (!File.Exists(zipPath))
                {
                    EditorUtility.DisplayDialog("错误", $"下载的文件{zipPath}不存在", "ok");
                    downloadErr = false;
                }

                //else if (MD5.ComputeFileMD5(zipPath).ToLower() != hashCode)
                //{
                //    EditorUtility.DisplayDialog("错误", $"下载的文件{zipPath} hash不匹配，请重新下载", "ok");
                //    downloadErr = false;
                //}
            }

            if (downloadErr)
            {
                m_nFailedCount++;
            }
            else
            {
                m_nSuccessCount++;
            }
        }
    }
}
