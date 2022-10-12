using System;
using UnityEditor;
using UnityEngine;

namespace Game.Editor {
    public sealed class GenProto {
        [MenuItem("TEngine/生成Sproto|GenSproto(需自行安装lua)", false, 11)]
        private static void GenProtoFunc() {
            RunBat("GenSproto.bat", Application.dataPath + "/TEngine/Tools~/Sproto/");
        }

        private static void RunBat(string batFile, string workingDir) {
            var path = FormatPath(workingDir + batFile);
            if (!System.IO.File.Exists(path)) {
                Debug.LogError("Error, Can't find the bat file： " + path);
            }
            else {
                System.Diagnostics.Process proc = null;
                try {
                    proc = new System.Diagnostics.Process();
                    proc.StartInfo.WorkingDirectory = workingDir;
                    proc.StartInfo.FileName = batFile;
                    proc.Start();
                    proc.Close();
                }
                catch (System.Exception ex) {
                    Debug.LogFormat("Exception Occurred: {0}, {1}", ex.Message, ex.StackTrace.ToString());
                }
            }
        }

        static string FormatPath(string path) {
            path = path.Replace("/", "\\");
            if (Application.platform == RuntimePlatform.OSXEditor) {
                path = path.Replace("\\", "/");
            }

            return path;
        }
    }
}