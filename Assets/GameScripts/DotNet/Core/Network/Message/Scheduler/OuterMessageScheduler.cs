using System;
using TEngine.IO;
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

namespace TEngine.Core.Network
{
#if TENGINE_UNITY
    public sealed class OuterMessageScheduler : ANetworkMessageScheduler
    {
        protected override FTask Handler(Session session, Type messageType, APackInfo packInfo)
        {
            throw new NotSupportedException($"Received unsupported message protocolCode:{packInfo.ProtocolCode} messageType:{messageType}");
        }
    }
#endif
#if TENGINE_NET
    public sealed class OuterMessageScheduler : ANetworkMessageScheduler
    {
        protected override async FTask Handler(Session session, Type messageType, APackInfo packInfo)
        {
            if (packInfo.ProtocolCode >= Opcode.InnerRouteMessage)
            {
                throw new NotSupportedException($"Received unsupported message protocolCode:{packInfo.ProtocolCode} messageType:{messageType}");
            }

            var packInfoMemoryStream = packInfo.MemoryStream;

            try
            {
                switch (packInfo.RouteTypeCode)
                {
                    case CoreRouteType.Route:
                    case CoreRouteType.BsonRoute:
                    {
                        break;
                    }
                    case CoreRouteType.Addressable:
                    {
                        var addressableRouteComponent = session.GetComponent<AddressableRouteComponent>();

                        if (addressableRouteComponent == null)
                        {
                            Log.Error("Session does not have an AddressableRouteComponent component");
                            return;
                        }

                        switch (packInfo.ProtocolCode)
                        {
                            case > Opcode.OuterRouteRequest:
                            {
                                var runtimeId = session.RuntimeId;
                                var response = await addressableRouteComponent.Call(packInfo.RouteTypeCode, messageType, packInfoMemoryStream);
                                
                                // session可能已经断开了，所以这里需要判断
                                
                                if (session.RuntimeId == runtimeId)
                                {
                                    session.Send(response, packInfo.RpcId);
                                }

                                return;
                            }
                            case > Opcode.OuterRouteMessage:
                            {
                                addressableRouteComponent.Send(packInfo.RouteTypeCode, messageType, packInfoMemoryStream);
                                return;
                            }
                        }

                        return;
                    }
                    case > CoreRouteType.CustomRouteType:
                    {
                        var routeComponent = session.GetComponent<RouteComponent>();

                        if (routeComponent == null)
                        {
                            Log.Error("Session does not have an routeComponent component");
                            return;
                        }

                        if (!routeComponent.TryGetRouteId(packInfo.RouteTypeCode, out var routeId))
                        {
                            Log.Error($"RouteComponent cannot find RouteId with RouteTypeCode {packInfo.RouteTypeCode}");
                            return;
                        }

                        switch (packInfo.ProtocolCode)
                        {
                            case > Opcode.OuterRouteRequest:
                            {
                                var runtimeId = session.RuntimeId;
                                var response = await MessageHelper.CallInnerRoute(session.Scene, routeId, packInfo.RouteTypeCode, messageType, packInfoMemoryStream);
                                // session可能已经断开了，所以这里需要判断
                                if (session.RuntimeId == runtimeId)
                                {
                                    session.Send(response, packInfo.RpcId);
                                }

                                return;
                            }
                            case > Opcode.OuterRouteMessage:
                            {
                                MessageHelper.SendInnerRoute(session.Scene, routeId, packInfo.RouteTypeCode, packInfoMemoryStream);
                                return;
                            }
                        }

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

            throw new NotSupportedException($"Received unsupported message protocolCode:{packInfo.ProtocolCode} messageType:{messageType}");
        }
    }
#endif
}