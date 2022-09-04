using System;
using System.Collections.Generic;
using UnityEngine;

namespace TEngine.Runtime
{
    /// <summary>
    /// 网络组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("TEngine/Network")]
    public class Network : UnitySingleton<Network>
    {
        private INetworkManager m_NetworkManager = null;

        public NetworkManager NetworkManager
        {
            private set;
            get;
        }

        /// <summary>
        /// 获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        public override int Priority => 10;
        

        /// <summary>
        /// 获取网络频道数量。
        /// </summary>
        public int NetworkChannelCount
        {
            get
            {
                if (m_NetworkManager == null)
                {
                    return 0;
                }
                return m_NetworkManager.NetworkChannelCount;
            }
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            m_NetworkManager = new NetworkManager();
            NetworkManager = m_NetworkManager as NetworkManager;
            if (m_NetworkManager == null)
            {
                Log.Fatal("Network manager is invalid.");
                return;
            }

            m_NetworkManager.NetworkConnected += OnNetworkConnected;
            m_NetworkManager.NetworkClosed += OnNetworkClosed;
            m_NetworkManager.NetworkMissHeartBeat += OnNetworkMissHeartBeat;
            m_NetworkManager.NetworkError += OnNetworkError;
            m_NetworkManager.NetworkCustomError += OnNetworkCustomError;
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (m_NetworkManager != null)
            {
                NetworkManager.Update(elapseSeconds, realElapseSeconds);;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (NetworkManager != null)
            {
                NetworkManager.Shutdown();
            }
        }

        /// <summary>
        /// 检查是否存在网络频道。
        /// </summary>
        /// <param name="name">网络频道名称。</param>
        /// <returns>是否存在网络频道。</returns>
        public bool HasNetworkChannel(string name)
        {
            return m_NetworkManager.HasNetworkChannel(name);
        }

        /// <summary>
        /// 获取网络频道。
        /// </summary>
        /// <param name="name">网络频道名称。</param>
        /// <returns>要获取的网络频道。</returns>
        public INetworkChannel GetNetworkChannel(string name)
        {
            return m_NetworkManager.GetNetworkChannel(name);
        }

        /// <summary>
        /// 获取所有网络频道。
        /// </summary>
        /// <returns>所有网络频道。</returns>
        public INetworkChannel[] GetAllNetworkChannels()
        {
            return m_NetworkManager.GetAllNetworkChannels();
        }

        /// <summary>
        /// 获取所有网络频道。
        /// </summary>
        /// <param name="results">所有网络频道。</param>
        public void GetAllNetworkChannels(List<INetworkChannel> results)
        {
            m_NetworkManager.GetAllNetworkChannels(results);
        }

        /// <summary>
        /// 创建网络频道。
        /// </summary>
        /// <param name="name">网络频道名称。</param>
        /// <param name="serviceType">网络服务类型。</param>
        /// <param name="networkChannelHelper">网络频道辅助器。</param>
        /// <returns>要创建的网络频道。</returns>
        public INetworkChannel CreateNetworkChannel(string name, ServiceType serviceType, INetworkChannelHelper networkChannelHelper)
        {
            return m_NetworkManager.CreateNetworkChannel(name, serviceType, networkChannelHelper);
        }

        /// <summary>
        /// 销毁网络频道。
        /// </summary>
        /// <param name="name">网络频道名称。</param>
        /// <returns>是否销毁网络频道成功。</returns>
        public bool DestroyNetworkChannel(string name)
        {
            return m_NetworkManager.DestroyNetworkChannel(name);
        }

        private void OnNetworkConnected(INetworkChannel channel, object obj)
        {
            GameEventMgr.Instance.Send(NetWorkEventId.NetworkConnectedEvent,channel, obj);
        }

        private void OnNetworkClosed(INetworkChannel channel)
        {
            GameEventMgr.Instance.Send(NetWorkEventId.NetworkClosedEvent,channel);
        }

        private void OnNetworkMissHeartBeat(INetworkChannel channel, int missCount)
        {
            GameEventMgr.Instance.Send(NetWorkEventId.NetworkMissHeartBeatEvent,channel,missCount);
        }

        private void OnNetworkError(INetworkChannel channel, NetworkErrorCode errorCode, string message)
        {
            GameEventMgr.Instance.Send(NetWorkEventId.NetworkErrorEvent,channel,errorCode,message);
        }

        private void OnNetworkCustomError(INetworkChannel channel,object message)
        {
            GameEventMgr.Instance.Send(NetWorkEventId.NetworkCustomErrorEvent,channel,message);
        }

        /// <summary>
        /// 注册网络消息包处理函数。
        /// </summary>
        /// <param name="channelName"></param>
        /// <param name="actionId"></param>
        /// <param name="msgDelegate"></param>
        /// <param name="checkRepeat"></param>
        public void RegisterHandler(string channelName, int actionId, CsMsgDelegate msgDelegate,
            bool checkRepeat = true)
        {
            var channel = this.GetNetworkChannel(channelName);
            if (channel == null)
            {
                Log.Warning($"Channel is null :{channelName}");
                return;
            }
            channel.RegisterHandler(actionId,msgDelegate);
        }

        /// <summary>
        /// 注销网络消息包处理函数。
        /// </summary>
        /// <param name="channelName"></param>
        /// <param name="actionId"></param>
        /// <param name="msgDelegate"></param>
        public void RmvHandler(string channelName, int actionId, CsMsgDelegate msgDelegate)
        {
            var channel = this.GetNetworkChannel(channelName);
            if (channel == null)
            {
                Log.Warning($"Channel is null :{channelName}");
                return;
            }
            channel.RmvHandler(actionId,msgDelegate);
        }

        /// <summary>
        /// 向远程主机发送消息包。
        /// </summary>
        /// <param name="channelName"></param>
        /// <param name="packet"></param>
        /// <returns></returns>
        public bool Send(string channelName, TEngineProto.MainPack packet)
        {
            var channel = this.GetNetworkChannel(channelName);
            if (channel == null)
            {
                Log.Warning($"Channel is null :{channelName}");
                return false;
            }
            return channel.Send(packet);
        }

        /// <summary>
        /// 向远程主机发送消息包并注册回调
        /// </summary>
        /// <param name="channelName"></param>
        /// <param name="pack"></param>
        /// <param name="resHandler"></param>
        /// <returns></returns>
        public bool SendCsMsg(string channelName, TEngineProto.MainPack pack, CsMsgDelegate resHandler = null)
        {
            var channel = this.GetNetworkChannel(channelName);
            if (channel == null)
            {
                Log.Warning($"Channel is null :{channelName}");
                return false;
            }
            return channel.SendCsMsg(pack,resHandler);
        }
    }
}