using System.Collections.Generic;

namespace TEngine.Runtime
{
    internal class DEventEntryData
    {
        public object m_interfaceWrap;
    };

    class DEventMgr
    {
        private DEventDispatcher m_dispatcher = new DEventDispatcher();

        /// <summary>
        /// 封装了调用的代理函数
        /// </summary>
        private Dictionary<string, DEventEntryData> m_entry = new Dictionary<string, DEventEntryData>();

        public T GetInterface<T>()
        {
            string typeName = typeof(T).FullName;
            DEventEntryData entry;
            if (m_entry.TryGetValue(typeName, out entry))
            {
                return (T)entry.m_interfaceWrap;
            }

            return default(T);
        }

        /// <summary>
        /// 注册wrap的函数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callerWrap"></param>
        public void RegWrapInterface<T>(T callerWrap)
        {
            string typeName = typeof(T).FullName;
            Log.Assert(!m_entry.ContainsKey(typeName));

            var entry = new DEventEntryData();
            entry.m_interfaceWrap = callerWrap;
            m_entry.Add(typeName, entry);
        }

        public DEventDispatcher GetDispatcher()
        {
            return m_dispatcher;
        }
    }
}