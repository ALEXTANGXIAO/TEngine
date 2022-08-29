using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;

using Debug = UnityEngine.Debug;

namespace HybridCLR.Editor.Installer
{
    public enum InstallErrorCode
    {
        Ok,
        Il2CppInstallPathNotMatchIl2CppBranch,
        Il2CppInstallPathNotExists,
        NotIl2CppPath,
    }

    public partial class InstallerController
    {
        private string m_Il2CppInstallDirectory;

        public string Il2CppInstallDirectory
        {
            get
            {
                return m_Il2CppInstallDirectory;
            }
            set
            {
                m_Il2CppInstallDirectory = value?.Replace('\\', '/');
                if (!string.IsNullOrEmpty(m_Il2CppInstallDirectory))
                {
                    EditorPrefs.SetString("UnityInstallDirectory", m_Il2CppInstallDirectory);
                }
            }
        }

        private string GetIl2CppPlusBranchByUnityVersion(string unityVersion)
        {
            if (unityVersion.Contains("2019."))
            {
                return "2019.4.40";
            }
            if (unityVersion.Contains("2020."))
            {
                return "2020.3.33";
            }
            if (unityVersion.Contains("2021."))
            {
                return "2021.3.1";
            }
            return "not support";
        }

        public string Il2CppBranch => GetIl2CppPlusBranchByUnityVersion(Application.unityVersion);

        public string InitLocalIl2CppBatFile => Application.dataPath + "/../HybridCLRData/init_local_il2cpp_data.bat";

        public string InitLocalIl2CppBashFile => Application.dataPath + "/../HybridCLRData/init_local_il2cpp_data.sh";

        public InstallerController()
        {
            PrepareIl2CppInstallPath();
        }

        void PrepareIl2CppInstallPath()
        {
#if UNITY_EDITOR_OSX
            m_Il2CppInstallDirectory = EditorPrefs.GetString("Il2CppInstallDirectory");
            if (CheckValidIl2CppInstallDirectory(Il2CppBranch, m_Il2CppInstallDirectory) == InstallErrorCode.Ok)
            {
                return;
            }
            var il2cppBranch = Il2CppBranch;
            var curAppInstallPath = EditorApplication.applicationPath;
            if (curAppInstallPath.Contains(il2cppBranch))
            {
                Il2CppInstallDirectory = $"{curAppInstallPath}/Contents/il2cpp";
                return;
            }
            string unityHubRootDir = Directory.GetParent(curAppInstallPath).Parent.Parent.ToString();
            foreach (var unityInstallDir in Directory.GetDirectories(unityHubRootDir, "*", SearchOption.TopDirectoryOnly))
            {
                Debug.Log("nity install dir:" + unityInstallDir);
                if (unityInstallDir.Contains(il2cppBranch))
                {
                    Il2CppInstallDirectory = $"{unityInstallDir}/Unity.app/Contents/il2cpp";
                    return;
                }
            }

            Il2CppInstallDirectory = $"{curAppInstallPath}/Contents/il2cpp";
#else
            m_Il2CppInstallDirectory = EditorPrefs.GetString("Il2CppInstallDirectory");
            if (CheckValidIl2CppInstallDirectory(Il2CppBranch, m_Il2CppInstallDirectory) == InstallErrorCode.Ok)
            {
                return;
            }
            var il2cppBranch = Il2CppBranch;
            var curAppInstallPath = EditorApplication.applicationPath;
            if (curAppInstallPath.Contains(il2cppBranch))
            {
                Il2CppInstallDirectory = $"{Directory.GetParent(curAppInstallPath)}/Data/il2cpp";
                return;
            }
            string unityHubRootDir = Directory.GetParent(curAppInstallPath).Parent.Parent.ToString();
            Debug.Log("unity hub root dir:" + unityHubRootDir);
            foreach (var unityInstallDir in Directory.GetDirectories(unityHubRootDir, "*", SearchOption.TopDirectoryOnly))
            {
                Debug.Log("nity install dir:" + unityInstallDir);
                if (unityInstallDir.Contains(il2cppBranch))
                {
                    Il2CppInstallDirectory = $"{unityInstallDir}/Editor/Data/il2cpp";
                    return;
                }
            }

            Il2CppInstallDirectory = $"{Directory.GetParent(curAppInstallPath)}/Data/il2cpp";
#endif
        }

        public void InitHybridCLR(string il2cppBranch, string il2cppInstallPath)
        {
            if (CheckValidIl2CppInstallDirectory(il2cppBranch, il2cppInstallPath) != InstallErrorCode.Ok)
            {
                Debug.LogError($"请正确设置 il2cpp 安装目录");
                return;
            }

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                RunInitLocalIl2CppDataBat(il2cppBranch, il2cppInstallPath);
            }
            else
            {
                RunInitLocalIl2CppDataBash(il2cppBranch, il2cppInstallPath);
            }
        }

        public bool HasInstalledHybridCLR()
        {
            return Directory.Exists($"{BuildConfig.LocalIl2CppDir}/libil2cpp/hybridclr");
        }

        public InstallErrorCode CheckValidIl2CppInstallDirectory(string il2cppBranch, string installDir)
        {
            installDir = installDir.Replace('\\', '/');
            if (!Directory.Exists(installDir))
            {
                return InstallErrorCode.Il2CppInstallPathNotExists;
            }

            if (!installDir.Contains(il2cppBranch))
            {
                return InstallErrorCode.Il2CppInstallPathNotMatchIl2CppBranch;
            }

            if (!installDir.EndsWith("/il2cpp"))
            {
                return InstallErrorCode.NotIl2CppPath;
            }

            return InstallErrorCode.Ok;
        }

        public bool IsUnity2019(string branch)
        {
            return branch.Contains("2019.");
        }

        private void RunInitLocalIl2CppDataBat(string il2cppBranch, string il2cppInstallPath)
        {
            using (Process p = new Process())
            {
                p.StartInfo.WorkingDirectory = BuildConfig.HybridCLRDataDir;
                p.StartInfo.FileName = InitLocalIl2CppBatFile;
                p.StartInfo.UseShellExecute = true;
                p.StartInfo.Arguments = $"{il2cppBranch} \"{il2cppInstallPath}\"";
                p.Start();
                p.WaitForExit();
                if (IsUnity2019(il2cppBranch))
                {
                    string srcIl2CppDll = $"{BuildConfig.HybridCLRDataDir}/ModifiedUnityAssemblies/2019.4.40/Unity.IL2CPP.dll";
                    string dstIl2CppDll = $"{BuildConfig.LocalIl2CppDir}/build/deploy/net471/Unity.IL2CPP.dll";
                    File.Copy(srcIl2CppDll, dstIl2CppDll, true);
                    Debug.Log($"copy {srcIl2CppDll} => {dstIl2CppDll}");
                }
                if (p.ExitCode == 0 && HasInstalledHybridCLR())
                {
                    Debug.Log("HybirdCLR 安装成功");
                }
            }
        }

        private void RunInitLocalIl2CppDataBash(string il2cppBranch, string il2cppInstallPath)
        {
            using (Process p = new Process())
            {
                p.StartInfo.WorkingDirectory = Application.dataPath + "/../HybridCLRData";
                p.StartInfo.FileName = "/bin/bash";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.Arguments = $"init_local_il2cpp_data.sh {il2cppBranch} '{il2cppInstallPath}'";
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.Start();
                string output = p.StandardOutput.ReadToEnd();
                Debug.Log(output);
                p.WaitForExit();
                if (HasInstalledHybridCLR())
                {
                    Debug.Log("安装成功!!!");
                }
            }
        }
    }
}
