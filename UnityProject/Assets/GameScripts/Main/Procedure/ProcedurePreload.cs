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

        private readonly Dictionary<string, bool> _loadedFlag = new Dictionary<string, bool>();

        public override bool UseNativeDialog => true;

        private readonly bool _needProLoadConfig = true;

        /// <summary>
        /// 预加载回调。
        /// </summary>
        private LoadAssetCallbacks m_PreLoadAssetCallbacks;
        
        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
            m_PreLoadAssetCallbacks = new LoadAssetCallbacks(OnPreLoadAssetSuccess, OnPreLoadAssetFailure);
        }

        
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            _loadedFlag.Clear();

            UILoadMgr.Show(UIDefine.UILoadUpdate, Utility.Text.Format(LoadText.Instance.Label_Load_Load_Progress, 0));

            GameEvent.Send("UILoadUpdate.RefreshVersion");

            PreloadResources();
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            var totalCount = _loadedFlag.Count <= 0 ? 1 : _loadedFlag.Count;

            var loadCount = _loadedFlag.Count <= 0 ? 1 : 0;

            foreach (KeyValuePair<string, bool> loadedFlag in _loadedFlag)
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

            if (_loadedFlag.Count != 0)
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

        private void PreloadResources()
        {
            if (_needProLoadConfig)
            {
                LoadAllConfig();
            }
        }

        private void LoadAllConfig()
        {
            if (GameModule.Resource.PlayMode == EPlayMode.EditorSimulateMode)
            {
                return;
            }
            AssetInfo[] assetInfos = GameModule.Resource.GetAssetInfos("PRELOAD");
            foreach (var assetInfo in assetInfos)
            {
                PreLoad(assetInfo.Address);
            }
#if UNITY_WEBGL
            AssetInfo[] webAssetInfos = GameModule.Resource.GetAssetInfos("WEBGL_PRELOAD");
            foreach (var assetInfo in webAssetInfos)
            {
                PreLoad(assetInfo.Address);
            }
#endif
        }

        private void PreLoad(string location)
        {
            _loadedFlag.Add(location, false);
            GameModule.Resource.LoadAssetAsync(location, typeof(UnityEngine.Object), m_PreLoadAssetCallbacks, null);
        }

        private void OnPreLoadAssetFailure(string assetName, LoadResourceStatus status, string errormessage, object userdata)
        {
            Log.Warning("Can not preload asset from '{0}' with error message '{1}'.", assetName, errormessage);
            _loadedFlag[assetName] = true;
        }

        private void OnPreLoadAssetSuccess(string assetName, object asset, float duration, object userdata)
        {
            Log.Debug("Success preload asset from '{0}' duration '{1}'.", assetName, duration);
            _loadedFlag[assetName] = true;
        }
    }
}