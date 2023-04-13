using System.Collections.Generic;
using TEngine;
using YooAsset;
using ProcedureOwner = TEngine.IFsm<TEngine.IProcedureManager>;

namespace GameMain
{
    /// <summary>
    /// 预加载流程
    /// </summary>
    public class ProcedurePreload : ProcedureBase
    {
        private Dictionary<string, bool> m_LoadedFlag = new Dictionary<string, bool>();

        public override bool UseNativeDialog
        {
            get { return true; }
        }

        private bool m_needProLoadConfig = false;

        private bool m_InitConfigXml = false;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_LoadedFlag.Clear();

            if (GameModule.Resource.playMode == EPlayMode.EditorSimulateMode)
            {
                m_InitConfigXml = true;
            }

            UILoadMgr.Show(UIDefine.UILoadUpdate, Utility.Text.Format(LoadText.Instance.Label_Load_Load_Progress, 0));

            PreloadResources();
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

            UILoadMgr.Show(UIDefine.UILoadUpdate, Utility.Text.Format(LoadText.Instance.Label_Load_Load_Progress, (float)loadCount / totalCount * 100));

            if (loadCount < totalCount)
            {
                return;
            }

            if (m_InitConfigXml == false)
            {
                return;
            }

            UILoadMgr.HideAll();

            ChangeState<ProcedureLoadAssembly>(procedureOwner);
        }

        private void PreloadResources()
        {
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
            if (GameModule.Resource.playMode == EPlayMode.EditorSimulateMode)
            {
                m_InitConfigXml = true;
                return;
            }
        }
    }
}