// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
#if TENGINE_NET

namespace TEngine.Core.Network
{
    public sealed class InnerMessageScheduler : ANetworkMessageScheduler
    {
        protected override async FTask Handler(Session session, Type messageType, APackInfo packInfo)
        {
            var disposeMemoryStream = true;
            var packInfoMemoryStream = packInfo.MemoryStream;
            
            try
            {
                switch (packInfo.ProtocolCode)
                {
                    case >= Opcode.InnerBsonRouteResponse:
                    case >= Opcode.InnerRouteResponse:
                    {
                        var response = (IRouteResponse)packInfo.Deserialize(messageType);
                        MessageHelper.ResponseHandler(packInfo.RpcId, response);
                        return;
                    }
                    case >= Opcode.OuterRouteResponse:
                    {
                        // 如果Gate服务器、需要转发Addressable协议、所以这里有可能会接收到该类型协议
                        var aResponse = (IResponse)packInfo.Deserialize(messageType);
                        MessageHelper.ResponseHandler(packInfo.RpcId, aResponse);
                        return;
                    }
                    case > Opcode.InnerBsonRouteMessage:
                    {
                        var obj = packInfo.Deserialize(messageType);
                        var entity = Entity.GetEntity(packInfo.RouteId);

                        if (entity == null)
                        {
                            if (packInfo.ProtocolCode > Opcode.InnerBsonRouteRequest)
                            {
                                MessageDispatcherSystem.Instance.FailResponse(session, (IRouteRequest)obj, CoreErrorCode.ErrNotFoundRoute, packInfo.RpcId);
                            }

                            return;
                        }

                        await MessageDispatcherSystem.Instance.RouteMessageHandler(session, messageType, entity, obj, packInfo.RpcId);
                        return;
                    }
                    case > Opcode.InnerRouteMessage:
                    {
                        var obj = packInfo.Deserialize(messageType);
                        var entity = Entity.GetEntity(packInfo.RouteId);

                        if (entity == null)
                        {
                            if (packInfo.ProtocolCode > Opcode.InnerRouteRequest)
                            {
                                MessageDispatcherSystem.Instance.FailResponse(session, (IRouteRequest)obj, CoreErrorCode.ErrNotFoundRoute, packInfo.RpcId);
                            }

                            return;
                        }

                        await MessageDispatcherSystem.Instance.RouteMessageHandler(session, messageType, entity, obj, packInfo.RpcId);
                        return;
                    }
                    case > Opcode.OuterRouteMessage:
                    {
                        var entity = Entity.GetEntity(packInfo.RouteId);

                        switch (entity)
                        {
                            case null:
                            {
                                var obj = packInfo.Deserialize(messageType);
                                var response = MessageDispatcherSystem.Instance.CreateResponse((IRouteMessage)obj, CoreErrorCode.ErrNotFoundRoute);
                                session.Send(response, packInfo.RpcId, packInfo.RouteId);
                                return;
                            }
                            case Session gateSession:
                            {
                                // 这里如果是Session只可能是Gate的Session、如果是的话、肯定是转发Address消息
                                disposeMemoryStream = false;
                                gateSession.Send(packInfoMemoryStream, packInfo.RpcId);
                                return;
                            }
                            default:
                            {
                                var obj = packInfo.Deserialize(messageType);
                                await MessageDispatcherSystem.Instance.RouteMessageHandler(session, messageType, entity, obj, packInfo.RpcId);
                                return;
                            }
                        }
                    }
                    default:
                    {
                        throw new NotSupportedException($"Received unsupported message protocolCode:{packInfo.ProtocolCode} messageType:{messageType}");
                    }
                }
            }
            catch (Exception e)
            {
                if (disposeMemoryStream && packInfoMemoryStream.CanRead)
                {
                    // ReSharper disable once MethodHasAsyncOverload
                    packInfoMemoryStream.Dispose();
                }
                
                Log.Error($"InnerMessageSchedulerHandler error messageProtocolCode:{packInfo.ProtocolCode} messageType:{messageType} {e}");
            }
        }
    }
}
#endif

