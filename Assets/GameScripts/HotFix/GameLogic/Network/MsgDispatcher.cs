using System;
using System.Collections.Generic;
using GameProto;
using TEngine;
using CSPkg = GameProto.CSPkg;

namespace GameLogic
{
    internal class MsgHandleDataToRmv
    {
        public uint MsgId;
        public CsMsgDelegate Handle;
    };

    class MsgDispatcher
    {
        const int CheckTimeoutPerframe = 10;
        const int MaxMsgHandle = 256;

        private readonly CsMsgDelegate[] _aMsgHandles = new CsMsgDelegate[MaxMsgHandle];
        private readonly float[] _fMsgRegTime = new float[MaxMsgHandle];
        private readonly UInt32[] _adwMsgRegSeq = new UInt32[MaxMsgHandle]; //因为_aiMsgRegResCmdID存储的是hash，不能保证一定seqid一样，所以这儿存储下，用来校验
        private readonly uint[] _aiMsgRegResCmdID = new uint[MaxMsgHandle];

        UInt32 _dwLastCheckIndex = 0;

        private readonly Dictionary<uint, List<CsMsgDelegate>> _mapCmdHandle = new Dictionary<uint, List<CsMsgDelegate>>();
        private readonly List<CsMsgStatDelegate> _listStatHandle = new List<CsMsgStatDelegate>();

        //防止在处理消息的时候又删除了消息映射，所以这儿加了个队列来做个保护
        private readonly List<MsgHandleDataToRmv> _rmvList = new List<MsgHandleDataToRmv>();
        private bool _isInHandleLoop = false;
        private float _timeout = 5;

        // 清理所有的网络消息
        public void CleanAllNetMsg()
        {
            _mapCmdHandle.Clear();
        }

        public void SetTimeout(float timeout)
        {
            _timeout = timeout;
        }

        public void RegSeqHandle(UInt32 dwMsgSeqID, uint iResCmdID, CsMsgDelegate msgDelegate)
        {
            UInt32 hashIndex = dwMsgSeqID % MaxMsgHandle;
            if (_aMsgHandles[hashIndex] != null)
            {
                OnCallSeqHandle(_adwMsgRegSeq[hashIndex], _aiMsgRegResCmdID[hashIndex]);
                NotifyTimeout(_aMsgHandles[hashIndex]);
                RmvReg((int)hashIndex);
            }

            _aMsgHandles[hashIndex] = msgDelegate;
            _fMsgRegTime[hashIndex] = NowTime;
            _adwMsgRegSeq[hashIndex] = dwMsgSeqID;
            _aiMsgRegResCmdID[hashIndex] = iResCmdID;
        }

        public void RegCmdHandle(uint iCmdID, CsMsgDelegate msgDelegate)
        {
            if (!_mapCmdHandle.TryGetValue(iCmdID, out var listHandle))
            {
                listHandle = new List<CsMsgDelegate>();
                _mapCmdHandle[iCmdID] = listHandle;
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
            _listStatHandle.Add(handler);
        }

        public void DispatchCmdStat(int cmdID, int pkgSize)
        {
            foreach (CsMsgStatDelegate handle in _listStatHandle)
            {
                handle(cmdID, pkgSize);
            }
        }

        public void RmvCmdHandle(uint iCmdID, CsMsgDelegate msgDelegate)
        {
            if (_isInHandleLoop)
            {
                MsgHandleDataToRmv toRmvData = new MsgHandleDataToRmv();
                toRmvData.MsgId = iCmdID;
                toRmvData.Handle = msgDelegate;

                _rmvList.Add(toRmvData);
                return;
            }

            if (!_mapCmdHandle.TryGetValue(iCmdID, out var listHandle))
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
            if (_mapCmdHandle.TryGetValue(cmdID, out var listHandle))
            {
                _isInHandleLoop = true;

                var rmvList = _rmvList;
                rmvList.Clear();
                foreach (CsMsgDelegate handle in listHandle)
                {
                    ret = true;

                    TProfiler.BeginSample("handle");
                    handle(result, pkg);
                    TProfiler.EndSample();
                }

                _isInHandleLoop = false;

                //再统一删除掉
                int rmvCnt = rmvList.Count;
                for (int i = 0; i < rmvCnt; i++)
                {
                    var rmvItem = rmvList[i];
                    Log.Error("-------------remove cmd handle on loop:{0}-----------", rmvItem.MsgId);
                    RmvCmdHandle(rmvItem.MsgId, rmvItem.Handle);
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
            UInt32 hashIndex = dwSeqID % MaxMsgHandle;

            //先判断是否有注册的指定消息
            if (_aMsgHandles[hashIndex] != null &&
                _adwMsgRegSeq[hashIndex] == dwSeqID)
            {
                OnCallSeqHandle(dwSeqID, _aiMsgRegResCmdID[hashIndex]);
                _aMsgHandles[hashIndex](result, null);

                RmvReg((int)hashIndex);
            }
        }

        public bool IsCmdFilterNoLog(int cmdID)
        {
            switch (cmdID)
            {
                case (int)CSMsgID.CsCmdHeatbeatRes:
                    return true;
                default:
                    break;
            }
            return false;
        }

        public void NotifyMsg(CSPkg msg)
        {
            UInt32 dwSeq = msg.Head.Echo;
            UInt32 hashIndex = dwSeq % MaxMsgHandle;
            //判断是否有固定的消息处理流程
            bool bHaveHandle = NotifyCmdHandle(msg.Head.MsgId, CsMsgResult.NoError, msg);

            //再判断是否有注册的指定消息
            if (_aMsgHandles[hashIndex] != null &&
                _adwMsgRegSeq[hashIndex] == dwSeq &&
                _aiMsgRegResCmdID[hashIndex] == (int)msg.Head.MsgId)
            {
                OnCallSeqHandle(_adwMsgRegSeq[hashIndex], _aiMsgRegResCmdID[hashIndex]);
                _aMsgHandles[hashIndex](CsMsgResult.NoError, msg);
                RmvReg((int)hashIndex);
                bHaveHandle = true;
            }

            if (!bHaveHandle)
            {
                Log.Debug("there is no handle for Msg[{0}]", msg.Head.MsgId);
            }
        }

        private float NowTime => GameTime.unscaledTime;

        public void Update()
        {
            CheckTimeOut();
        }

        /// <summary>
        /// 定时检查是否请求超时。
        /// </summary>
        private void CheckTimeOut()
        {
            float timeout = _timeout;
            float nowTime = NowTime;
            for (int i = 0; i < CheckTimeoutPerframe; i++)
            {
                _dwLastCheckIndex = (_dwLastCheckIndex + 1) % MaxMsgHandle;
                if (_aMsgHandles[_dwLastCheckIndex] != null)
                {
                    if (_fMsgRegTime[_dwLastCheckIndex] + timeout < nowTime)
                    {
                        Log.Error("msg timeout, resCmdID[{0}], reqSeq[{1}]", _aiMsgRegResCmdID[_dwLastCheckIndex],
                            _adwMsgRegSeq[_dwLastCheckIndex]);

                        OnCallSeqHandle(_adwMsgRegSeq[_dwLastCheckIndex], _aiMsgRegResCmdID[_dwLastCheckIndex]);
                        NotifyTimeout(_aMsgHandles[_dwLastCheckIndex]);

                        RmvReg((int)_dwLastCheckIndex);
                    }
                }
            }
        }

        public void RmvReg(int index)
        {
            _aMsgHandles[index] = null;
            _adwMsgRegSeq[index] = 0;
            _aiMsgRegResCmdID[index] = 0;
            _fMsgRegTime[index] = 0;
        }
    }
}