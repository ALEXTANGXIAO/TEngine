#if TENGINE_NET
// ReSharper disable InconsistentNaming

namespace TEngine.Core.Network
{
    public interface IRouteMessageHandler
    {
        public Type Type();
        FTask Handle(Session session, Entity entity, uint rpcId, object routeMessage);
    }
    
    public abstract class Route<TEntity, TMessage> : IRouteMessageHandler where TEntity : Entity where TMessage : IRouteMessage
    {
        public Type Type()
        {
            return typeof(TMessage);
        }

        public async FTask Handle(Session session, Entity entity, uint rpcId, object routeMessage)
        {
            if (routeMessage is not TMessage ruteMessage)
            {
                Log.Error($"Message type conversion error: {routeMessage.GetType().FullName} to {typeof(TMessage).Name}");
                return;
            }

            if (entity is not TEntity tEntity)
            {
                Log.Error($"Route type conversion error: {entity.GetType().Name} to {nameof(TEntity)}");
                return;
            }

            try
            {
                await Run(tEntity, ruteMessage);
            }
            catch (Exception e)
            {
                if (entity is not Scene scene)
                {
                    scene = entity.Scene;
                }
                
                Log.Error($"SceneWorld:{session.Scene.World.Id} SceneRouteId:{scene.RouteId} SceneType:{scene.SceneInfo.SceneType} EntityId {tEntity.Id} : Error {e}");
            }
        }
        
        protected abstract FTask Run(TEntity entity, TMessage message);
    }
    
    public abstract class RouteRPC<TEntity, TRouteRequest, TRouteResponse> : IRouteMessageHandler where TEntity : Entity where TRouteRequest : IRouteRequest where TRouteResponse : IRouteResponse
    {
        public Type Type()
        {
            return typeof(TRouteRequest);
        }

        public async FTask Handle(Session session, Entity entity, uint rpcId, object routeMessage)
        {
            if (routeMessage is not TRouteRequest tRouteRequest)
            {
                Log.Error($"Message type conversion error: {routeMessage.GetType().FullName} to {typeof(TRouteRequest).Name}");
                return;
            }

            if (entity is not TEntity tEntity)
            {
                Log.Error($"Route type conversion error: {entity.GetType().Name} to {nameof(TEntity)}");
                return;
            }
            
            var isReply = false;
            var response = Activator.CreateInstance<TRouteResponse>();
            
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
                await Run(tEntity, tRouteRequest, response, Reply);
            }
            catch (Exception e)
            {
                if (entity is not Scene scene)
                {
                    scene = entity.Scene;
                }
                
                Log.Error($"SceneWorld:{session.Scene.World.Id} SceneRouteId:{scene.RouteId} SceneType:{scene.SceneInfo.SceneType} EntityId {tEntity.Id} : Error {e}");
                response.ErrorCode = CoreErrorCode.ErrRpcFail;
            }
            finally
            {
                Reply();
            }
        }
        
        protected abstract FTask Run(TEntity entity, TRouteRequest request, TRouteResponse response, Action reply);
    }
    
    public abstract class Addressable<TEntity, TMessage> : IRouteMessageHandler where TEntity : Entity where TMessage : IAddressableRouteMessage
    {
        public Type Type()
        {
            return typeof(TMessage);
        }

        public async FTask Handle(Session session, Entity entity, uint rpcId, object routeMessage)
        {
            if (routeMessage is not TMessage ruteMessage)
            {
                Log.Error($"Message type conversion error: {routeMessage.GetType().FullName} to {typeof(TMessage).Name}");
                return;
            }

            if (entity is not TEntity tEntity)
            {
                Log.Error($"Route type conversion error: {entity.GetType().Name} to {nameof(TEntity)}");
                return;
            }

            try
            {
                await Run(tEntity, ruteMessage);
            }
            catch (Exception e)
            {
                if (entity is not Scene scene)
                {
                    scene = entity.Scene;
                }
                
                Log.Error($"SceneWorld:{session.Scene.World.Id} SceneRouteId:{scene.RouteId} SceneType:{scene.SceneInfo.SceneType} EntityId {tEntity.Id} : Error {e}");
            }
            finally
            {
                session.Send(MessageDispatcherSystem.Instance.CreateRouteResponse(), rpcId);
            }
        }
        
        protected abstract FTask Run(TEntity entity, TMessage message);
    }
    
    public abstract class AddressableRPC<TEntity, TRouteRequest, TRouteResponse> : IRouteMessageHandler where TEntity : Entity where TRouteRequest : IAddressableRouteRequest where TRouteResponse : IAddressableRouteResponse
    {
        public Type Type()
        {
            return typeof(TRouteRequest);
        }

        public async FTask Handle(Session session, Entity entity, uint rpcId, object routeMessage)
        {
            if (routeMessage is not TRouteRequest tRouteRequest)
            {
                Log.Error($"Message type conversion error: {routeMessage.GetType().FullName} to {typeof(TRouteRequest).Name}");
                return;
            }

            if (entity is not TEntity tEntity)
            {
                Log.Error($"Route type conversion error: {entity.GetType().Name} to {nameof(TEntity)}");
                return;
            }
            
            var isReply = false;
            var response = Activator.CreateInstance<TRouteResponse>();
            
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
                await Run(tEntity, tRouteRequest, response, Reply);
            }
            catch (Exception e)
            {
                if (entity is not Scene scene)
                {
                    scene = entity.Scene;
                }
                
                Log.Error($"SceneWorld:{session.Scene.World.Id} SceneRouteId:{scene.RouteId} SceneType:{scene.SceneInfo.SceneType} EntityId {tEntity.Id} : Error {e}");
                response.ErrorCode = CoreErrorCode.ErrRpcFail;
            }
            finally
            {
                Reply();
            }
        }
        
        protected abstract FTask Run(TEntity entity, TRouteRequest request, TRouteResponse response, Action reply);
    }
}
#endif