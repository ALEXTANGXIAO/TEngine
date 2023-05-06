using TEngine;
using UnityEngine;
#if ENABLE_URP
using UnityEngine.Rendering.Universal;
#endif

namespace GameLogic
{
    public class CameraUtils
    {
        public static void AddCameraStack(Camera camera,Camera mainCamera)
        {
#if ENABLE_URP
            if (mainCamera != null)
            {
                // 通过脚本的方式，只要能找到 camera 不轮是否跨 base 相机的场景，都可以 Add 进 Stack
                mainCamera.GetComponent<UniversalAdditionalCameraData>().cameraStack.Add(GameModule.UI.UICamera);
            }
#else
            Log.Fatal("Could not add camera stack because had no URP-Render-Pip");
#endif
        }
    }
}