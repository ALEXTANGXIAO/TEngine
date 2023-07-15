namespace TEngine.Logic;

public class UserInfoAwakeSystem : AwakeSystem<UserInfo>
{
    protected override void Awake(UserInfo self)
    {
        self.Awake();
    }
}

/// <summary>
/// 角色信息。
/// </summary>
public class UserInfo : Entity
{
    //昵称
    public string UserName { get; set; }

    //等级
    public int Level { get; set; }

    //余额
    public long Money { get; set; }


    //上次游戏角色序列 1/2/3
    public int LastPlay { get; set; }

    //public List<Ca>
    public void Awake()
    {
        UserName = string.Empty;
        Level = 1;
        Money = 10000;
        LastPlay = 0;
    }

}