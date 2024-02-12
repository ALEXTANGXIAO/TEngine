using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    public class Module01 : BaseLogicSys<Module01>
    {
        GameObject cube;
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Log.Info("Module01 OnUpdate: Escape");
                ModuleSystem.Shutdown(TEngine.ShutdownType.Quit);
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                if (!cube)
                {
                    var c = GameModule.Resource.LoadAsset<GameObject>("Assets/AssetRaw/Prefabs/Cube.prefab", false);
                    cube = GameObject.Instantiate(c);
                }
                cube.transform.position = Random.insideUnitSphere;
                cube.transform.rotation = Random.rotation;
            }
        }
    }
}
