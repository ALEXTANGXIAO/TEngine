using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;

namespace TEngine.Runtime
{
    /// <summary>
    /// 音频类型
    /// </summary>
    public enum AudioType
    {
        /// <summary>
        /// 声音
        /// </summary>
        Sound,
        /// <summary>
        /// 背景音乐
        /// </summary>
        Music,
        /// <summary>
        /// 人声
        /// </summary>
        Voice,
        /// <summary>
        /// 最大
        /// </summary>
        Max
    }

    /// <summary>
    /// 音频控制管理器
    /// </summary>
    public class AudioMgr : UnitySingleton<AudioMgr>
    {
        #region Propreties
        private float _volume = 1f;
        private bool _enable = true;
        private bool _disabled = false;
        public AudioMixer audioMixer { get; set; }
        float[] _agentVolume = new float[(int)AudioType.Max];
        private AudioAgent[] _audioAgents = new AudioAgent[(int)AudioType.Max];
        public Dictionary<string, AssetData> AudioClipPool = new Dictionary<string, AssetData>();
        #endregion

        #region 控制器
        /// <summary>
        /// 总音量
        /// </summary>
        public float Volume
        {
            get
            {
                if (_disabled)
                {
                    return 0.0f;
                }
                return _volume;
            }
            set
            {
                if (_disabled)
                {
                    return;
                }
                _volume = value;
                AudioListener.volume = _volume;
            }
        }

        /// <summary>
        /// 总开关
        /// </summary>
        public bool Enable
        {
            get
            {
                if (_disabled)
                {
                    return false;
                }
                return _enable;
            }
            set
            {
                if (_disabled)
                {
                    return;
                }
                _enable = value;
                AudioListener.volume = _enable ? _volume : 0f;
            }
        }

        /// <summary>
        /// 背景音量
        /// </summary>
        public float MusicVolume
        {
            get
            {
                if (_disabled)
                {
                    return 0.0f;
                }
                return _agentVolume[(int)AudioType.Music];
            }
            set
            {
                if (_disabled)
                {
                    return;
                }
                float volume = Mathf.Clamp(value, 0.0001f, 1.0f);
                _agentVolume[(int)AudioType.Music] = volume;
                audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20f);
            }
        }

        /// <summary>
        /// 音效音量
        /// </summary>
        public float SoundVolume
        {
            get
            {
                if (_disabled)
                {
                    return 0.0f;
                }
                return _agentVolume[(int)AudioType.Sound];
            }
            set
            {
                if (_disabled)
                {
                    return;
                }
                float volume = Mathf.Clamp(value, 0.0001f, 1.0f);
                _agentVolume[(int)AudioType.Sound] = volume;
                audioMixer.SetFloat("SoundVolume", Mathf.Log10(volume) * 20f);
            }
        }

        /// <summary>
        /// Voice音量
        /// </summary>
        public float VoiceVolume
        {
            get
            {
                if (_disabled)
                {
                    return 0.0f;
                }
                return _agentVolume[(int)AudioType.Voice];
            }
            set
            {
                if (_disabled)
                {
                    return;
                }
                float volume = Mathf.Clamp(value, 0.0001f, 1.0f);
                _agentVolume[(int)AudioType.Voice] = volume;
                audioMixer.SetFloat("VoiceVolume", Mathf.Log10(volume) * 20f);
            }
        }

