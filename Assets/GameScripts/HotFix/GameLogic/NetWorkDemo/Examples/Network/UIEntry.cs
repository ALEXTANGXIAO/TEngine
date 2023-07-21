using System;
using System.Collections;
using TEngine.Core.Network;
using TEngine.Core;
using TEngine.Logic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0436

namespace TEngine.Demo
{
    public class UIEntry : MonoBehaviour
    {
        public Scene Scene;
        public bool IsConnect;
        public bool IsLoginAddress;

        public Text Message;
        public Button SendButton;
        public Button SendRPCButton;
        public Button ReceiveButton;
        public Button SendAddressButton;
        public Button SendAddressRPCButton;
        public Button ReceiveAddressButton;
        public Button ConnentServerButton;
        public Button LoginAddressButton;

        IEnumerator Start()
        {
            yield return new WaitForSeconds(2f);
            // 框架初始化
            // Scene = GameSystem.Init();
            // 把当前工程的程序集装载到框架中、这样框架才会正常的操作
            // demo里是把unity自带的程序集加载到框架中了
            // 如果你有多个工程、可以多次调用AssemblyManager.Load把其他工程装载下
            // 可以通过ab包来加载工程dll、也可以通过引用方式来拿到程序集加载
            // 装载后例如网络协议等一些框架提供的功能就可以使用了、如果不装载就没办法使用
            // AssemblyManager.Load(233, this.GetType().Assembly);
            // 给Scene挂在一个组件、用来记录下操作提示UI的组件、方便后面使用
            Scene = GameApp.Instance.Scene;
            var networkEntryComponent = Scene.AddComponent<NetworkEntryComponent>();
            networkEntryComponent.Action = LogDebug;
            // 到这里为止框架就配置完成了
            // 下面是给每个按钮增加事件、不属于框架的部分了
            SendButton.onClick.AddListener(OnSendButtonClick);
            SendRPCButton.onClick.AddListener(() => OnSendRPCButtonClick().Coroutine());
            ReceiveButton.onClick.AddListener(OnReceiveButtonClick);
            SendAddressButton.onClick.AddListener(OnSendAddressButtonClick);
            SendAddressRPCButton.onClick.AddListener(() => OnSendAddressRPCButtonClick().Coroutine());
            ReceiveAddressButton.onClick.AddListener(OnReceiveAddressButtonClick);
            LoginAddressButton.onClick.AddListener(() => OnLoginAddressButtonClick().Coroutine());
            ConnentServerButton.onClick.AddListener(OnConnectServerButtonClick);
        }

        private void OnConnectServerButtonClick()
        {
            // Demo对应服务器的Network解决方案
            // 创建网络连接
            // CreateSession有五个参数:
            // remoteAddress:连接的服务器地址 地址在Demo下Config/Excel/Server里四个文件配置出的 
            // networkProtocolType:网络协议类型、目前只有TCP和UDP(KCP)这两种
            // onConnectComplete:当跟服务器建立连接后的回调
            // onConnectFail:当网络无法连接或出错时的回调
            // connectTimeout:连接超时时间、默认是5000毫秒
            Scene.CreateSession("127.0.0.1:20000", NetworkProtocolType.KCP, OnConnectComplete, OnConnectFail, OnConnectDisconnect);
            // 注意:框架使用的ProtoBuf协议、文件定义的位置在Demo下Config/ProtoBuf/里
            // 生成协议是在服务器工程生成
            // ProtoBuf有三个文件:
            // OuterMessage:用于客户端和服务器之间通讯的所有协议都定义到这里
            // InnerMessage:用于服务器之间通讯的协议都定义到这里
            // InnerBsonMessage:用于服务器之间通讯的、但不是使用的ProtoBuf协议、而是用的Bosn格式压缩的
        }

        private void OnConnectComplete()
        {
            IsConnect = true;
            Scene.Session.AddComponent<SessionHeartbeatComponent>().Start(15000);
            LogDebug("已连接到服务器");
        }

        private void OnConnectFail()
        {
            IsConnect = false;
            LogError("无法连接到服务器");
        }
        
        private void OnConnectDisconnect()
        {
            IsConnect = false;
            LogError("服务器主动断开了连接");
        }

        private void OnSendButtonClick()
        {
            if (!IsConnect)
            {
                LogError("没有连接到服务器、请先点击连接到服务器按钮在进行此操作");
                return;
            }

            Scene.Session.Send(new H_C2G_Message()
            {
                Message = "Hello TEngine"
            });
        }

