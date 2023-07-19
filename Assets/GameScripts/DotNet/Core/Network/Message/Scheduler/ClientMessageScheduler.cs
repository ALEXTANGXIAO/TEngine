using System;

namespace TEngine.Core.Network
{
#if TENGINE_UNITY
    public sealed class ClientMessageScheduler : ANetworkMessageScheduler
    {
        protected override async FTask Handler(Session session, Type messageType, APackInfo packInfo)
        {
            var packInfoMemoryStream = packInfo.MemoryStream;

            try
            {
                switch (packInfo.ProtocolCode)
                {
                    case > Opcode.InnerRouteResponse:
                    {
                        throw new NotSupportedException($"Received unsupported message protocolCode:{packInfo.ProtocolCode} messageType:{messageType}");
                    }
                    case Opcode.PingResponse:
                    case > Opcode.OuterRouteResponse:
                    {
                        // 这个一般是客户端Session.Call发送时使用的、目前这个逻辑只有Unity客户端时使用
                        var aResponse = (IResponse)packInfo.Deserialize(messageType);

                        if (!session.RequestCallback.TryGetValue(packInfo.RpcId, out var action))
                        {
                            Log.Error($"not found rpc {packInfo.RpcId}, response message: {aResponse.GetType().Name}");
                            return;
                        }
                        
                        session.RequestCallback.Remove(packInfo.RpcId);
                        action.SetResult(aResponse);
                        return;
                    }
                    case < Opcode.OuterRouteRequest:
                    {
                        var message = packInfo.Deserialize(messageType);
                        MessageDispatcherSystem.Instance.MessageHandler(session, messageType, message, packInfo.RpcId, packInfo.ProtocolCode);
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
                
                Log.Error(e);
                return;
            }

            await FTask.CompletedTask;
            throw new NotSupportedException($"Received unsupported message protocolCode:{packInfo.ProtocolCode} messageType:{messageType}");
        }
    }
#endif
#if TENGINE_NET
    public sealed class ClientMessageScheduler : ANetworkMessageScheduler
    {
        protected override FTask Handler(Session session, Type messageType, APackInfo packInfo)
        {
            throw new NotSupportedException($"Received unsupported message protocolCode:{packInfo.ProtocolCode} messageType:{messageType}");
        }
    }
#endif
}