        /// <summary>
        /// 是否允许Music
        /// </summary>
        public bool MusicEnable
        {
            get
            {
                if (_disabled)
                {
                    return false;
                }
                float db;
                if (audioMixer.GetFloat("MusicVolume", out db))
                {
                    return db > -80f;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (_disabled)
                {
                    return;
                }
                if (value)
                {
                    audioMixer.SetFloat("MusicVolume", Mathf.Log10(_agentVolume[(int)AudioType.Music]) * 20f);
                }
                else
                {
                    audioMixer.SetFloat("MusicVolume", -80f);
                }
            }
        }

        /// <summary>
        /// 是否允许Sound
        /// </summary>
        public bool SoundEnable
        {
            get
            {
                if (_disabled)
                {
                    return false;
                }
                return _audioAgents[(int)AudioType.Sound].Enable;
            }
            set
            {
                if (_disabled)
                {
                    return;
                }
                _audioAgents[(int)AudioType.Sound].Enable = value;
            }
        }

        /// <summary>
        /// 是否允许Voice
        /// </summary>
        public bool VoiceEnable
        {
            get
            {
                if (_disabled)
                {
                    return false;
                }
                return _audioAgents[(int)AudioType.Voice].Enable;
            }
            set
            {
                if (_disabled)
                {
                    return;
                }
                _audioAgents[(int)AudioType.Voice].Enable = value;
            }
        }
        #endregion


        protected override void OnLoad()
        {
            try
            {
                TypeInfo typeInfo = typeof(AudioSettings).GetTypeInfo();
                PropertyInfo propertyInfo = typeInfo.GetDeclaredProperty("unityAudioDisabled");
                _disabled = (bool)propertyInfo.GetValue(null);
                if (_disabled)
                {
                    return;
                }
            }
            catch (Exception e)
            {
                TLogger.LogError(e.ToString());
            }

            audioMixer = Resources.Load<AudioMixer>("Audio/TEngineAudioMixer");

            for (int i = 0; i < (int)AudioType.Max; ++i)
            {
                int channelMaxNum = 0;
                if (i == (int)AudioType.Sound)
                {
                    channelMaxNum = 10;
                }
                else
                {
                    channelMaxNum = 1;
                }
                _audioAgents[i] = new AudioAgent(channelMaxNum, audioMixer.FindMatchingGroups(((AudioType)i).ToString())[0]);
                _agentVolume[i] = 1.0f;
            }
        }

        #region 外部调用播放操作
        public TAudio Play(AudioType type, string path, bool bLoop = false, float volume = 1.0f, bool bAsync = false, bool bInPool = false)
        {
            if (_disabled)
            {
                return null;
            }
            TAudio audio = _audioAgents[(int)type].Play(path, bAsync, bInPool);
            {
                if (audio != null)
                {
                    audio.IsLoop = bLoop;
                    audio.Volume = volume;
                }
                return audio;
            }
        }

        public void Stop(AudioType type, bool fadeout)
        {
            if (_disabled)
            {
                return;
            }
            _audioAgents[(int)type].Stop(fadeout);
        }

        public void StopAll(bool fadeout)
        {
            if (_disabled)
            {
                return;
            }
            for (int i = 0; i < (int)AudioType.Max; ++i)
            {
                if (_audioAgents[i] != null)
                {
                    _audioAgents[i].Stop(fadeout);
                }
            }

        }

        public void Restart()
        {
            if (_disabled)
            {
                return;
            }
            CleanSoundPool();
            for (int i = 0; i < (int)AudioType.Max; ++i)
            {
                if (_audioAgents[i] != null)
                {
                    for (int j = 0; j < _audioAgents[i]._audioObjects.Count; ++j)
                    {
                        if (_audioAgents[i]._audioObjects[j] != null)
                        {
                            _audioAgents[i]._audioObjects[j].Destroy();
                            _audioAgents[i]._audioObjects[j] = null;
                        }
                    }
                }
                _audioAgents[i] = null;
            }
            OnLoad();
        }
        #endregion


        #region Pool
        public void PutInAudioPool(List<string> list)
        {
            if (_disabled)
                return;
            foreach (string path in list)
            {
                if (!AudioClipPool.ContainsKey(path))
                {
                    AssetData assetData = ResMgr.Instance.GetAsset(path, false);
                    AudioClipPool?.Add(assetData.Path, assetData);
                }
            }
        }

        public void RemoveClipFromPool(List<string> list)
        {
            if (_disabled)
            {
                return;
            }
            foreach (string path in list)
            {
                if (AudioClipPool.ContainsKey(path))
                {
                    AudioClipPool[path].DecRef();
                    AudioClipPool.Remove(path);
                }
            }
        }

        public void CleanSoundPool()
        {
            if (_disabled)
            {
                return;
            }
            foreach (var dic in AudioClipPool)
            {
                dic.Value.DecRef();
            }
            AudioClipPool.Clear();
        }

        private void Update()
        {
            for (int i = 0; i < _audioAgents.Length; ++i)
            {
                if (_audioAgents[i] != null)
                {
                    _audioAgents[i].Update(Time.deltaTime);
                }
            }
        }
        #endregion
    }

