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
        public NetworkManager NetworkManager;

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
            
            GameEventMgr.Instance.AddEventListener(TEngineEvent.OnStartGame,OnStartGame);
        }

        private void Update()
        {
            if (m_NetworkManager != null)
            {
                NetworkManager.Update(Time.deltaTime, Time.unscaledDeltaTime);;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (NetworkManager != null)
            {
                NetworkManager.Shutdown();
            }
        }

        private void OnStartGame()
        {
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
            
            if (NetEvent.Instance == null)
            {
                Log.Fatal("Event component is invalid.");
                return;
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

        private void OnNetworkConnected(object sender, NetworkConnectedEventArgs e)
        {
            NetEvent.Instance.Fire(this, e);
        }

        private void OnNetworkClosed(object sender, NetworkClosedEventArgs e)
        {
            NetEvent.Instance.Fire(this, e);
        }

        private void OnNetworkMissHeartBeat(object sender, NetworkMissHeartBeatEventArgs e)
        {
            NetEvent.Instance.Fire(this, e);
        }

        private void OnNetworkError(object sender, NetworkErrorEventArgs e)
        {
            NetEvent.Instance.Fire(this, e);
        }

        private void OnNetworkCustomError(object sender, NetworkCustomErrorEventArgs e)
        {
            NetEvent.Instance.Fire(this, e);
        }
    }
}