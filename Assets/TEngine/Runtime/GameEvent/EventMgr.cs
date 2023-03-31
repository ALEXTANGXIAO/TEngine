using System.Collections.Generic;

namespace TEngine
{
    internal class EventEntryData
    {
        public object InterfaceWrap;
    };

    class EventMgr
    {
        private EventDispatcher m_dispatcher = new EventDispatcher();

        /// <summary>
        /// 封装了调用的代理函数
        /// </summary>
        private Dictionary<string, EventEntryData> m_entry = new Dictionary<string, EventEntryData>();

        public T GetInterface<T>()
        {
            string typeName = typeof(T).FullName;
            EventEntryData entry;
            if (m_entry.TryGetValue(typeName, out entry))
            {
                return (T)entry.InterfaceWrap;
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
            
            var entry = new EventEntryData();
            entry.InterfaceWrap = callerWrap;
            m_entry.Add(typeName, entry);
        }

        public EventDispatcher GetDispatcher()
        {
            return m_dispatcher;
        }
    }
}