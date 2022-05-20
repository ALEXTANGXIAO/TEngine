
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
        AddLogicSys(UISys.Instance);
    }

    protected override void StartGame()
    {
        UISys.Mgr.ShowWindow<TEngineUI>(true);

        UISys.Mgr.ShowWindow<MsgUI>(true);

        StartCoroutine(ActiveObjMgr());
    }

    IEnumerator ActiveObjMgr()
    {
        yield return new WaitForSeconds(2.0f);

        ObjMgr.Instance.Active();
    }

}

public class ObjMgr : TSingleton<ObjMgr>
{
    public override void Active()
    {
        //外部注入Update
        MonoManager.Instance.AddUpdateListener(Update);

        GameEventMgr.Instance.Send<string>(TipsEvent.Log, "WelCome To Use TEngine");
    }

    private void Update()
    {
        
    }
}
