using System;
using GameProto;

namespace GameLogic
{
    public class ProtoUtil
    {
        public static UInt32 MsgEcho = 0;
        
        /// <summary>
        /// 根据msgId来生成一个数据包
        /// </summary>
        /// <param name="msgId"></param>
        /// <returns></returns>
        public static CSPkg BuildCsMsg(int msgId)
        {
            CSPkg tmp = new CSPkg();
            tmp.Head = new CSPkgHead();
            tmp.Head.MsgId = (UInt16)msgId;
            tmp.Head.Echo = GetNextEcho();
            // tmp.Body.create(msgId);
            return tmp;
        }
        
        private static UInt32 GetNextEcho()
        {
            return ++MsgEcho;
        }
    }
}