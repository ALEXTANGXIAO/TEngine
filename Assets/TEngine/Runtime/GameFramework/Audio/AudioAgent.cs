using UnityEngine;
using UnityEngine.Audio;
using YooAsset;

namespace TEngine
{
    /// <summary>
    /// 声音代理辅助器。
    /// </summary>
    public class AudioAgent
    {
        private AudioModule _audioModule;
        private int _id = 0;
        public AssetOperationHandle assetOperationHandle = null;
        private AudioSource _source = null;
        Transform _transform = null;
        float _volume = 1.0f;
        float _duration = 0;
        float _fadeoutTimer = 0f;
        const float FadeoutDuration = 0.2f;
        private bool _inPool = false;

        enum State
        {
            None,
            Loading,
            Playing,
            FadingOut,
            End,
        };

        State _state = State.None;

        class LoadRequest
        {
            public string path;
            public bool bAsync;
        }

        LoadRequest _pendingLoad = null;

        public int ID => _id;

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

        public bool IsFinish
        {
            get
            {
                if (_source != null)
                {
                    return _state == State.End;
                }
                else
                {
                    return true;
                }
            }
        }

        public float Duration => _duration;

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

        public Vector3 Position
        {
            get => _transform.position;
            set => _transform.position = value;
        }

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

        public AudioSource AudioResource()
        {
            return _source;
        }

        public static AudioAgent Create(string path, bool bAsync, AudioCategory audioCategory, bool bInPool = false)
        {
            AudioAgent audioAgent = new AudioAgent();
            audioAgent.Init(audioCategory);
            audioAgent.Load(path, bAsync, bInPool);
            return audioAgent;
        }

        public void Init(AudioCategory audioCategory,int index = 0)
        {
            _audioModule = GameEntry.GetModule<AudioModule>();
            GameObject host = new GameObject(Utility.Text.Format("Audio Agent Helper - {0} - {1}", audioCategory.AudioMixerGroup.name, index));
            host.transform.SetParent(audioCategory.InstanceRoot);
            host.transform.localPosition = Vector3.zero;
            _transform = host.transform;
            _source = host.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            AudioMixerGroup[] audioMixerGroups = audioCategory.AudioMixer.FindMatchingGroups(Utility.Text.Format("Master/{0}/{1}", audioCategory.AudioMixerGroup.name, index));
            _source.outputAudioMixerGroup = audioMixerGroups.Length > 0 ? audioMixerGroups[0] : audioCategory.AudioMixerGroup;
            _id = _source.GetInstanceID();
        }

        public void Load(string path, bool bAsync, bool bInPool = false)
        {
            _inPool = bInPool;
            if (_state == State.None || _state == State.End)
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
                        _state = State.Loading;
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
                _pendingLoad = new LoadRequest { path = path, bAsync = bAsync };

                if (_state == State.Playing)
                {
                    Stop(true);
                }
            }
        }

        public void Stop(bool fadeout = false)
        {
            if (_source != null)
            {
                if (fadeout)
                {
                    _fadeoutTimer = FadeoutDuration;
                    _state = State.FadingOut;
                }
                else
                {
                    _source.Stop();
                    _state = State.End;
                }
            }
        }

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

                _state = State.End;
                string path = _pendingLoad.path;
                bool bAsync = _pendingLoad.bAsync;
                _pendingLoad = null;
                Load(path, bAsync);
            }
            else if (handle != null)
            {
                if (assetOperationHandle != null)
                {
                    assetOperationHandle.Dispose();
                }

                assetOperationHandle = handle;

                _source.clip = assetOperationHandle.AssetObject as AudioClip;
                if (_source.clip != null)
                {
                    _source.Play();
                    _state = State.Playing;
                }
                else
                {
                    _state = State.End;
                }
            }
            else
            {
                _state = State.End;
            }
        }

        public void Update(float delta)
        {
            if (_state == State.Playing)
            {
                if (!_source.isPlaying)
                    _state = State.End;
            }
            else if (_state == State.FadingOut)
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
                        string path = _pendingLoad.path;
                        bool bAsync = _pendingLoad.bAsync;
                        _pendingLoad = null;
                        Load(path, bAsync);
                    }

                    _source.volume = _volume;
                }
            }

            _duration += delta;
        }

        public void Destroy()
        {
            if (_transform != null)
            {
                Object.Destroy(_transform.gameObject);
            }

            if (assetOperationHandle != null)
            {
                assetOperationHandle.Dispose();
            }
        }
    }
}