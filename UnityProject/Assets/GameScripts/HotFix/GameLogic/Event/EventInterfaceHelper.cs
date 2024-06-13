using System;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 事件接口帮助类。
    /// </summary>
    internal class EventInterfaceHelper
    {
        /// <summary>
        /// 初始化。
        /// </summary>
        public static void Init()
        {
            GameEvent.EventMgr.Init();
            RegisterEventInterface_Logic.Register(GameEvent.EventMgr);
            RegisterEventInterface_UI.Register(GameEvent.EventMgr);
        }
    }
}