namespace TEngine.Runtime.Entity
{
    internal sealed partial class EntityManager
    {
        private sealed class ShowEntityInfo : IMemory
        {
            private int m_SerialId;
            private int m_EntityId;
            private EntityGroup m_EntityGroup;
            private object m_UserData;

            public ShowEntityInfo()
            {
                m_SerialId = 0;
                m_EntityId = 0;
                m_EntityGroup = null;
                m_UserData = null;
            }

            public int SerialId
            {
                get { return m_SerialId; }
            }

            public int EntityId
            {
                get { return m_EntityId; }
            }

            public EntityGroup EntityGroup
            {
                get { return m_EntityGroup; }
            }

            public object UserData
            {
                get { return m_UserData; }
            }

            public static ShowEntityInfo Create(int serialId, int entityId, EntityGroup entityGroup, object userData)
            {
                ShowEntityInfo showEntityInfo = MemoryPool.Acquire<ShowEntityInfo>();
                showEntityInfo.m_SerialId = serialId;
                showEntityInfo.m_EntityId = entityId;
                showEntityInfo.m_EntityGroup = entityGroup;
                showEntityInfo.m_UserData = userData;
                return showEntityInfo;
            }

            public void Clear()
            {
                m_SerialId = 0;
                m_EntityId = 0;
                m_EntityGroup = null;
                m_UserData = null;
            }
        }
    }
}