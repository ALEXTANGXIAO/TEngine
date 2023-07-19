#if TENGINE_NET
using System;
using TEngine.Core.Network;
namespace TEngine
{
    public struct OnCreateScene
    {
        public readonly SceneConfigInfo SceneInfo;
        public readonly Action<Session> OnSetNetworkComplete;

        public OnCreateScene(SceneConfigInfo sceneInfo, Action<Session> onSetNetworkComplete)
        {
            SceneInfo = sceneInfo;
            OnSetNetworkComplete = onSetNetworkComplete;
        }
    }
}
#endif
