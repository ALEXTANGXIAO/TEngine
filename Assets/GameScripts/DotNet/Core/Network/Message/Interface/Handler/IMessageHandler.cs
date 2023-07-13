// ReSharper disable InconsistentNaming

using System;

namespace TEngine.Core.Network
{
    public interface IMessageHandler
    {
        public Type Type();
        FTask Handle(Session session, uint rpcId, uint messageTypeCode, object message);
    }
    
    public abstract class Message<T> : IMessageHandler
    {
        public Type Type()
        {
            return typeof(T);
        }

        public async FTask Handle(Session session, uint rpcId, uint messageTypeCode, object message)
        {
            try
            {
                await Run(session, (T) message);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        
        protected abstract FTask Run(Session session, T message);
    }
    
    public abstract class MessageRPC<TRequest, TResponse> : IMessageHandler where TRequest : IRequest where TResponse : IResponse
    {
        public Type Type()
        {
            return typeof(TRequest);
        }

        public async FTask Handle(Session session, uint rpcId, uint messageTypeCode, object message)
        {
            if (message is not TRequest request)
            {
                Log.Error($"消息类型转换错误: {message.GetType().Name} to {typeof(TRequest).Name}");
                return;
            }
            
            var response = Activator.CreateInstance<TResponse>();
            var isReply = false;

            void Reply()
            {
                if (isReply)
                {
                    return;
                }

                isReply = true;

                if (session.IsDisposed)
                {
                    return;
                }

                session.Send(response, rpcId);
            }

            try
            {
                await Run(session, request, response, Reply);
            }
            catch (Exception e)
            {
                Log.Error(e);
                response.ErrorCode = CoreErrorCode.ErrRpcFail;
            }
            finally
            {
                Reply();
            }
        }

        protected abstract FTask Run(Session session, TRequest request, TResponse response, Action reply);
    }
}