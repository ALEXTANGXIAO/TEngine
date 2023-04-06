using System;
using UnityEngine;

namespace TEngine
{
    [Serializable]
    public sealed class AudioGroupConfig
    {
        [SerializeField]
        private string m_Name = null;

        [SerializeField]
        private bool m_Mute = false;

        [SerializeField, Range(0f, 1f)]
        private float m_Volume = 1f;

        [SerializeField]
        private int m_AgentHelperCount = 1;

        public AudioType AudioType;

        public string Name
        {
            get
            {
                return m_Name;
            }
        }

        public bool Mute
        {
            get
            {
                return m_Mute;
            }
        }

        public float Volume
        {
            get
            {
                return m_Volume;
            }
        }

        public int AgentHelperCount
        {
            get
            {
                return m_AgentHelperCount;
            }
        }
    }
}