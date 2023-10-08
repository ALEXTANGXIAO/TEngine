using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TEngine;
using UnityEngine;
using YooAsset;
using ProcedureOwner = TEngine.IFsm<TEngine.IProcedureManager>;

namespace GameMain
{
    /// <summary>
    /// 预加载流程
    /// </summary>
    public class ProcedurePreload : ProcedureBase
    {
        private float _progress = 0f;

        private Dictionary<string, bool> m_LoadedFlag = new Dictionary<string, bool>();

        public override bool UseNativeDialog => true;

        private bool m_needProLoadConfig = false;

        private bool m_InitConfigXml = false;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_LoadedFlag.Clear();

            if (GameModule.Resource.PlayMode == EPlayMode.EditorSimulateMode)
            {
                m_InitConfigXml = true;
            }

            UILoadMgr.Show(UIDefine.UILoadUpdate, Utility.Text.Format(LoadText.Instance.Label_Load_Load_Progress, 0));

            GameEvent.Send("UILoadUpdate.RefreshVersion");

            PreloadResources().Forget();
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            var totalCount = m_LoadedFlag.Count <= 0 ? 1 : m_LoadedFlag.Count;

            var loadCount = m_LoadedFlag.Count <= 0 ? 1 : 0;

            foreach (KeyValuePair<string, bool> loadedFlag in m_LoadedFlag)
            {
                if (!loadedFlag.Value)
                {
                    break;
                }
                else
                {
                    loadCount++;
                }
            }

            if (m_LoadedFlag.Count != 0)
            {
                UILoadMgr.Show(UIDefine.UILoadUpdate, Utility.Text.Format(LoadText.Instance.Label_Load_Load_Progress, (float)loadCount / totalCount * 100));
            }
            else
            {
                LoadUpdateLogic.Instance.DownProgressAction?.Invoke(_progress);

                string progressStr = $"{_progress * 100:f1}";

                if (Math.Abs(_progress - 1f) < 0.001f)
                {
                    UILoadMgr.Show(UIDefine.UILoadUpdate, LoadText.Instance.Label_Load_Load_Complete);
                }
                else
                {
                    UILoadMgr.Show(UIDefine.UILoadUpdate, Utility.Text.Format(LoadText.Instance.Label_Load_Load_Progress, progressStr));
                }
            }

            if (loadCount < totalCount)
            {
                return;
            }

            if (m_InitConfigXml == false)
            {
                return;
            }

            ChangeState<ProcedureLoadAssembly>(procedureOwner);
        }


        public IEnumerator SmoothValue(float value, float duration, Action callback = null)
        {
            float time = 0f;
            while (time < duration)
            {
                time += GameTime.deltaTime;
                var result = Mathf.Lerp(0, value, time / duration);
                _progress = result;
                yield return new WaitForEndOfFrame();
            }
            _progress = value;
            callback?.Invoke();
        }

        private async UniTaskVoid PreloadResources()
        {
            await SmoothValue(1f, 1.2f).ToUniTask(GameModule.Procedure);

            await UniTask.Delay(TimeSpan.FromSeconds(2.5f));

            if (m_needProLoadConfig)
            {
                LoadAllConfig();
            }
            else
            {
                m_InitConfigXml = true;
            }
        }

        private void LoadAllConfig()
        {
            if (GameModule.Resource.PlayMode == EPlayMode.EditorSimulateMode)
            {
                m_InitConfigXml = true;
                return;
            }
        }

        private void LoadConfig(string configName)
        {
            m_LoadedFlag.Add(configName, false);
            GameModule.Resource.LoadAssetAsync<TextAsset>(configName, OnLoadSuccess);
        }

        private void OnLoadSuccess(AssetOperationHandle assetOperationHandle)
        {
            if (assetOperationHandle == null)
            {
                return;
            }

            var name = assetOperationHandle.GetAssetInfo().Address;
            m_LoadedFlag[name] = true;
            Log.Info("Load config '{0}' OK.", name);
        }
    }
}