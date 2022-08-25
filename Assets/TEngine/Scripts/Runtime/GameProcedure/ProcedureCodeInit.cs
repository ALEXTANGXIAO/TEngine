using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TEngine.Runtime
{
    /// <summary>
    /// 流程加载器 - 代码初始化
    /// </summary>
    public class ProcedureCodeInit : ProcedureBase
    {
        /// <summary>
        /// 是否需要加载热更新DLL
        /// </summary>
        public bool NeedLoadDll = false;

        private IFsm<IProcedureManager> m_procedureOwner;

        protected internal override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_procedureOwner = procedureOwner;

            if (!NeedLoadDll)
            {
                ChangeState<ProcedureStartGame>(procedureOwner);
                return;
            }

#if UNITY_EDITOR
            Assembly hotfixAssembly = System.AppDomain.CurrentDomain.GetAssemblies().First(assembly => assembly.GetName().Name == "Game.Hotfix");
            StartHotfix(hotfixAssembly);
#else
            TResources.LoadAsync("Assets/Game/Hotfix/Game.Hotfix.dll.bytes", (data =>
            {
                if (data == null)
                {
                    OnLoadAssetFail();
                    return;
                }
                var obj = data.AssetObject as TextAsset;

                if (obj == null)
                {
                    OnLoadAssetFail();
                    return;
                }
                else
                {
                    OnLoadAssetSuccess(obj);
                }
            }));
#endif
        }

        private void OnLoadAssetSuccess(TextAsset asset)
        {
            TextAsset dll = (TextAsset)asset;
            Assembly hotfixAssembly = Assembly.Load(dll.bytes);
            Log.Info("Load hotfix dll OK.");
            StartHotfix(hotfixAssembly);
        }

        private void OnLoadAssetFail()
        {
            Log.Error("Load hotfix dll failed. ");
        }

        private void StartHotfix(Assembly hotfixAssembly)
        {
            var hotfixEntry = hotfixAssembly.GetType("Game.Hotfix.GameHotfixEntry");
            var start = hotfixEntry.GetMethod("Start");
            start?.Invoke(null, null);
            ChangeState<ProcedureStartGame>(m_procedureOwner);
        }
    }
}
