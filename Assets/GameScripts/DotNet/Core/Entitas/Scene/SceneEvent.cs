#if TENGINE_NET
using System;
using TEngine.Core.Network;
namespace TEngine
{
    public struct OnCreateScene
    {
        public readonly Scene Scene;

        public OnCreateScene(Scene scene)
        {
            Scene = scene;
        }
    }
}
#endif
