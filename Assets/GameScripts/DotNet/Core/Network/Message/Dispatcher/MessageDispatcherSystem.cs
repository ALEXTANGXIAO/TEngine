using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TEngine.DataStructure;
using TEngine.Core;
using Type = System.Type;
#pragma warning disable CS8602
#pragma warning disable CS8600
#pragma warning disable CS8618

// ReSharper disable PossibleNullReferenceException

namespace TEngine.Core.Network
{
    public sealed class HandlerInfo<T>
    {
        public T Obj;
        public Type Type;
    }
    
    public sealed class MessageDispatcherSystem : Singleton<MessageDispatcherSystem>
    {
        private readonly Dictionary<Type, Type> _responseTypes = new Dictionary<Type, Type>();
        private readonly DoubleMapDictionary<uint, Type> _networkProtocols = new DoubleMapDictionary<uint, Type>();
        private readonly Dictionary<Type, IMessageHandler> _messageHandlers = new Dictionary<Type, IMessageHandler>();
        
        private readonly OneToManyList<int, Type> _assemblyResponseTypes = new OneToManyList<int, Type>();
        private readonly OneToManyList<int, uint> _assemblyNetworkProtocols = new OneToManyList<int, uint>();
        private readonly OneToManyList<int, HandlerInfo<IMessageHandler>> _assemblyMessageHandlers = new OneToManyList<int, HandlerInfo<IMessageHandler>>();

#if TENGINE_NET
        private readonly Dictionary<Type, IRouteMessageHandler> _routeMessageHandlers = new Dictionary<Type, IRouteMessageHandler>();
        private readonly OneToManyList<int, HandlerInfo<IRouteMessageHandler>> _assemblyRouteMessageHandlers= new OneToManyList<int, HandlerInfo<IRouteMessageHandler>>();
#endif
        private static readonly CoroutineLockQueueType ReceiveRouteMessageLock = new CoroutineLockQueueType("ReceiveRouteMessageLock");
        
#if TENGINE_UNITY

        public readonly Dictionary<uint, List<Action<IResponse>>> MsgHandles = new Dictionary<uint, List<Action<IResponse>>>();

        public void RegisterMsgHandler(uint protocolCode,Action<IResponse> ctx)
        {
            if (!MsgHandles.ContainsKey(protocolCode))
            {
                MsgHandles[protocolCode] = new List<Action<IResponse>>();
            }
            MsgHandles[protocolCode].Add(ctx);
        }
        
        public void UnRegisterMsgHandler(uint protocolCode,Action<IResponse> ctx)
        {
            if (MsgHandles.TryGetValue(protocolCode, out var handle))
            {
                handle.Remove(ctx);
            }
        }
#endif
        
        protected override void OnLoad(int assemblyName)
        {
            foreach (var type in AssemblyManager.ForEach(assemblyName, typeof(IMessage)))
            {
                var obj = (IMessage) Activator.CreateInstance(type);
                var opCode = obj.OpCode();
                
                _networkProtocols.Add(opCode, type);

                var responseType = type.GetProperty("ResponseType");

                if (responseType != null)
                {
                    _responseTypes.Add(type, responseType.PropertyType);
                    _assemblyResponseTypes.Add(assemblyName, type);
                }

                _assemblyNetworkProtocols.Add(assemblyName, opCode);
            }

            foreach (var type in AssemblyManager.ForEach(assemblyName, typeof(IMessageHandler)))
            {
                var obj = (IMessageHandler) Activator.CreateInstance(type);

                if (obj == null)
                {
                    throw new Exception($"message handle {type.Name} is null");
                }

                var key = obj.Type();
                _messageHandlers.Add(key, obj);
                _assemblyMessageHandlers.Add(assemblyName, new HandlerInfo<IMessageHandler>()
                {
                    Obj = obj, Type = key
                });
            }
#if TENGINE_NET
            foreach (var type in AssemblyManager.ForEach(assemblyName, typeof(IRouteMessageHandler)))
            {
                var obj = (IRouteMessageHandler) Activator.CreateInstance(type);

                if (obj == null)
                {
                    throw new Exception($"message handle {type.Name} is null");
                }

                var key = obj.Type();
                _routeMessageHandlers.Add(key, obj);
                _assemblyRouteMessageHandlers.Add(assemblyName, new HandlerInfo<IRouteMessageHandler>()
                {
                    Obj = obj, Type = key
                });
            }
#endif
        }

