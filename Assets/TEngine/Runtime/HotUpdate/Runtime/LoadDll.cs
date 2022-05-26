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
#if !UNITY_EDITOR
        TextAsset dllBytes2 = TResources.Load<TextAsset>("DLL/TEngineHotUpdate.dll.bytes");
        //生产模式从bytes加载
        gameLogic = System.Reflection.Assembly.Load(dllBytes2.bytes);
#else
        //编辑器模式从dll反射加载
        gameLogic = AppDomain.CurrentDomain.GetAssemblies().First(assembly => assembly.GetName().Name == "TEngineHotUpdate");
#endif
    }

    private void RunMain()
    {
        if (gameLogic == null)
        {
            UnityEngine.Debug.LogError("DLL未加载");
            return;
        }

        AddMyComponent("TEngineCore.TEngineDemo", this.gameObject);

        //Type appType = gameLogic.GetType("TEngineCore.TEngine");
        //MethodInfo mainMethod = appType.GetMethod("InitLibImp");
        //mainMethod?.Invoke(null, null);

        //updateMethod = appType.GetMethod("Update");
        //var updateDel = System.Delegate.CreateDelegate(typeof(Action<float>), null, updateMethod);
    }

    void AddMyComponent(string className, GameObject obj)
    {
        Type type = gameLogic.GetType(className);
        obj.AddComponent(type);
    }


    void Update()
    {
        //if (gameLogic == null)
        //{
        //    return;
        //}
        //updateMethod?.Invoke(Time.deltaTime,null);
    }
}