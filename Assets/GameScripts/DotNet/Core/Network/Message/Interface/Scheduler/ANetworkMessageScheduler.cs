using System;
using System.IO;

namespace TEngine.Core.Network
{
    public abstract class ANetworkMessageScheduler
    {
        protected bool DisposePackInfo;
        private readonly PingResponse _pingResponse = new PingResponse();

        public async FTask Scheduler(Session session, APackInfo packInfo)
        {
            Type messageType = null;
            DisposePackInfo = true;

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
                Log.Error($"NetworkMessageScheduler error messageProtocolCode:{packInfo.ProtocolCode} messageType:{messageType} SessionId {session.Id} IsDispose {session.IsDisposed} {e}");
            }
            finally
            {
                if (DisposePackInfo)
                {
                    packInfo.Dispose();
                }
            }
        }

        public async FTask InnerScheduler(Session session, uint rpcId, long routeId, uint protocolCode, long routeTypeCode, object message)
        {
            var messageType = message.GetType();
            
            try
            {
                if (session.IsDisposed)
                {
                    return;
                }

                switch (protocolCode)
                {
                    case >= Opcode.OuterRouteMessage:
                    {
                        await InnerHandler(session, rpcId, routeId, protocolCode, routeTypeCode, messageType, message);
                        return;
                    }
                    case < Opcode.OuterResponse:
                    {
                        MessageDispatcherSystem.Instance.MessageHandler(session, messageType, message, rpcId, protocolCode);
                        return;
                    }
                    default:
                    {
#if TENGINE_NET
                        // 服务器之间发送消息因为走的是MessageHelper、所以接收消息的回调也应该放到MessageHelper里处理
                        MessageHelper.ResponseHandler(rpcId, (IResponse)message);
#endif
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"NetworkMessageScheduler error messageProtocolCode:{protocolCode} messageType:{messageType} SessionId {session.Id} IsDispose {session.IsDisposed} {e}");
            }
        }

        protected abstract FTask Handler(Session session, Type messageType, APackInfo packInfo);
        protected abstract FTask InnerHandler(Session session, uint rpcId, long routeId, uint protocolCode, long routeTypeCode, Type messageType, object message);
    }
}