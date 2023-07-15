namespace TEngine.Logic;

/// <summary>
/// 账号信息
/// </summary>
public class AccountInfo : Entity
{
    /// <summary>
    /// 用户唯一ID。
    /// </summary>
    public uint UID { get; set; }
    
    /// <summary>
    /// 用户名。
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 密码。
    /// </summary>
    public string Password { get; set; }
    
    /// <summary>
    /// 渠道唯一ID。
    /// </summary>
    public uint SDKUID { get; set; }
    
    /// <summary>
    /// 是否禁用账号。
    /// </summary>
    public bool Forbid { get; set; }
}