    #region AudioAgent
    public class AudioAgent
    {
        public List<TAudio> _audioObjects;
        AudioMixerGroup _audioMixerGroup;
        int _maxChannel;
        bool _bEnable = true;

        public bool Enable
        {
            get
            {
                return _bEnable;
            }
            set
            {
                if (_bEnable != value)
                {
                    _bEnable = value;
                    if (!_bEnable)
                    {
                        for (int i = 0; i < _audioObjects.Count; ++i)
                        {
                            if (_audioObjects[i] != null)
                            {
                                _audioObjects[i].Stop();
                            }
                        }
                    }
                }
            }
        }


        public AudioAgent(int maxChannel, AudioMixerGroup audioMixerGroup)
        {
            _maxChannel = maxChannel;
            _audioObjects = new List<TAudio>();
            for (int i = 0; i < _maxChannel; i++)
            {
                TAudio tAudio = new TAudio();
                tAudio.Init(audioMixerGroup);
                _audioObjects.Add(tAudio);
            }
            _audioMixerGroup = audioMixerGroup;

        }

        public void AddAudio(int Num)
        {
            _maxChannel += Num;
            for (int i = 0; i < Num; i++)
            {
                _audioObjects.Add(null);
            }
        }



        public TAudio Play(string path, bool bAsync, bool bInPool = false)
        {
            if (!_bEnable)
            {
                return null;
            }
            int freeChannel = -1;
            float duration = -1;
            int num = 0;
            for (int i = 0; i < _audioObjects.Count; ++i)
            {
                if (_audioObjects[i] != null && _audioObjects[i]._assetData != null && _audioObjects[i].IsFinish == false)
                {
                    if (path.Equals(_audioObjects[i]._assetData.Path))
                    {
                        num++;
                    }
                }
            }

            for (int i = 0; i < _audioObjects.Count; i++)
            {
                if (_audioObjects[i]._assetData == null || _audioObjects[i].IsFinish == true)
                {
                    freeChannel = i;

                    break;
                }
                else if (_audioObjects[i].Duration > duration)
                {
                    duration = _audioObjects[i].Duration;

                    freeChannel = i;
                }
            }

            if (freeChannel >= 0)
            {
                if (_audioObjects[freeChannel] == null)
                {
                    _audioObjects[freeChannel] = TAudio.Create(path, bAsync, _audioMixerGroup, bInPool);
                }
                else
                {
                    _audioObjects[freeChannel].Load(path, bAsync, bInPool);
                }
                return _audioObjects[freeChannel];
            }
            else
            {
                TLogger.LogError($"Here is no channel to play audio {path}");
                return null;
            }
        }

        public void Stop(bool fadeout)
        {
            for (int i = 0; i < _audioObjects.Count; ++i)
            {
                if (_audioObjects[i] != null)
                {
                    _audioObjects[i].Stop(fadeout);
                }
            }
        }

        public void Update(float delta)
        {
            for (int i = 0; i < _audioObjects.Count; ++i)
            {
                if (_audioObjects[i] != null)
                {
                    _audioObjects[i].Update(delta);
                }
            }
        }
    }
    #endregion
}
