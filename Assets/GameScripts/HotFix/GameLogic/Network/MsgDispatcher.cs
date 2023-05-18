using System;
using System.Collections.Generic;
using GameProto;
using TEngine;
using CSPkg = GameProto.CSPkg;

namespace GameLogic
{
    internal class MsgHandleDataToRmv
    {
        public uint m_msgCmd;
        public CsMsgDelegate m_handle;
    };

    class MsgDispatcher
    {
        const int CHECK_TIMEOUT_PERFRAME = 10;
        const int MAX_MSG_HANDLE = 256;

        CsMsgDelegate[] m_aMsgHandles = new CsMsgDelegate[MAX_MSG_HANDLE];
        float[] m_fMsgRegTime = new float[MAX_MSG_HANDLE];
        UInt32[] m_adwMsgRegSeq = new UInt32[MAX_MSG_HANDLE]; //因为m_aMsgHandles存储的是hash，不能保证一定seqid一样，所以这儿存储下，用来校验
        private uint[] m_aiMsgRegResCmdID = new uint[MAX_MSG_HANDLE];

        UInt32 m_dwLastCheckIndex = 0;

        Dictionary<uint, List<CsMsgDelegate>> m_mapCmdHandle = new Dictionary<uint, List<CsMsgDelegate>>();
        List<CsMsgStatDelegate> m_listStatHandle = new List<CsMsgStatDelegate>();

        //防止在处理消息的时候又删除了消息映射，所以这儿加了个队列来做个保护
        private List<MsgHandleDataToRmv> m_rmvList = new List<MsgHandleDataToRmv>();
        private bool m_isInHandleLoop = false;

        private float m_timeout = 5;

        // 清理所有的网络消息
        public void CleanAllNetMsg()
        {
            m_mapCmdHandle.Clear();
        }

        public void SetTimeout(float timeout)
        {
            m_timeout = timeout;
        }

        public void RegSeqHandle(UInt32 dwMsgSeqID, uint iResCmdID, CsMsgDelegate msgDelegate)
        {
            UInt32 hashIndex = dwMsgSeqID % MAX_MSG_HANDLE;
            if (m_aMsgHandles[hashIndex] != null)
            {
                OnCallSeqHandle(m_adwMsgRegSeq[hashIndex], m_aiMsgRegResCmdID[hashIndex]);
                NotifyTimeout(m_aMsgHandles[hashIndex]);
                RmvReg((int)hashIndex);
            }

            m_aMsgHandles[hashIndex] = msgDelegate;
            m_fMsgRegTime[hashIndex] = NowTime;
            m_adwMsgRegSeq[hashIndex] = dwMsgSeqID;
            m_aiMsgRegResCmdID[hashIndex] = iResCmdID;
        }

        public void RegCmdHandle(uint iCmdID, CsMsgDelegate msgDelegate)
        {
            List<CsMsgDelegate> listHandle;
            if (!m_mapCmdHandle.TryGetValue(iCmdID, out listHandle))
            {
                listHandle = new List<CsMsgDelegate>();
                m_mapCmdHandle[iCmdID] = listHandle;
            }

            if (listHandle != null)
            {
                if (listHandle.Contains(msgDelegate))
                {
                    Log.Error("-------------repeat RegCmdHandle:{0}-----------", iCmdID);
                }

                listHandle.Add(msgDelegate);
            }
        }

        /// <summary>
        /// 注册统计处理接口
        /// </summary>
        /// <param name="handler"></param>
        public void RegCmdStatHandle(CsMsgStatDelegate handler)
        {
            m_listStatHandle.Add(handler);
        }

        public void DispatchCmdStat(int cmdID, int pkgSize)
        {
            foreach (CsMsgStatDelegate handle in m_listStatHandle)
            {
                handle(cmdID, pkgSize);
            }
        }

        public void RmvCmdHandle(uint iCmdID, CsMsgDelegate msgDelegate)
        {
            if (m_isInHandleLoop)
            {
                MsgHandleDataToRmv toRmvData = new MsgHandleDataToRmv();
                toRmvData.m_msgCmd = iCmdID;
                toRmvData.m_handle = msgDelegate;

                m_rmvList.Add(toRmvData);
                return;
            }

            List<CsMsgDelegate> listHandle;
            if (!m_mapCmdHandle.TryGetValue(iCmdID, out listHandle))
            {
                return;
            }

            if (listHandle != null)
            {
                listHandle.Remove(msgDelegate);
            }
        }

        public void NotifyCmdError(uint iCmdID, CsMsgResult result)
        {
            NotifyCmdHandle(iCmdID, result, default(CSPkg));
        }

