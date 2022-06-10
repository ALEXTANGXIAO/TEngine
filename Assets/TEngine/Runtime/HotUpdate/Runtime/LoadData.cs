using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEngine
{
    public enum GameStatus
    {
        First = 0,
        AssetLoad = 1
    }

    public struct LoadResource
    {
        public string Url;//资源名称
        public string Md5;//资源的md5码
        public long Size; //资源大小（字节为单位）
        public string RemoteUrl;//服务器地址
    }

    public class LoadData
    {
    }
}
