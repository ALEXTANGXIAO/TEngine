#if TENGINE_NET
namespace TEngine.Core.Network
{
    /// <summary>
    /// 可寻址消息组件、挂载了这个组件可以接收Addressable消息
    /// </summary>
    public sealed class AddressableMessageComponent : Entity
    {
        public long AddressableId;
        
        public override void Dispose()
        {
            if (AddressableId != 0)
            {
                AddressableHelper.RemoveAddressable(Scene, AddressableId).Coroutine();
                AddressableId = 0;
            }
            
            base.Dispose();
        }

        public FTask Register()
        {
            if (Parent == null)
            {
                throw new Exception("AddressableRouteComponent must be mounted under a component");
            }

            AddressableId = Parent.Id;
            
            if (AddressableId == 0)
            {
                throw new Exception("AddressableRouteComponent.Parent.Id is null");
            }

#if TENGINE_DEVELOP
            Log.Debug($"AddressableMessageComponent Register addressableId:{AddressableId} RouteId:{Parent.RuntimeId}");
#endif
            return AddressableHelper.AddAddressable(Scene, AddressableId, Parent.RuntimeId);
        }

        public FTask Lock()
        {
#if TENGINE_DEVELOP
            Log.Debug($"AddressableMessageComponent Lock {Parent.Id}");
#endif
            return AddressableHelper.LockAddressable(Scene, Parent.Id);
        }

        public FTask UnLock(string source)
        {
#if TENGINE_DEVELOP
            Log.Debug($"AddressableMessageComponent UnLock {Parent.Id} {Parent.RuntimeId}");
#endif
            return AddressableHelper.UnLockAddressable(Scene, Parent.Id, Parent.RuntimeId, source);
        }
    }
}
#endif