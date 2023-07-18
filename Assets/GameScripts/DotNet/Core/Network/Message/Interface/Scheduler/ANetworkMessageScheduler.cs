using System;
using System.IO;
using TEngine.Core;
#pragma warning disable CS8600

namespace TEngine.Core.Network
{
    public abstract class ANetworkMessageScheduler
    {
        private readonly PingResponse _pingResponse = new PingResponse();

        public async FTask Scheduler(Session session, APackInfo packInfo)
        {
            Type messageType = null;
            var packInfoMemoryStream = packInfo.MemoryStream;

            try
            {
                if (session.IsDisposed)
                {
                    return;
                }

                if (packInfo.ProtocolCode == Opcode.PingRequest)
                {
                    _pingResponse.Now = TimeHelper.Now;
                    session.Send(_pingResponse, packInfo.RpcId);
                    return;
                }

                messageType = MessageDispatcherSystem.Instance.GetOpCodeType(packInfo.ProtocolCode);

                if (messageType == null)
                {
                    throw new Exception($"可能遭受到恶意发包或没有协议定义ProtocolCode ProtocolCode：{packInfo.ProtocolCode}");
                }

                switch (packInfo.ProtocolCode)
                {
                    case Opcode.PingResponse:
                    case >= Opcode.OuterRouteMessage:
                    {
                        await Handler(session, messageType, packInfo);
                        return;
                    }
                    case < Opcode.OuterResponse:
                    {
                        var message = packInfo.Deserialize(messageType);
                        MessageDispatcherSystem.Instance.MessageHandler(session, messageType, message, packInfo.RpcId, packInfo.ProtocolCode);
                        return;
                    }
                    default:
                    {
                        var aResponse = (IResponse)packInfo.Deserialize(messageType);
#if TENGINE_NET
                        // 服务器之间发送消息因为走的是MessageHelper、所以接收消息的回调也应该放到MessageHelper里处理
                        MessageHelper.ResponseHandler(packInfo.RpcId, aResponse);
#else

#if TENGINE_UNITY
                        if (MessageDispatcherSystem.Instance.MsgHandles.TryGetValue(packInfo.ProtocolCode,out var msgDelegates))
                        {
                            foreach (var msgDelegate in msgDelegates)
                            {
                                msgDelegate.Invoke(aResponse);
                            }
                            return;
                        }       
#endif
                        // 这个一般是客户端Session.Call发送时使用的、目前这个逻辑只有Unity客户端时使用
                        
                        if (!session.RequestCallback.TryGetValue(packInfo.RpcId, out var action))
                        {
                            Log.Error($"not found rpc {packInfo.RpcId}, response message: {aResponse.GetType().Name}");
                            return;
                        }
                        
                        session.RequestCallback.Remove(packInfo.RpcId);
                        action.SetResult(aResponse);
#endif
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                if (packInfoMemoryStream.CanRead)
                {
                    // ReSharper disable once MethodHasAsyncOverload
                    packInfoMemoryStream.Dispose();
                }
                
                Log.Error($"NetworkMessageScheduler error messageProtocolCode:{packInfo.ProtocolCode} messageType:{messageType} SessionId {session.Id} IsDispose {session.IsDisposed} {e}");
            }
            finally
            {
                packInfo.Dispose();
            }
        }
        
        protected abstract FTask Handler(Session session, Type messageType, APackInfo packInfo);
    }
}