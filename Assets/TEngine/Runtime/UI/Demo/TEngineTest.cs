
using System.Collections;
using TEngine;
using UI;
using UnityEngine;

public class TEngineTest : TEngine.TEngine
{
    /// <summary>
    /// 注册系统
    /// </summary>
    protected override void RegisterAllSystem()
    {
        base.RegisterAllSystem();
        //注册系统，例如UI系统，网络系统，战斗系统等等
        AddLogicSys(BehaviourSingleSystem.Instance);
        AddLogicSys(UISys.Instance);
    }

    protected override void StartGame()
    {
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        yield return new WaitForSeconds(2.0f);

        ObjMgr.Instance.Active();

        UISys.Mgr.ShowWindow<TEngineUI>(true);

        UISys.Mgr.ShowWindow<MsgUI>(true);
    }

}

public class ObjMgr : TSingleton<ObjMgr>
{
    public override void Active()
    {
        //外部注入Update
        MonoUtility.AddUpdateListener(Update);

        GameEventMgr.Instance.Send<string>(TipsEvent.Log, "WelCome To Use TEngine");
    }

    private void Update()
    {
        
    }
}
