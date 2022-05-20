using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TEngine;
using UnityEngine;

public class LoadDll : MonoBehaviour
{
    private System.Reflection.Assembly gameLogic;
    private MethodInfo updateMethod;

    void Start()
    {
        LoadGameDll();
        RunMain();
    }

    private void LoadGameDll()
    {

#if UNITY_EDITOR
        TextAsset dllBytes2 = TResources.Load<TextAsset>("DLL/GameLogic.dll.bytes");
        gameLogic = System.Reflection.Assembly.Load(dllBytes2.bytes);
#else
        gameLogic = AppDomain.CurrentDomain.GetAssemblies().First(assembly => assembly.GetName().Name == "GameLogic");
#endif

    }

    private void RunMain()
    {
        if (gameLogic == null)
        {
            UnityEngine.Debug.LogError("DLL未加载");
            return;
        }

        Type appType = gameLogic.GetType("GameMain");
        MethodInfo mainMethod = appType.GetMethod("RunMain");
        mainMethod?.Invoke(null, null);

        updateMethod = appType.GetMethod("Update");
        var updateDel = System.Delegate.CreateDelegate(typeof(Action<float>), null, updateMethod);
    }

    void Update()
    {
        if (gameLogic == null)
        {
            return;
        }
        updateMethod?.Invoke(Time.deltaTime,null);
    }
}
