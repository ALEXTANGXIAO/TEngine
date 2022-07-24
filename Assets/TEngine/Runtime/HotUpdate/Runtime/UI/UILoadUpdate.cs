using System.IO;
using TEngine;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace TEngine.UI
{
    public class UILoadUpdate : UIBase
    {
#pragma warning disable 649
        [SerializeField]
        public Button _btn_clear;
        [SerializeField]
        public Scrollbar _obj_progress;
        [SerializeField]
        public Text _label_desc;
        [SerializeField]
        public Text _label_appid;
        [SerializeField]
        public Text _label_resid;
#pragma warning restore 649

        public virtual void Start()
        {
            EventTriggerListener.Get(_btn_clear.gameObject).OnClick = OnClear;
            _btn_clear.gameObject.SetActive(true);
        }

        public virtual void OnEnable()
        {
            LoadUpdateLogic.Instance.Download_Complete_Action  += DownLoad_Complete_Action;
            LoadUpdateLogic.Instance.Down_Progress_Action      += DownLoad_Progress_Action;
            LoadUpdateLogic.Instance._Unpacked_Complete_Action += Unpacked_Complete_Action;
            LoadUpdateLogic.Instance._Unpacked_Progress_Action += Unpacked_Progress_Action;
            RefreshVersion();
        }

        public override void OnEnter(object param)
        {
            base.OnEnter(param);
            _label_desc.text = param.ToString();
            RefreshVersion();
        }

        public virtual void Update()
        {

        }

        private void RefreshVersion()
        {
            _label_appid.text = string.Format(LoadText.Instance.Label_App_id, GameConfig.Instance.GameBundleVersion);
            _label_resid.text = string.Format(LoadText.Instance.Label_Res_id, GameConfig.Instance.ResId);
        }

        public virtual void OnContinue(GameObject obj)
        {
            LoadMgr.Instance.StartDownLoad();
        }

        public virtual void OnStop(GameObject obj)
        {
            LoadMgr.Instance.StopDownLoad();
        }

        /// <summary>
        /// 清空本地缓存
        /// </summary>
        /// <param name="obj"></param>
        public virtual void OnClear(GameObject obj)
        {
            OnStop(null);
            LoaderUtilities.ShowMessageBox(LoadText.Instance.Label_Clear_Comfirm, MessageShowType.TwoButton,
                LoadStyle.StyleEnum.Style_Clear,
                () =>
            {
                LoaderUtilities.DeleteFolder(FileSystem.ResourceRoot);
                Application.Quit();
            },()=> {
                OnContinue(null);
            });
        }

        /// <summary>
        /// 下载进度完成
        /// </summary>
        /// <param name="type"></param>
        public virtual void DownLoad_Complete_Action(int type)
        {
            
        }
        /// <summary>
        /// 下载进度更新
        /// </summary>
        /// <param name="size"></param>
        public virtual void DownLoad_Progress_Action(long size)
        {
            if (LoadMgr.Instance.Downloader != null)
            {
                _obj_progress.gameObject.SetActive(true);
                _label_desc.text = string.Format(LoadText.Instance.Label_Load_Progress, LoaderUtilities.FormatData(LoadMgr.Instance.Downloader.Speed), LoaderUtilities.FormatData(LoadMgr.Instance.Downloader.FileSize));
                float pro = (float)(LoadMgr.Instance.Downloader.DownLoadSize + size) / LoadMgr.Instance.Downloader.FileSize;
                _obj_progress.size = pro;
            }
        }

        /// <summary>
        /// 解压缩完成回调
        /// </summary>
        /// <param name="type"></param>
        /// <param name="status"></param>
        public virtual void Unpacked_Complete_Action(bool type,GameStatus status)
        {
            _obj_progress.gameObject.SetActive(true);
            _label_desc.text = LoadText.Instance.Label_Load_UnpackComplete;
            if (status == GameStatus.AssetLoad)
            {
                LoadMgr.Instance.StartGame();
            }
            else
            {
                TLogger.LogError("error emnu type");
            }
        }

        /// <summary>
        /// 解压缩进度更新
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="status"></param>
        public virtual void Unpacked_Progress_Action(float progress,GameStatus status)
        {
            _obj_progress.gameObject.SetActive(true);
            if (status == GameStatus.First)
            {
                _label_desc.text = LoadText.Instance.Label_Load_FirstUnpack;
            }
            else
            {
                _label_desc.text = LoadText.Instance.Label_Load_Unpacking;                
            }
            _obj_progress.size = progress;
        }

        public virtual void OnDisable()
        {
            OnStop(null);
            LoadUpdateLogic.Instance.Download_Complete_Action  -= DownLoad_Complete_Action;
            LoadUpdateLogic.Instance.Down_Progress_Action      -= DownLoad_Progress_Action;
            LoadUpdateLogic.Instance._Unpacked_Complete_Action -= Unpacked_Complete_Action;
            LoadUpdateLogic.Instance._Unpacked_Progress_Action -= Unpacked_Progress_Action;
        }
    }

}