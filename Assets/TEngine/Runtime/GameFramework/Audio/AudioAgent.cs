using UnityEngine;
using UnityEngine.Audio;
using YooAsset;

namespace TEngine
{
    /// <summary>
    /// 音频代理辅助器。
    /// </summary>
    public class AudioAgent
    {
        private AudioModule _audioModule;
        private int _id;
        private AudioSource _source;
        private AssetOperationHandle _assetOperationHandle;
        private Transform _transform;
        float _volume = 1.0f;
        float _duration;
        private float _fadeoutTimer;
        private const float FadeoutDuration = 0.2f;
        private bool _inPool;

        /// <summary>
        /// 音频代理辅助器运行时状态枚举。
        /// </summary>
        enum RuntimeState
        {
            /// <summary>
            /// 无状态。
            /// </summary>
            None,
            /// <summary>
            /// 加载中状态。
            /// </summary>
            Loading,
            /// <summary>
            /// 播放中状态。
            /// </summary>
            Playing,
            /// <summary>
            /// 渐渐消失状态。
            /// </summary>
            FadingOut,
            /// <summary>
            /// 结束状态。
            /// </summary>
            End,
        };

        /// <summary>
        /// 音频代理辅助器运行时状态。
        /// </summary>
        RuntimeState _runtimeState = RuntimeState.None;

        /// <summary>
        /// 音频代理加载请求。
        /// </summary>
        class LoadRequest
        {
            public string Path;
            public bool BAsync;
        }

        /// <summary>
        /// 音频代理加载请求。
        /// </summary>
        LoadRequest _pendingLoad = null;

        /// <summary>
        /// AudioSource实例化Id
        /// </summary>
        public int ID => _id;

        /// <summary>
        /// 资源操作句柄。
        /// </summary>
        public AssetOperationHandle assetOperationHandle => _assetOperationHandle;

        /// <summary>
        /// 音频代理辅助器音频大小。
        /// </summary>
        public float Volume
        {
            set
            {
                if (_source != null)
                {
                    _volume = value;
                    _source.volume = _volume;
                }
            }
            get => _volume;
        }

