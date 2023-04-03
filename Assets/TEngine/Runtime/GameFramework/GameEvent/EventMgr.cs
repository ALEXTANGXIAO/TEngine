using System.Collections.Generic;

namespace TEngine
{
    /// <summary>
    /// 事件实体数据。
    /// </summary>
    internal class EventEntryData
    {
        public object InterfaceWrap;
    };

    /// <summary>
    /// 事件管理器。
    /// </summary>
    public class EventMgr
    {
        /// <summary>
        /// 封装了调用的代理函数
        /// </summary>
        private Dictionary<string, EventEntryData> m_entry = new Dictionary<string, EventEntryData>();

        /// <summary>
        /// 事件管理器获取接口。
        /// </summary>
        /// <typeparam name="T">接口类型。</typeparam>
        /// <returns>接口实例。</returns>
        public T GetInterface<T>()
        {
            string typeName = typeof(T).FullName;
            if (typeName != null && m_entry.TryGetValue(typeName, out var entry))
            {
                return (T)entry.InterfaceWrap;
            }
            return default(T);
        }

        /// <summary>
        /// 注册wrap的函数。
        /// </summary>
        /// <typeparam name="T">Wrap接口类型。</typeparam>
        /// <param name="callerWrap">callerWrap接口名字。</param>
        public void RegWrapInterface<T>(T callerWrap)
        {
            string typeName = typeof(T).FullName;
            var entry = new EventEntryData();
            entry.InterfaceWrap = callerWrap;
            if (typeName != null)
            {
                m_entry.Add(typeName, entry);
            }
        }

        /// <summary>
        /// 分发注册器。
        /// </summary>
        public EventDispatcher Dispatcher { get; } = new EventDispatcher();
    }
}