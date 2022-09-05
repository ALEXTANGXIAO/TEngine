using UnityEngine;

namespace TEngine.Runtime.Entity
{
    internal sealed class AttachEntityInfo : IMemory
    {
        private Transform m_ParentTransform;
        private object m_UserData;

        public AttachEntityInfo()
        {
            m_ParentTransform = null;
            m_UserData = null;
        }

        public Transform ParentTransform
        {
            get { return m_ParentTransform; }
        }

        public object UserData
        {
            get { return m_UserData; }
        }

        public static AttachEntityInfo Create(Transform parentTransform, object userData)
        {
            AttachEntityInfo attachEntityInfo = MemoryPool.Acquire<AttachEntityInfo>();
            attachEntityInfo.m_ParentTransform = parentTransform;
            attachEntityInfo.m_UserData = userData;
            return attachEntityInfo;
        }

        public void Clear()
        {
            m_ParentTransform = null;
            m_UserData = null;
        }
    }
}