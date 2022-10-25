using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HybridCLR;
using UnityEngine;

namespace TEngine.Runtime
{
    /// <summary>
    /// 流程加载器 - 代码初始化
    /// </summary>
    public class ProcedureCodeInit : ProcedureBase
    {
        
        public static List<string> AOTMetaAssemblyNames { get; } = new List<string>()
        {
            "mscorlib.dll",
            "System.dll",
            "System.Core.dll",
        };
        
        /// <summary>
        /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
        /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
        /// </summary>
        private static void LoadMetadataForAOTAssemblies()
        {
            // 可以加载任意aot assembly的对应的dll。但要求dll必须与unity build过程中生成的裁剪后的dll一致，而不能直接使用原始dll。
            // 我们在BuildProcessors里添加了处理代码，这些裁剪后的dll在打包时自动被复制到 {项目目录}/HybridCLRData/AssembliesPostIl2CppStrip/{Target} 目录。

            // 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
            // 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
            foreach (var aotDllName in AOTMetaAssemblyNames)
            {
                byte[] dllBytes = TResources.Load<TextAsset>(aotDllName)?.bytes;
                if (dllBytes == null)
                {
                    Log.Fatal($"{aotDllName} is null");
                    continue;
                }
                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes);
                Log.Info($"LoadMetadataForAOTAssembly:{aotDllName}. ret:{err}");
            }
        }
        
        /// <summary>
        /// 是否需要加载热更新DLL
        /// </summary>
        public bool NeedLoadDll => ResourceComponent.Instance.ResourceMode == ResourceMode.Updatable || ResourceComponent.Instance.ResourceMode == ResourceMode.UpdatableWhilePlaying;

        private IFsm<IProcedureManager> m_procedureOwner;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_procedureOwner = procedureOwner;

            if (!NeedLoadDll)
            {
                ChangeState<ProcedureStartGame>(procedureOwner);
                return;
            }

            LoadMetadataForAOTAssemblies();

#if UNITY_EDITOR
            Assembly hotfixAssembly = System.AppDomain.CurrentDomain.GetAssemblies().First(assembly => assembly.GetName().Name == "Assembly-CSharp");
            StartHotfix(hotfixAssembly);
#else
            TResources.LoadAsync<TextAsset>("Dll/HotFix.dll.bytes", (data =>
            {
                if (data == null)
                {
                    OnLoadAssetFail();
                    return;
                }
                var obj = data as TextAsset;

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
            Assembly hotfixAssembly =  System.Reflection.Assembly.Load(dll.bytes);
            Log.Info("Load hotfix dll OK.");
            StartHotfix(hotfixAssembly);
        }

        private void OnLoadAssetFail()
        {
            Log.Error("Load hotfix dll failed. ");
        }

        private void StartHotfix(Assembly hotfixAssembly)
        {
            var hotfixEntry = hotfixAssembly.GetType("HotFix.GameHotfixEntry");
            var start = hotfixEntry.GetMethod("Start");
            start?.Invoke(null, null);
            ChangeState<ProcedureStartGame>(m_procedureOwner);
        }
    }
}
