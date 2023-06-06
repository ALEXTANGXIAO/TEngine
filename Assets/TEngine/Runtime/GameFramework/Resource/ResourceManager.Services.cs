using System;
using System.IO;
using YooAsset;

namespace TEngine
{
    internal partial class ResourceManager
    {
        /// <summary>
        /// 资源文件解密服务类。
        /// </summary>
        private class GameDecryptionServices : IDecryptionServices
        {
            public ulong LoadFromFileOffset(DecryptFileInfo fileInfo)
            {
                return 32;
            }

            public byte[] LoadFromMemory(DecryptFileInfo fileInfo)
            {
                throw new NotImplementedException();
            }

            public Stream LoadFromStream(DecryptFileInfo fileInfo)
            {
                BundleStream bundleStream = new BundleStream(fileInfo.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return bundleStream;
            }

            public uint GetManagedReadBufferSize()
            {
                return 1024;
            }
        }

        /// <summary>
        /// 内置文件查询服务类。
        /// </summary>
        private class GameQueryServices : IQueryServices
        {
            public bool QueryStreamingAssets(string fileName)
            {
                string builtinFolderName = YooAssets.GetStreamingAssetBuildinFolderName();
                return StreamingAssetsHelper.FileExists($"{builtinFolderName}/{fileName}");
            }
        }
    }

    public class BundleStream : FileStream
    {
        public const byte KEY = 64;

        public BundleStream(string path, FileMode mode, FileAccess access, FileShare share) : base(path, mode, access, share)
        {
        }

        public BundleStream(string path, FileMode mode) : base(path, mode)
        {
        }

        public override int Read(byte[] array, int offset, int count)
        {
            var index = base.Read(array, offset, count);
            for (int i = 0; i < array.Length; i++)
            {
                array[i] ^= KEY;
            }

            return index;
        }
    }
}