        /// <summary>
        /// 音频代理辅助器当前是否空闲。
        /// </summary>
        public bool IsFree
        {
            get
            {
                if (_source != null)
                {
                    return _runtimeState == RuntimeState.End;
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// 音频代理辅助器播放秒数。
        /// </summary>
        public float Duration => _duration;

        /// <summary>
        /// 音频代理辅助器当前音频长度。
        /// </summary>
        public float Length
        {
            get
            {
                if (_source != null && _source.clip != null)
                {
                    return _source.clip.length;
                }

                return 0;
            }
        }

        /// <summary>
        /// 音频代理辅助器实例位置。
        /// </summary>
        public Vector3 Position
        {
            get => _transform.position;
            set => _transform.position = value;
        }

        /// <summary>
        /// 音频代理辅助器是否循环。
        /// </summary>
        public bool IsLoop
        {
            get
            {
                if (_source != null)
                {
                    return _source.loop;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (_source != null)
                {
                    _source.loop = value;
                }
            }
        }

        /// <summary>
        /// 音频代理辅助器是否正在播放。
        /// </summary>
        internal bool IsPlaying
        {
            get
            {
                if (_source != null && _source.isPlaying)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 音频代理辅助器获取当前声源。
        /// </summary>
        /// <returns></returns>
        public AudioSource AudioResource()
        {
            return _source;
        }

        /// <summary>
        /// 创建音频代理辅助器。
        /// </summary>
        /// <param name="path">生效路径。</param>
        /// <param name="bAsync">是否异步。</param>
        /// <param name="audioCategory">音频轨道（类别）。</param>
        /// <param name="bInPool">是否池化。</param>
        /// <returns>音频代理辅助器。</returns>
        public static AudioAgent Create(string path, bool bAsync, AudioCategory audioCategory, bool bInPool = false)
        {
            AudioAgent audioAgent = new AudioAgent();
            audioAgent.Init(audioCategory);
            audioAgent.Load(path, bAsync, bInPool);
            return audioAgent;
        }

        /// <summary>
        /// 初始化音频代理辅助器。
        /// </summary>
        /// <param name="audioCategory">音频轨道（类别）。</param>
        /// <param name="index">音频代理辅助器编号。</param>
        public void Init(AudioCategory audioCategory,int index = 0)
        {
            _audioModule = GameModule.Audio;
            GameObject host = new GameObject(Utility.Text.Format("Audio Agent Helper - {0} - {1}", audioCategory.AudioMixerGroup.name, index));
            host.transform.SetParent(audioCategory.InstanceRoot);
            host.transform.localPosition = Vector3.zero;
            _transform = host.transform;
            _source = host.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            AudioMixerGroup[] audioMixerGroups = audioCategory.AudioMixer.FindMatchingGroups(Utility.Text.Format("Master/{0}/{1}", audioCategory.AudioMixerGroup.name, index));
            _source.outputAudioMixerGroup = audioMixerGroups.Length > 0 ? audioMixerGroups[0] : audioCategory.AudioMixerGroup;
            _source.rolloffMode = audioCategory.AudioGroupConfig.audioRolloffMode;
            _source.minDistance = audioCategory.AudioGroupConfig.minDistance;
            _source.maxDistance = audioCategory.AudioGroupConfig.maxDistance;
            _id = _source.GetInstanceID();
        }

        /// <summary>
        /// 加载音频代理辅助器。
        /// </summary>
        /// <param name="path">资源路径。</param>
        /// <param name="bAsync">是否异步。</param>
        /// <param name="bInPool">是否池化。</param>
        public void Load(string path, bool bAsync, bool bInPool = false)
        {
            _inPool = bInPool;
            if (_runtimeState == RuntimeState.None || _runtimeState == RuntimeState.End)
            {
                _duration = 0;
                if (!string.IsNullOrEmpty(path))
                {
                    if (_audioModule.AudioClipPool.ContainsKey(path))
                    {
                        OnAssetLoadComplete(_audioModule.AudioClipPool[path]);
                        return;
                    }

                    if (bAsync)
                    {
                        _runtimeState = RuntimeState.Loading;
                        AssetOperationHandle handle = _audioModule.ResourceManager.LoadAssetAsyncHandle<AudioClip>(path);
                        handle.Completed += OnAssetLoadComplete;
                    }
                    else
                    {
                        AssetOperationHandle handle = _audioModule.ResourceManager.LoadAssetGetOperation<AudioClip>(path);
                        OnAssetLoadComplete(handle);
                    }
                }
            }
            else
            {
                _pendingLoad = new LoadRequest { Path = path, BAsync = bAsync };

                if (_runtimeState == RuntimeState.Playing)
                {
                    Stop(true);
                }
            }
        }

        /// <summary>
        /// 暂停音频代理辅助器。
        /// </summary>
        /// <param name="fadeout">是否渐出。</param>
        public void Stop(bool fadeout = false)
        {
            if (_source != null)
            {
                if (fadeout)
                {
                    _fadeoutTimer = FadeoutDuration;
                    _runtimeState = RuntimeState.FadingOut;
                }
                else
                {
                    _source.Stop();
                    _runtimeState = RuntimeState.End;
                }
            }
        }

        /// <summary>
        /// 资源加载完成。
        /// </summary>
        /// <param name="handle">资源操作句柄。</param>
        void OnAssetLoadComplete(AssetOperationHandle handle)
        {
            if (handle != null)
            {
                handle.Completed -= OnAssetLoadComplete;
                if (_inPool && !_audioModule.AudioClipPool.ContainsKey(handle.GetAssetInfo().AssetPath))
                {
                    _audioModule.AudioClipPool.Add(handle.GetAssetInfo().AssetPath, handle);
                }
            }

            if (_pendingLoad != null)
            {
                if (handle != null)
                {
                    handle.Dispose();
                }

                _runtimeState = RuntimeState.End;
                string path = _pendingLoad.Path;
                bool bAsync = _pendingLoad.BAsync;
                _pendingLoad = null;
                Load(path, bAsync);
            }
            else if (handle != null)
            {
                if (_assetOperationHandle != null)
                {
                    _assetOperationHandle.Dispose();
                }

                _assetOperationHandle = handle;

                _source.clip = _assetOperationHandle.AssetObject as AudioClip;
                if (_source.clip != null)
                {
                    _source.Play();
                    _runtimeState = RuntimeState.Playing;
                }
                else
                {
                    _runtimeState = RuntimeState.End;
                }
            }
            else
            {
                _runtimeState = RuntimeState.End;
            }
        }

        /// <summary>
        /// 轮询音频代理辅助器。
        /// </summary>
        /// <param name="delta"></param>
        public void Update(float delta)
        {
            if (_runtimeState == RuntimeState.Playing)
            {
                if (!_source.isPlaying)
                {
                    _runtimeState = RuntimeState.End;
                }
            }
            else if (_runtimeState == RuntimeState.FadingOut)
            {
                if (_fadeoutTimer > 0f)
                {
                    _fadeoutTimer -= delta;
                    _source.volume = _volume * _fadeoutTimer / FadeoutDuration;
                }
                else
                {
                    Stop();
                    if (_pendingLoad != null)
                    {
                        string path = _pendingLoad.Path;
                        bool bAsync = _pendingLoad.BAsync;
                        _pendingLoad = null;
                        Load(path, bAsync);
                    }

                    _source.volume = _volume;
                }
            }
            _duration += delta;
        }

        /// <summary>
        /// 销毁音频代理辅助器。
        /// </summary>
        public void Destroy()
        {
            if (_transform != null)
            {
                Object.Destroy(_transform.gameObject);
            }

            if (_assetOperationHandle != null)
            {
                _assetOperationHandle.Dispose();
            }
        }
    }
}