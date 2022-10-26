using System.Net;
using TEngine.Runtime;
using TEngine.Runtime.UIModule;
using TEngineProto;
using UnityEngine;

public class NetTestMain : MonoBehaviour
{
    public const string MainTcp = "MainTcp";

    public string Ip = "127.0.0.1";

    public int Port = 8080;
    
    void Start()
    {
        //Demo示例，监听TEngine流程加载器OnStartGame事件
        //抛出这个事件说明框架流程加载完成（热更新，初始化等）
        GameEvent.AddEventListener(TEngineEvent.OnStartGame,OnStartGame);

    }

    /// <summary>
    /// NetworkChannel通信Channel
    /// </summary>
    private INetworkChannel _networkChannel;
    
    /// <summary>
    /// OnStartGame
    /// </summary>
    private void OnStartGame()
    {
        Log.Debug("TEngineEvent.OnStartGame");
        
        //创建网络Channel Service类型 Tcp
        _networkChannel = TEngine.Runtime.Network.Instance.CreateNetworkChannel(MainTcp, ServiceType.Tcp, new NetworkChannelHelper());
         
        //连接Channel 本地8081 需要开启服务器
        _networkChannel.Connect(IPAddress.Parse(Ip),Port);
        
        //注册消息包回调 ActionCode.Login -> Action Login
        TEngine.Runtime.Network.Instance.RegisterHandler(MainTcp,(int)ActionCode.Login,Login);
    }

    /// <summary>
    /// 测试发送消息包，需要开启服务器
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            var a = MemoryPool.Acquire<MainPack>();
            a.actioncode = ActionCode.Login;
            a.requestcode = RequestCode.User;
            a.loginPack = new LoginPack();
            a.loginPack.username = "1111";
            a.loginPack.password = "2222";
            TEngine.Runtime.Network.Instance.Send(MainTcp, a);
        }
    }

    /// <summary>
    /// Login消息回调
    /// </summary>
    /// <param name="mainPack">消息包</param>
    private void Login(MainPack mainPack)
    {
        Log.Debug(mainPack.extstr);
    }
}


