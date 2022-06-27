using System;
using TEngineCore.Net;

namespace TEngineCore
{
    public class TEngineDemo : TEngine
    {
        protected override void RegisterAllSystem()
        {
            base.RegisterAllSystem();
            AddLogicSys(UISys.Instance);
            AddLogicSys(DataCenterSys.Instance);
        }

        protected override void StartGame()
        {
            UnityEngine.Debug.Log("你好呀华佗".ToColor(ColorUtils.White));

            UISys.Mgr.ShowWindow<TEngineLoginUI>();

            GameClient.Instance.Connect("127.0.0.1", 54809, true);
        }

        public static Action<float> GetUpdateDelegate()
        {
            return Update;
        }

        public static void Update(float deltaTime)
        {
            UnityEngine.Debug.Log("HuaTuo热更域Update");
        }
    }
}