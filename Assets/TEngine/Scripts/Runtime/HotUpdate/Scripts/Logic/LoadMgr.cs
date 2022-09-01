using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TEngine.Runtime.HotUpdate
{
#pragma warning disable CS0162
    public class LoadMgr : TSingleton<LoadMgr>
    {
        /// <summary>
        /// 资源版本号
        /// </summary>
        private readonly string _downloadFilePath = FileSystem.ResourceRoot + "/";
        private Coroutine _coroutine;
        //加载检测
        private bool _dllLoad;
        private Coroutine _load_data_check;
        public DownloadImpl Downloader { get; set; }

        /// <summary>
        /// 下载结果回调
        /// </summary>
        /// <param name="result"></param>
        /// <param name="status"></param>
        /// <param name="files"></param>
        public void DownLoadCallBack(int result, GameStatus status, List<LoadResource> files = null)
        {
            //下载完成
            if (result == (int)DownLoadResult.AllDownLoaded)
            {
                _UnPackCallback(100, status, 100);
                GameEventMgr.Instance.Send("DownLoadResult.AllDownLoaded", true);
                _StopLoadingCheck();
            }
            else
            {
                _StopLoadingCheck();
                //网络发生变化，重试继续下载
                if (result == (int)DownLoadResult.NetChanged || result == (int)DownLoadResult.HeadRequestError)
                {
                    _RetryConnect();
                    return;
                }
                if (result == (int)DownLoadResult.DownLoadingError ||
                    result == (int)DownLoadResult.ReceiveNullData ||
                        result == (int)DownLoadResult.DownError ||
                        result == (int)DownLoadResult.ReceiveError ||
                        result == (int)DownLoadResult.Md5Wrong)
                {
                    LoaderUtilities.ShowMessageBox(string.Format(LoadText.Instance.Label_Load_Error, result), MessageShowType.OneButton,
                        LoadStyle.StyleEnum.Style_QuitApp,
                        () => {
                            Application.Quit();
                        });
                }
            }

            ResetRetry();
        }

        /// <summary>
        /// 本地检测,过滤掉本地存在了的资源
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        public List<LoadResource> CheckLocalExitResource(List<LoadResource> res)
        {
            var list = new List<LoadResource>();
            foreach (var item in res)
            {
                string resPath = _downloadFilePath + item.Url;
                if (File.Exists(resPath))
                {
                    if (string.CompareOrdinal(LoaderUtilities.GetMd5Hash(resPath), item.Md5) == 0)
                    {
                        continue;
                    }

                    list.Add(item);
                }
                else
                {
                    list.Add(item);
                }
            }
            return list;
        }

        public void StartDownLoad()
        {
            if (Downloader == null)
                return;

            if (Downloader.IsLoading())
            {
                TLogger.LogInfo("LoadMgr.StartUpdate，down loading......");
                return;
            }


            if (_coroutine != null)
            {
                MonoUtility.StopCoroutine(_coroutine);
                _coroutine = null;
            }
            //开始下载
            _coroutine = MonoUtility.StartCoroutine(Downloader.DownLoad());

            LoaderUtilities.DelayFun(() =>
            {
                if (_load_data_check != null)
                {
                    MonoUtility.StopCoroutine(_load_data_check);
                }

                _load_data_check = MonoUtility.StartCoroutine(_StartLoadingCheck());
            }, new WaitForSeconds(1));
        }

        public void StopDownLoad()
        {
            if (Downloader == null)
                return;

            Downloader.StopDownLoad();
            if (_coroutine == null) return;
            MonoUtility.StopCoroutine(_coroutine);
            _coroutine = null;
        }

        #region 数据加载检测
        private long _currentSize;
        private int _currentTryCount;
        private Action _retryAction;
        private IEnumerator _StartLoadingCheck()
        {
            while (Downloader.IsLoading())
            {
                _currentSize = Downloader.CurrentLoadSize();
                yield return new WaitForSeconds(20);
                long newSize = Downloader.CurrentLoadSize();
                if (newSize - _currentSize < 1024)
                {
                    StopDownLoad();
                    LoaderUtilities.ShowMessageBox(LoadText.Instance.Label_DownLoadFailed, MessageShowType.TwoButton,
                        LoadStyle.StyleEnum.Style_DownZip,
                        () =>
                        {
                            StartDownLoad();
                        }, () =>
                        {
                            Application.Quit();
                        });
                }
                else
                {
                    _currentSize = Downloader.CurrentLoadSize();
                }
            }
        }
        private void _StopLoadingCheck()
        {
            if (_load_data_check != null)
            {
                MonoUtility.StopCoroutine(_load_data_check);
            }
        }
        //尝试重连
        private void _RetryConnect()
        {
            if (_retryAction == null)
            {
                _retryAction = () => {
                    LoaderUtilities.DelayFun(() =>
                    {
                        _currentTryCount++;
                        UILoadMgr.Show(UIDefine.UILoadUpdate, string.Format(LoadText.Instance.Label_Net_Changed, _currentTryCount));
                        StartDownLoad();
                    }, new WaitForSeconds(3));
                };
            }

            if (_currentTryCount >= 3)
            {
                LoaderUtilities.ShowMessageBox(LoadText.Instance.Label_Net_Error, MessageShowType.TwoButton,
                    LoadStyle.StyleEnum.Style_DownZip,
                    () =>
                    {
                        _currentTryCount = 0;
                        _retryAction.Invoke();
                    }, Quit);
            }
            else
            {
                _retryAction.Invoke();
            }
        }

        private void ResetRetry()
        {
            _currentTryCount = 0;
            _retryAction = null;
        }

        #endregion

        /// <summary>
        /// 开始更新资源
        /// </summary>
        public void StartUpdate(List<LoadResource> list)
        {
            if (list == null || list.Count <= 0)
            {
                TLogger.LogError("LoadMgr.StartUpdate， resource list is empty");
                return;
            }
            //检测内存是否充足
            long totalSize = 0;
            foreach (var item in list)
            {
                totalSize += item.Size;
            }

            if (false)
            {
                LoaderUtilities.ShowMessageBox(LoadText.Instance.Label_Memory_Low_Load, MessageShowType.OneButton,
                    LoadStyle.StyleEnum.Style_QuitApp,
                    () => {
                        Application.Quit();
                    });
                return;
            }

            Downloader = new DownloadImpl(list, _downloadFilePath, (result, files) =>
            {
                DownLoadCallBack(result, GameStatus.AssetLoad, files);
            });
            StartDownLoad();
        }

        /// <summary>
        /// 解压缩进度回调
        /// </summary>
        /// <param name="current"></param>
        /// <param name="status"></param>
        /// <param name="total"></param>
        private void _UnPackCallback(long current, GameStatus status, long total)
        {
            float t;
            if (total == 0)
                t = 0;
            else
            {
                t = (float)current / total;
            }
            LoadUpdateLogic.Instance.UnpackedProgressAction?.Invoke(t, status);
        }

        internal void Quit()
        {
            Application.Quit();
        }
    }
}
