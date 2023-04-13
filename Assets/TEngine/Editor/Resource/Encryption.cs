using System;
using System.IO;
using TEngine;
using YooAsset;

public class FileOffsetEncryption : IEncryptionServices
{
    public EncryptResult Encrypt(EncryptFileInfo fileInfo)
    {
        int offset = 32;
        byte[] fileData = File.ReadAllBytes(fileInfo.FilePath);
        var encryptedData = new byte[fileData.Length + offset];
        Buffer.BlockCopy(fileData, 0, encryptedData, offset, fileData.Length);

        EncryptResult result = new EncryptResult();
        result.LoadMethod = EBundleLoadMethod.LoadFromFileOffset;
        result.EncryptedData = encryptedData;
        return result;
    }
}

public class FileStreamEncryption : IEncryptionServices
{
    public EncryptResult Encrypt(EncryptFileInfo fileInfo)
    {
        // LoadFromStream
        var fileData = File.ReadAllBytes(fileInfo.FilePath);
        for (int i = 0; i < fileData.Length; i++)
        {
            fileData[i] ^= BundleStream.KEY;
        }

        EncryptResult result = new EncryptResult();
        result.LoadMethod = EBundleLoadMethod.LoadFromStream;
        result.EncryptedData = fileData;
        return result;
    }
}