        protected override void OnUnLoad(int assemblyName)
        {
            if (_assemblyResponseTypes.TryGetValue(assemblyName, out var removeResponseTypes))
            {
                foreach (var removeResponseType in removeResponseTypes)
                {
                    _responseTypes.Remove(removeResponseType);
                }
                
                _assemblyResponseTypes.RemoveByKey(assemblyName);
            }

            if (_assemblyNetworkProtocols.TryGetValue(assemblyName, out var removeNetworkProtocols))
            {
                foreach (var removeNetworkProtocol in removeNetworkProtocols)
                {
                    _networkProtocols.RemoveByKey(removeNetworkProtocol);
                }

                _assemblyNetworkProtocols.RemoveByKey(assemblyName);
            }

            if (_assemblyMessageHandlers.TryGetValue(assemblyName, out var removeMessageHandlers))
            {
                foreach (var removeMessageHandler in removeMessageHandlers)
                {
                    _messageHandlers.Remove(removeMessageHandler.Type);
                }
               
                _assemblyMessageHandlers.Remove(assemblyName);
            }
#if TENGINE_NET
            if (_assemblyRouteMessageHandlers.TryGetValue(assemblyName, out var removeRouteMessageHandlers))
            {
                foreach (var removeRouteMessageHandler in removeRouteMessageHandlers)
                {
                    _routeMessageHandlers.Remove(removeRouteMessageHandler.Type);
                }
                
                _assemblyRouteMessageHandlers.Remove(assemblyName);
            }
#endif
        }

        public void MessageHandler(Session session, Type type, object message, uint rpcId, uint protocolCode)
        {
            if (!_messageHandlers.TryGetValue(type, out var messageHandler))
            {
                Log.Warning($"Scene:{session.Scene.Id} Found Unhandled Message: {message.GetType()}");
                return;
            }
            
            messageHandler.Handle(session, rpcId, protocolCode, message).Coroutine();
        }
#if TENGINE_NET
        public async FTask RouteMessageHandler(Session session, Type type, Entity entity, object message, uint rpcId)
        {
            if (!_routeMessageHandlers.TryGetValue(type, out var routeMessageHandler))
            {
                Log.Warning($"Scene:{session.Scene.Id} Found Unhandled RouteMessage: {message.GetType()}");

                if (message is IRouteRequest request)
                {
                    FailResponse(session, request, CoreErrorCode.Error_NotFindEntity, rpcId);
                }

                return;
            }
            
            var runtimeId = entity.RuntimeId;
            var sessionRuntimeId = session.RuntimeId;

            if (entity is Scene)
            {
                // 如果是Scene的话、就不要加锁了、如果加锁很一不小心就可能会造成死锁
                await routeMessageHandler.Handle(session, entity, rpcId, message);
                return;
            }

            using (await ReceiveRouteMessageLock.Lock(runtimeId))
            {
                if (sessionRuntimeId != session.RuntimeId)
                {
                    return;
                }
                
                if (runtimeId != entity.RuntimeId)
                {
                    if (message is IRouteRequest request)
                    {
                        FailResponse(session, request, CoreErrorCode.Error_NotFindEntity, rpcId);
                    }
                
                    return;
                }
                
                await routeMessageHandler.Handle(session, entity, rpcId, message);
            }
        }
#endif
        public void FailResponse(Session session, IRouteRequest iRouteRequest, int error, uint rpcId)
        {
            var response = CreateResponse(iRouteRequest, error);
            session.Send(response, rpcId);
        }

        public IRouteResponse CreateRouteResponse()
        {
            return new RouteResponse();
        }
        
        public IResponse CreateResponse(Type requestType, int error)
        {
            IResponse response;

            if (_responseTypes.TryGetValue(requestType, out var responseType))
            {
                response = (IResponse) Activator.CreateInstance(responseType);
            }
            else
            {
                response = new Response();
            }

            response.ErrorCode = error;
            return response;
        }

        public IResponse CreateResponse(IRequest iRequest, int error)
        {
            IResponse response;

            if (_responseTypes.TryGetValue(iRequest.GetType(), out var responseType))
            {
                response = (IResponse) Activator.CreateInstance(responseType);
            }
            else
            {
                response = new Response();
            }

            response.ErrorCode = error;
            return response;
        }

        public IRouteResponse CreateResponse(IRouteRequest iRouteRequest, int error)
        {
            IRouteResponse response;

            if (_responseTypes.TryGetValue(iRouteRequest.GetType(), out var responseType))
            {
                response = (IRouteResponse) Activator.CreateInstance(responseType);
            }
            else
            {
                response = new RouteResponse();
            }

            response.ErrorCode = error;
            return response;
        }

        public uint GetOpCode(Type type)
        {
            return _networkProtocols.GetKeyByValue(type);
        }
        
        public Type GetOpCodeType(uint code)
        {
            return _networkProtocols.GetValueByKey(code);
        }
    }
}