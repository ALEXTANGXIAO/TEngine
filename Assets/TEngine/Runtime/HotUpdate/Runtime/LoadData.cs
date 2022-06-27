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
        public UpdateType Type;    //是否底包更新
        public UpdateStyle Style;  //是否强制更新
        public UpdateNotice Notice;//是否提示
        public List<LoadResource> List;
        public List<LoadResource> All;
    }

    public enum UpdateType
    {
        None = 0,
        PackageUpdate = 1, //底包更新
        ResourceUpdate = 2,//资源更新
    }

    public enum UpdateStyle
    {
        None = 0,
        Froce = 1,   //强制
        Optional = 2,//非强制
    }

    public enum UpdateNotice
    {
        None = 0,
        Notice = 1,   //提示
        NoNotice = 2,//非提示
    }

    public enum VersionRequestErrorCode
    {
        Ok = 1,
        Error = 2,
        Invalid_Param = 3,
        Update_Process = 4,
        Patch_Not_Exist = 5
    }
}
