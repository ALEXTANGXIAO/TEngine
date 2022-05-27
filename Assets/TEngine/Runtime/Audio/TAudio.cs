using UnityEngine;
using UnityEngine.Audio;

namespace TEngine
{
    public class TAudio
    {
        #region Propreties
        private int _id = 0;
        public AssetData _assetData = null;
        public AudioSource _source = null;
        Transform _transform = null;
        private float _volume = 1.0f;
        private float _duration = 0;
        private float _fadeoutTimer = 0f;
        private const float FadeoutDuration = 0.2f;
        private bool _inPool = false;
        #endregion

        #region Public Propreties
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

        public int ID
        {
            get
            {
                return _id;
            }
        }

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
            get
            {
                return _volume;
            }
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

        public float Duration
        {
            get
            {
                return _duration;
            }
        }

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
            get
            {
                return _transform.position;
            }
            set
            {
                _transform.position = value;
            }
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
        #endregion

        public AudioSource AudioResource()
        {
            return _source;
        }

        public static TAudio Create(string path, bool bAsync, AudioMixerGroup audioMixerGroup = null, bool bInPool = false)
        {
            TAudio audio = new TAudio();
            audio.Init(audioMixerGroup);
            audio.Load(path, bAsync, bInPool);

            return audio;
        }

        public void Init(AudioMixerGroup audioMixerGroup = null)
        {
            GameObject root = new GameObject("Audio");
            root.transform.SetParent(AudioMgr.Instance.transform);
            root.transform.localPosition = Vector3.zero;
            _transform = root.transform;
            _source = root.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            if (audioMixerGroup != null)
            {
                _source.outputAudioMixerGroup = audioMixerGroup;
            }
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
                    if (AudioMgr.Instance.AudioClipPool.ContainsKey(path))
                    {
                        OnAssetLoadComplete(AudioMgr.Instance.AudioClipPool[path]);
                        return;
                    }
                    if (bAsync)
                    {
                        _state = State.Loading;
                        ResMgr.Instance.GetAssetAsync(path, false, OnAssetLoadComplete);
                    }
                    else
                    {
                        OnAssetLoadComplete(ResMgr.Instance.GetAsset(path, false));
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

        void OnAssetLoadComplete(AssetData assetData)
        {
            if (assetData != null)
            {
                assetData.OnAsyncLoadComplete -= OnAssetLoadComplete;
                if (_inPool && !AudioMgr.Instance.AudioClipPool.ContainsKey(assetData.Path))
                {
                    assetData.AddRef();
                    AudioMgr.Instance.AudioClipPool.Add(assetData.Path, assetData);
                }
            }


            if (_pendingLoad != null)
            {
                assetData.AddRef();
                if (assetData != null)
                {
                    assetData.DecRef();
                }
                _state = State.End;
                string path = _pendingLoad.path;
                bool bAsync = _pendingLoad.bAsync;
                _pendingLoad = null;
                Load(path, bAsync);
            }
            else if (assetData != null)
            {
                assetData.AddRef();
                if (_assetData != null)
                {
                    _assetData.DecRef();
                }
                _assetData = assetData;
                _source.clip = _assetData.AssetObject as AudioClip;
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
                {
                    _state = State.End;
                }
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

            if (_assetData != null)
            {
                _assetData.DecRef();
            }
        }
    }
}
