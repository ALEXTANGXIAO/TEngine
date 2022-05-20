using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Huatuo.Editor
{
    internal class HTEditorInstaller
    {
        private static HTEditorInstaller instance = null;

        public static HTEditorInstaller Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new HTEditorInstaller();
                }

                return instance;
            }
        }

        HTEditorInstaller()
        {
        }

        public void Init()
        {
            if (File.Exists(HTEditorConfig.HuatuoVersionPath))
            {
                var data = File.ReadAllText(HTEditorConfig.HuatuoVersionPath, Encoding.UTF8);
                m_HuatuoVersion = JsonUtility.FromJson<HuatuoVersion>(data);
            }
            else
            {
                m_HuatuoVersion = default;
            }

            HTEditorCache.Instance.SetCacheDirectory(m_HuatuoVersion.CacheDir);
        }

        public void DoUninstall()
        {
            // backup libil2cpp
            if (Directory.Exists(HTEditorConfig.Libil2cppOritinalPath))
            {
                if (Directory.Exists(HTEditorConfig.Libil2cppPath))
                {
                    Directory.Delete(HTEditorConfig.Libil2cppPath, true);
                }

                Directory.Move(HTEditorConfig.Libil2cppOritinalPath, HTEditorConfig.Libil2cppPath);
            }

            m_InstallVersion.huatuoTag = "";
            m_InstallVersion.il2cppTag = "";
            SaveVersionLog();
            // 不存在原始备份目录
            // TODO 这里考虑下是否帮用户下载libil2cpp
        }

        public static void Enable(Action<string> callback)
        {
            var mv1 = HTEditorUtility.Mv(HTEditorConfig.Libil2cppPath, HTEditorConfig.Libil2cppOritinalPath);
            if (!string.IsNullOrEmpty(mv1))
            {
                Debug.LogError(mv1);
                callback?.Invoke(mv1);
                return;
            }

            mv1 = HTEditorUtility.Mv(HTEditorConfig.HuatuoIL2CPPBackPath, HTEditorConfig.HuatuoIL2CPPPath);
            if (!string.IsNullOrEmpty(mv1))
            {
                Debug.LogError(mv1);
                callback?.Invoke(mv1);
                return;
            }

            callback?.Invoke(null);
        }

        public static void Disable(Action<string> callback)
        {
            var mv1 = HTEditorUtility.Mv(HTEditorConfig.HuatuoIL2CPPPath, HTEditorConfig.HuatuoIL2CPPBackPath);
            if (!string.IsNullOrEmpty(mv1))
            {
                Debug.LogError(mv1);
                callback?.Invoke(mv1);
                return;
            }

            mv1 = HTEditorUtility.Mv(HTEditorConfig.Libil2cppOritinalPath, HTEditorConfig.Libil2cppPath);
            if (!string.IsNullOrEmpty(mv1))
            {
                Debug.LogError(mv1);
                callback?.Invoke(mv1);
                return;
            }

            callback?.Invoke(null);
        }

        public static void Uninstall(Action<string> callback)
        {
            Disable(ret =>
            {
                if (!string.IsNullOrEmpty(ret))
                {
                    callback?.Invoke(ret);
                    return;
                }

                if (Directory.Exists(HTEditorConfig.HuatuoIL2CPPBackPath))
                {
                    Directory.Delete(HTEditorConfig.HuatuoIL2CPPBackPath, true);
                }

                callback?.Invoke(null);
            });
        }

        public InstallVersion m_InstallVersion; // 当前安装临时使用的版本数据
        public HuatuoVersion m_HuatuoVersion; // 已安装的版本信息

        private bool m_bDoBackup;
        private string m_sBackupFileName;

        private IEnumerator Extract(Action<bool> callback)
        {
            var il2cppZip = HTEditorCache.Instance.GetZipPath(EFILE_NAME.IL2CPP, m_InstallVersion.il2cppTag);
            var huatuozip = HTEditorCache.Instance.GetZipPath(m_InstallVersion.huatuoType, m_InstallVersion.huatuoTag);

            var il2cppCachePath = Path.GetDirectoryName(il2cppZip) + $"/il2cpp_huatuo-{m_InstallVersion.il2cppTag}";
            var huatuoCachePath = Path.GetDirectoryName(huatuozip) + $"/huatuo-{m_InstallVersion.huatuoTag}";

            var cnt = 0;
            var haserr = false;
            var itor = HTEditorUtility.UnzipAsync(il2cppZip, il2cppCachePath, b => { cnt = b; },
                p => { EditorUtility.DisplayProgressBar("解压中...", $"il2cpp:{p}/{cnt}", (float) p / cnt); }, null,
                () => { haserr = true; });
            while (itor.MoveNext())
            {
                yield return itor.Current;
            }
            EditorUtility.ClearProgressBar();

            if (haserr)
            {
                callback?.Invoke(true);
                yield break;
            }

            cnt = 0;
            itor = HTEditorUtility.UnzipAsync(huatuozip, huatuoCachePath, b => { cnt = b; },
                p => { EditorUtility.DisplayProgressBar("解压中...", $"huatuo:{p}/{cnt}", (float) p / cnt); }, null,
                () => { haserr = true; });
            while (itor.MoveNext())
            {
                yield return itor.Current;
            }

            EditorUtility.ClearProgressBar();
            
            if (haserr)
            {
                callback?.Invoke(true);
                yield break;
            }

            var il2cppDirName = il2cppCachePath + $"/il2cpp_huatuo-{m_InstallVersion.il2cppTag}/libil2cpp";
            var huatuoDirName = huatuoCachePath + HTEditorCache.GetHuatuoZipInnerFolder(m_InstallVersion.huatuoType, m_InstallVersion.huatuoTag);
            if (!Directory.Exists(il2cppDirName))
            {
                Debug.LogError($"{il2cppDirName} not exists!!!");
                callback?.Invoke(true);
                yield break;
            }

            if (!Directory.Exists(huatuoDirName))
            {
                Debug.LogError($"{huatuoDirName} not exists!!!");
                callback?.Invoke(true);
                yield break;
            }

            try
            {
                if (Directory.Exists(HTEditorConfig.Libil2cppPath))
                {
                    Directory.Delete(HTEditorConfig.Libil2cppPath, true);
                }
                
                HTEditorUtility.CopyFilesRecursively(il2cppDirName, HTEditorConfig.HuatuoIL2CPPPath);
                HTEditorUtility.CopyFilesRecursively(huatuoDirName, HTEditorConfig.HuatuoPath);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                haserr = true;
            }

            callback?.Invoke(haserr);
        }

        public IEnumerator Install(InstallVersion installVersion, Action<bool> callback)
        {
            this.m_InstallVersion = installVersion;

            Debug.Log("备份il2cpp目录");
            var task = Task.Run(BackupLibil2cpp);
            while (!task.IsCompleted)
            {
                yield return null;
            }

            var hasErr = false;
            var itor = Extract(r => { hasErr = r; });
            while (itor.MoveNext())
            {
                yield return itor.Current;
            }

            if (hasErr)
            {
                RevertInstall();
                callback?.Invoke(false);
                yield break;
            }

            task = Task.Run(SaveVersionLog);
            while (!task.IsCompleted)
            {
                yield return null;
            }

            task = Task.Run(DelBackupLibil2cpp);
            while (!task.IsCompleted)
            {
                yield return null;
            }

            callback?.Invoke(true);
        }

        public void RevertInstall()
        {
            m_InstallVersion.huatuoTag = m_HuatuoVersion.HuatuoTag;
            m_InstallVersion.il2cppTag = m_HuatuoVersion.Il2cppTag;
            if (!m_bDoBackup)
            {
                return;
            }

            string installPathBak = Path.Combine(HTEditorConfig.Il2cppPath, m_sBackupFileName);
            // backup libil2cpp
            if (Directory.Exists(installPathBak))
            {
                Directory.Delete(HTEditorConfig.Libil2cppPath, true);
                Directory.Move(installPathBak, HTEditorConfig.Libil2cppPath);
            }
        }

        public void DelBackupLibil2cpp()
        {
            if (!m_bDoBackup)
            {
                return;
            }

            string installPathBak = Path.Combine(HTEditorConfig.Il2cppPath, m_sBackupFileName);
            // backup libil2cpp
            if (Directory.Exists(installPathBak))
            {
                Directory.Delete(installPathBak, true);
            }
        }

        public void BackupLibil2cpp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            m_sBackupFileName = $"libil2cpp_{ts.TotalSeconds}";
            string installPathBak = Path.Combine(HTEditorConfig.Il2cppPath, m_sBackupFileName);
            string original = Path.Combine(HTEditorConfig.Il2cppPath, "libil2cpp_original_unity");

            if (!Directory.Exists(HTEditorConfig.Libil2cppPath))
            {
                return;
            }

            // backup libil2cpp original
            if (!Directory.Exists(original))
            {
                Directory.Move(HTEditorConfig.Libil2cppPath, original);
            }

            if (Directory.Exists(HTEditorConfig.Libil2cppPath))
            {
                m_bDoBackup = true;
                Directory.Move(HTEditorConfig.Libil2cppPath, installPathBak);
            }
        }

        public void SaveVersionLog()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);

            // TODO 记录libil2cpp 和 huatuo 版本信息
            m_HuatuoVersion.HuatuoTag = m_InstallVersion.huatuoTag;
            m_HuatuoVersion.Il2cppTag = m_InstallVersion.il2cppTag;
            //m_HuatuoVersion.Il2cppUrl = HTEditorCache.Instance.GetDownUrlWithTagIl2cpp(m_InstallVersion.il2cppTag);
            //m_HuatuoVersion.HuatuoUrl = HTEditorCache.Instance.GetDownUrlWithTagHuatuo(m_InstallVersion.huatuoTag);
            m_HuatuoVersion.InstallTime = DateTime.Now.ToString();
            m_HuatuoVersion.Timestamp = Convert.ToInt64(ts.TotalMilliseconds);
            Debug.Log($"Save huatuo install version, path: {HTEditorConfig.HuatuoVersionPath}");
            File.WriteAllText(HTEditorConfig.HuatuoVersionPath, JsonUtility.ToJson(m_HuatuoVersion, true),
                Encoding.UTF8);
        }

        public void SaveCacheDir()
        {
            m_HuatuoVersion.CacheDir = HTEditorCache.Instance.CacheBasePath;
            File.WriteAllText(HTEditorConfig.HuatuoVersionPath, JsonUtility.ToJson(m_HuatuoVersion, true),
                Encoding.UTF8);
        }

        /*
        public static HuatuoRemoteConfig GetVersionData()
        {
            var data = File.ReadAllText(HTEditorConfig.HuatuoVersionPath, Encoding.UTF8);
            return JsonUtility.FromJson<HuatuoRemoteConfig>(data);
        }

        public static bool Extract(string zipPath, string extractDir, string installPath)
        {
            var result = ExtractZip(zipPath, extractDir, installPath);
            return result.Count > 0;
        }

        public static List<string> ExtractZip(string zipFilePath, string relativePath, string destPath)
        {
            var result = new List<string>();

            relativePath = relativePath.Replace(@"\", @"/");

            using (FileStream zipToOpen = new FileStream(zipFilePath, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {
                    var entry = archive.Entries.FirstOrDefault(x => x.FullName.ToUpper() == relativePath.ToUpper());
                    if (entry == null)
                        entry = archive.Entries.FirstOrDefault(x =>
                            x.FullName.ToUpper() == (relativePath + "/").ToUpper());

                    if (!string.IsNullOrWhiteSpace(entry.Name))
                    {
                        var path = Path.Combine(destPath, entry.Name);
                        using (var file = new FileStream(path, FileMode.Create, FileAccess.Write))
                        {
                            entry.Open().CopyTo(file);
                            file.Close();
                        }

                        result.Add(path);
                    }
                    else
                    {
                        var items = archive.Entries.Where(x => x.FullName.StartsWith(entry.FullName)).ToList();
                        foreach (var item in items.Where(x => string.IsNullOrWhiteSpace(x.Name)).OrderBy(x => x.Length))
                        {
                            var path = Path.Combine(destPath, item.FullName.Substring(entry.FullName.Length));
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);
                        }

                        foreach (var item in items.Where(x => !string.IsNullOrWhiteSpace(x.Name))
                            .OrderBy(x => x.Length))
                        {
                            var path = new FileInfo(Path.Combine(destPath,
                                item.FullName.Substring(entry.FullName.Length))).Directory.FullName;
                            path = Path.Combine(path, item.Name);
                            using (var file = new FileStream(path, FileMode.Create, FileAccess.Write))
                            {
                                item.Open().CopyTo(file);
                                file.Close();
                            }

                            result.Add(path);
                        }
                    }
                }
            }

            return result;
        }*/
    }
}
