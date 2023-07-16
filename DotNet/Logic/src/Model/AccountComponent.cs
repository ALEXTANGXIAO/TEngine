namespace TEngine.Logic;

/// <summary>
/// 场景账号管理组件。
/// <remarks>[Scene]</remarks>
/// </summary>
public class AccountComponent:Entity, INotSupportedPool
{
    /// <summary>
    /// 当前登陆的账号。
    /// </summary>
    public readonly Dictionary<uint, AccountInfo?> accountInfoMap = new Dictionary<uint, AccountInfo?>();

    /// <summary>
    /// 获取账号信息。
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    public AccountInfo? Get(uint uid)
    {
        this.accountInfoMap.TryGetValue(uid, out AccountInfo? ret);
        return ret;
    }

    /// <summary>
    /// 添加当前登录的账号。
    /// </summary>
    /// <param name="accountInfo">账号信息。</param>
    /// <returns></returns>
    public bool Add(AccountInfo? accountInfo)
    {
        if (accountInfo == null)
        {
            return false;
        }

        if (this.accountInfoMap.ContainsKey(accountInfo.UID))
        {
            return false;
        }
        
        this.accountInfoMap[accountInfo.UID] = accountInfo;
        
        return true;
    }
    
    /// <summary>
    /// 移除当前登录的账号。
    /// </summary>
    /// <param name="accountInfo">账号信息。</param>
    /// <returns></returns>
    public bool Remove(AccountInfo? accountInfo)
    {
        if (accountInfo == null)
        {
            return false;
        }

        if (!this.accountInfoMap.ContainsKey(accountInfo.UID))
        {
            return false;
        }

        this.accountInfoMap.Remove(accountInfo.UID);
        
        return true;
    }
}