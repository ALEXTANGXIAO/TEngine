using System;
using System.IO;
using UnityEngine;

namespace TEngineCore.Editor
{
    public class AssetbundleEncryption
    {
        public static void EncryptAssetBundlesByDirectory(string bundlePath, ulong offset)
        {
            if (offset < 1)
            {
                Debug.LogWarning("无法生效的偏移值:" + offset);
                return;
            }

            if (string.IsNullOrEmpty(bundlePath))
            {
                Debug.LogError("bundlePath为空");
                return;
            }

            if (!Directory.Exists(bundlePath))
            {
                Debug.LogError("不存在的AB包路径：" + bundlePath);
                return;
            }


            using (FileTree fileTree =
                   FileTree.CreateWithExcludeFilter(bundlePath, new[] { ".manifest", ".bin" }))
            {
                foreach (FileInfo file in fileTree.GetAllFiles())
                {
                    byte[] fileData = File.ReadAllBytes(file.FullName);
                    ulong newLength = (ulong)fileData.Length + offset;
                    byte[] buffer = new byte[newLength];

                    Array.Copy(fileData, buffer, (int)offset);
                    Array.Copy(fileData, 0, buffer, (int)offset, fileData.Length);


                    FileStream fs = File.OpenWrite(file.FullName);
                    fs.Write(buffer, 0, (int)newLength);
                    fs.Close();

                    Debug.Log(file.FullName);
                }
            }

        }
    }
}