using System.Collections;
using System.Collections.Generic;
using TEngine.Runtime;
using UnityEngine;

namespace HotFix
{
    public class GameHotfixEntry
    {
        public static void Start()
        {
            Log.Fatal("HotFix.GameHotfixEntry");
            
            Log.Fatal("=======看到此条日志代表你成功运行了示例项目的热更新代码=======");
            MonoUtility.AddUpdateListener(Update);
            TResources.Load<GameObject>("Test/Cube.prefab");
        }

        private static void Update()
        {
            
        }
    }
}

