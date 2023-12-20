/// <summary>
/// APP更新类型。
/// </summary>
public enum UpdateType
{
    None = 0,

    //资源更新
    ResourceUpdate = 1,

    //底包更新
    PackageUpdate = 2,
}

/// <summary>
/// 强制更新类型。
/// </summary>
public enum UpdateStyle
{
    None = 0,
    Force = 1, //强制(不更新无法进入游戏。)
    Optional = 2, //非强制(不更新可以进入游戏。)
}

/// <summary>
/// 是否提示更新。
/// </summary>
public enum UpdateNotice
{
    None = 0,
    Notice = 1, //提示
    NoNotice = 2, //非提示
}

public enum GameStatus
{
    First = 0,
    AssetLoad = 1
}

/// <summary>
/// 版本更新数据。
/// </summary>
public class UpdateData
{
    /// <summary>
    /// 当前版本信息。
    /// </summary>
    public string CurrentVersion;

    /// <summary>
    /// 是否底包更新。
    /// </summary>
    public UpdateType UpdateType;

    /// <summary>
    /// 是否强制更新。
    /// </summary>
    public UpdateStyle UpdateStyle;

    /// <summary>
    /// 是否提示。
    /// </summary>
    public UpdateNotice UpdateNotice;

    /// <summary>
    /// 热更资源地址。
    /// </summary>
    public string HostServerURL;

    /// <summary>
    /// 备用热更资源地址。
    /// </summary>
    public string FallbackHostServerURL;
}