        private async FTask OnSendRPCButtonClick()
        {
            if (!IsConnect)
            {
                LogError("没有连接到服务器、请先点击连接到服务器按钮在进行此操作");
                return;
            }

            var response = (H_G2C_MessageResponse)await Scene.Session.Call(new H_C2G_MessageRequest()
            {
                Message = "Hello TEngine"
            });

            LogDebug(response.ErrorCode == 0
                ? $"接收到服务器返回的消息:{response.Message}"
                : $"发生错误 ErrorCode:{response.ErrorCode}");
        }

        private void OnReceiveButtonClick()
        {
            if (!IsConnect)
            {
                LogError("没有连接到服务器、请先点击连接到服务器按钮在进行此操作");
                return;
            }

            // 一般服务器推送给客户端消息都是服务器主动的
            // 但demo中服务器没有这样的逻辑
            // 所以发送一个消息告诉服务器、然后服务器在主动推送一个消息给客户端就可以了
            // 虽然这样看起来跟RPC消息很像、但原理是不一样的
            // 客户端怎么接收服务器发送的协议、您可以看下H_G2C_ReceiveMessageToServer这个类

            Scene.Session.Send(new H_C2G_PushMessageToClient()
            {
                Message = "请推送一个消息给我"
            });

            LogDebug("请推送一个消息给我");
        }

        private async FTask OnLoginAddressButtonClick()
        {
            if (!IsConnect)
            {
                LogError("没有连接到服务器、请先点击连接到服务器按钮在进行此操作");
                return;
            }

            var response = (H_G2C_LoginAddressResponse)await Scene.Session.Call(new H_C2G_LoginAddressRequest()
            {
                Message = "注册Address"
            });

            if (response.ErrorCode == 0)
            {
                IsLoginAddress = true;
                LogDebug("Address 注册完成");
                return;
            }

            IsLoginAddress = false;
            LogError($"Address 注册失败 ErrorCode:{response.ErrorCode}");
        }

        private void OnSendAddressButtonClick()
        {
            if (!IsLoginAddress)
            {
                LogError("没有注册到Address、请先点击注册Address按钮在进行此操作");
                return;
            }

            Scene.Session.Send(new H_C2M_Message()
            {
                Message = "Hello TEngine Address"
            });

            LogDebug("发送成功");
        }

        private async FTask OnSendAddressRPCButtonClick()
        {
            if (!IsLoginAddress)
            {
                LogError("没有注册到Address、请先点击注册Address按钮在进行此操作");
                return;
            }

            var response = (H_M2C_MessageResponse)await Scene.Session.Call(new H_C2M_MessageRequest()
            {
                Message = "这里是客户端发送给MAP的一个AddressRPC消息"
            });

            if (response.ErrorCode == 0)
            {
                LogDebug(response.Message);
                return;
            }

            LogError($"发送消息失败 ErrorCode:{response.ErrorCode}");
        }

        private void OnReceiveAddressButtonClick()
        {
            if (!IsLoginAddress)
            {
                LogError("没有注册到Address、请先点击注册Address按钮在进行此操作");
                return;
            }

            Scene.Session.Send(new H_C2M_PushAddressMessageToClient()
            {
                Message = "请推送一个消息给我"
            });

            LogDebug("请推送一个消息给我");
        }

        private void LogError(string text)
        {
            Message.text = text;
            Log.Error(text);
        }

        private void LogDebug(string text)
        {
            Message.text = text;
            Log.Debug(text);
        }
    }

    /// <summary>
    /// 这个组件是方便接收服务器推送消息显示在UI上使用
    /// 我这里是方便使用这样定义的、不一定非要这样做
    /// </summary>
    public class NetworkEntryComponent : Entity
    {
        public Action<string> Action;
    }

    /// <summary>
    /// 这个是客户端接收服务器的处理程序
    /// </summary>
    public class H_G2C_ReceiveMessageToServerHandler : Message<H_G2C_ReceiveMessageToServer>
    {
        protected override async FTask Run(Session session, H_G2C_ReceiveMessageToServer message)
        {
            var networkEntryComponent = session.Scene.GetComponent<NetworkEntryComponent>();
            networkEntryComponent.Action($"收到服务器推送的消息 message:{message.Message}");
            await FTask.CompletedTask;
        }
    }

    /// <summary>
    /// 这个是客户端接收服务器的Address消息
    /// </summary>
    public class H_M2C_ReceiveAddressMessageToServerHandler : Message<H_M2C_ReceiveAddressMessageToServer>
    {
        protected override async FTask Run(Session session, H_M2C_ReceiveAddressMessageToServer message)
        {
            var networkEntryComponent = session.Scene.GetComponent<NetworkEntryComponent>();
            networkEntryComponent.Action($"收到服务器推送的Address消息 message:{message.Message}");
            await FTask.CompletedTask;
        }
    }
}