        protected bool NotifyCmdHandle(uint cmdID, CsMsgResult result, CSPkg pkg)
        {
            bool ret = false;
            if (m_mapCmdHandle.TryGetValue(cmdID, out var listHandle))
            {
                m_isInHandleLoop = true;

                var rmvList = m_rmvList;
                rmvList.Clear();
                foreach (CsMsgDelegate handle in listHandle)
                {
                    ret = true;

                    TProfiler.BeginSample("handle");
                    handle(result, pkg);
                    TProfiler.EndSample();
                }

                m_isInHandleLoop = false;

                //再统一删除掉
                int rmvCnt = rmvList.Count;
                for (int i = 0; i < rmvCnt; i++)
                {
                    var rmvItem = rmvList[i];
                    Log.Error("-------------remove cmd handle on loop:{0}-----------", rmvItem.m_msgCmd);
                    RmvCmdHandle(rmvItem.m_msgCmd, rmvItem.m_handle);
                }
            }

            return ret;
        }

        protected void OnCallSeqHandle(UInt32 echoSeq, uint resCmdID)
        {
            if (echoSeq > 0)
            {
                // TODO
                // GameEvent.Get<ICommUI>().FinWaitUI(echoSeq);
            }
        }

        protected void NotifyTimeout(CsMsgDelegate msgHandler)
        {
            msgHandler(CsMsgResult.MsgTimeOut, default(CSPkg));
        }

        public void NotifySeqError(UInt32 dwSeqID, CsMsgResult result)
        {
            UInt32 hashIndex = dwSeqID % MAX_MSG_HANDLE;

            //先判断是否有注册的指定消息
            if (m_aMsgHandles[hashIndex] != null &&
                m_adwMsgRegSeq[hashIndex] == dwSeqID)
            {
                OnCallSeqHandle(dwSeqID, m_aiMsgRegResCmdID[hashIndex]);
                m_aMsgHandles[hashIndex](result, null);

                RmvReg((int)hashIndex);
            }
        }

        public bool IsCmdFilterNoLog(int cmdID)
        {
            // TODO
            /*switch (cmdID)
            {
                case netMacros.CS_CMD_HEATBEAT_RES:
                    return true;
                default:
                    break;
            }*/

            return false;
        }

        public void NotifyMsg(CSPkg msg)
        {
            UInt32 dwSeq = msg.Head.Echo;
            UInt32 hashIndex = dwSeq % MAX_MSG_HANDLE;
            //判断是否有固定的消息处理流程
            bool bHaveHandle = NotifyCmdHandle(msg.Head.MsgId, CsMsgResult.NoError, msg);

            //再判断是否有注册的指定消息
            if (m_aMsgHandles[hashIndex] != null &&
                m_adwMsgRegSeq[hashIndex] == dwSeq &&
                m_aiMsgRegResCmdID[hashIndex] == (int)msg.Head.MsgId)
            {
                OnCallSeqHandle(m_adwMsgRegSeq[hashIndex], m_aiMsgRegResCmdID[hashIndex]);
                m_aMsgHandles[hashIndex](CsMsgResult.NoError, msg);
                RmvReg((int)hashIndex);
                bHaveHandle = true;
            }

            if (!bHaveHandle)
            {
                //todo..临时改为debug
                Log.Debug("there is no handle for Msg[{0}]", msg.Head.MsgId);
            }
        }

        private float NowTime => GameTime.unscaledTime;

        //定时检查是否请求超时了
        public void Update()
        {
            float timeout = m_timeout;
            float nowTime = NowTime;
            for (int i = 0; i < CHECK_TIMEOUT_PERFRAME; i++)
            {
                m_dwLastCheckIndex = (m_dwLastCheckIndex + 1) % MAX_MSG_HANDLE;
                if (m_aMsgHandles[m_dwLastCheckIndex] != null)
                {
                    if (m_fMsgRegTime[m_dwLastCheckIndex] + timeout < nowTime)
                    {
                        Log.Error("msg timeout, resCmdID[{0}], reqSeq[{1}]", m_aiMsgRegResCmdID[m_dwLastCheckIndex],
                            m_adwMsgRegSeq[m_dwLastCheckIndex]);

                        OnCallSeqHandle(m_adwMsgRegSeq[m_dwLastCheckIndex], m_aiMsgRegResCmdID[m_dwLastCheckIndex]);
                        NotifyTimeout(m_aMsgHandles[m_dwLastCheckIndex]);

                        RmvReg((int)m_dwLastCheckIndex);
                    }
                }
            }
        }

        public void RmvReg(int index)
        {
            m_aMsgHandles[index] = null;
            m_adwMsgRegSeq[index] = 0;
            m_aiMsgRegResCmdID[index] = 0;
            m_fMsgRegTime[index] = 0;
        }
    }
}