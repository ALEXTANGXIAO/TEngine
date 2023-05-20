using System.Collections.Generic;
using System.Net.Sockets;
using GameBase;
using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// 网络组件。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Network : UnitySingleton<Network>
    {
        private NetworkManager m_NetworkManager = null;

        /// <summary>
        /// 获取网络频道数量。
        /// </summary>
        public int NetworkChannelCount => m_NetworkManager.NetworkChannelCount;

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            // m_NetworkManager = GameFrameworkEntry.GetModule<INetworkManager>();
            m_NetworkManager = new NetworkManager();
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

        private void Update()
        {
            m_NetworkManager.Update(GameTime.deltaTime, GameTime.unscaledDeltaTime);
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
        public INetworkChannel[] StaticGetAllNetworkChannels()
        {
            return m_NetworkManager.GetAllNetworkChannels();
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
        public INetworkChannel CreateNetworkChannel(string name, ServiceType serviceType,
            INetworkChannelHelper networkChannelHelper)
        {
            return m_NetworkManager.CreateNetworkChannel(name, serviceType, networkChannelHelper);
        }

        /// <summary>
        /// 销毁网络频道。
        /// </summary>
        /// <param name="channelName">网络频道名称。</param>
        /// <returns>是否销毁网络频道成功。</returns>
        public bool DestroyNetworkChannel(string channelName)
        {
            return m_NetworkManager.DestroyNetworkChannel(channelName);
        }

        private void OnNetworkConnected(INetworkChannel channel, object userdata)
        {
            GameEvent.Send(NetworkEvent.NetworkConnectedEvent, channel, userdata);
        }

        private void OnNetworkClosed(INetworkChannel channel)
        {
            GameEvent.Send(NetworkEvent.NetworkClosedEvent, channel);
        }

        private void OnNetworkMissHeartBeat(INetworkChannel channel, int missCount)
        {
            GameEvent.Send(NetworkEvent.NetworkMissHeartBeatEvent, channel, missCount);
        }

        private void OnNetworkError(INetworkChannel channel, NetworkErrorCode networkErrorCode, SocketError socketError,
            string errorMessage)
        {
            GameEvent.Send(NetworkEvent.NetworkErrorEvent, channel, networkErrorCode, socketError, errorMessage);
        }

        private void OnNetworkCustomError(INetworkChannel channel, object userData)
        {
            GameEvent.Send(NetworkEvent.NetworkCustomErrorEvent, channel, userData);
        }
    }
}