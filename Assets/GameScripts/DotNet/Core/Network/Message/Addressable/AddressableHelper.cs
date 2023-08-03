#if TENGINE_NET

namespace TEngine.Core.Network
{
    public static class AddressableHelper
    {
        private static readonly List<SceneConfigInfo> AddressableScenes = new List<SceneConfigInfo>();

        static AddressableHelper()
        {
            var sceneConfigInfos = ConfigTableManage.AllSceneConfig();

            foreach (var sceneConfigInfo in sceneConfigInfos)
            {
                if (sceneConfigInfo.SceneTypeStr == "Addressable")
                {
                    AddressableScenes.Add(sceneConfigInfo);
                }
            }
        }

        public static async FTask AddAddressable(Scene scene, long addressableId, long routeId, bool isLock = true)
        {
            var addressableScene = AddressableScenes[(int)addressableId % AddressableScenes.Count];
            var response = await MessageHelper.CallInnerRoute(scene, addressableScene.EntityId,
                new I_AddressableAdd_Request
                {
                    AddressableId = addressableId, RouteId = routeId, IsLock = isLock
                });
            if (response.ErrorCode != 0)
            {
                Log.Error($"AddAddressable error is {response.ErrorCode}");
            }
        }

        public static async FTask<long> GetAddressableRouteId(Scene scene, long addressableId)
        {
            var addressableScene = AddressableScenes[(int)addressableId % AddressableScenes.Count];
            
            var response = (I_AddressableGet_Response) await MessageHelper.CallInnerRoute(scene, addressableScene.EntityId,
                new I_AddressableGet_Request
                {
                    AddressableId = addressableId
                });
            
            if (response.ErrorCode == 0)
            {
                return response.RouteId;
            }

            Log.Error($"GetAddressable error is {response.ErrorCode} addressableId:{addressableId}");
            return 0;
        }
        
        public static async FTask RemoveAddressable(Scene scene, long addressableId)
        {
            var addressableScene = AddressableScenes[(int)addressableId % AddressableScenes.Count];
            var response = await MessageHelper.CallInnerRoute(scene, addressableScene.EntityId,
                new I_AddressableRemove_Request
                {
                    AddressableId = addressableId
                });
            
            if (response.ErrorCode != 0)
            {
                Log.Error($"RemoveAddressable error is {response.ErrorCode}");
            }
        }
        
        public static async FTask LockAddressable(Scene scene, long addressableId)
        {
            var addressableScene = AddressableScenes[(int)addressableId % AddressableScenes.Count];
            var response = await MessageHelper.CallInnerRoute(scene, addressableScene.EntityId,
                new I_AddressableLock_Request
                {
                    AddressableId = addressableId
                });
            
            if (response.ErrorCode != 0)
            {
                Log.Error($"LockAddressable error is {response.ErrorCode}");
            }
        }
        
        public static async FTask UnLockAddressable(Scene scene, long addressableId, long routeId, string source)
        {
            var addressableScene = AddressableScenes[(int)addressableId % AddressableScenes.Count];
            var response = await MessageHelper.CallInnerRoute(scene, addressableScene.EntityId,
                new I_AddressableUnLock_Request
                {
                    AddressableId = addressableId,
                    RouteId = routeId,
                    Source = source
                });

            if (response.ErrorCode != 0)
            {
                Log.Error($"UnLockAddressable error is {response.ErrorCode}");
            }
        }
    }
}
#endif