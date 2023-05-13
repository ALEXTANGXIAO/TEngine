using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;
using YooAsset;

namespace TEngine
{
    /// <summary>
    /// 音效管理，为游戏提供统一的音效播放接口。
    /// </summary>
    /// <remarks>场景3D音效挂到场景物件、技能3D音效挂到技能特效上，并在AudioSource的Output上设置对应分类的AudioMixerGroup</remarks>
    public class AudioModule : GameFrameworkModuleBase
    {
        [SerializeField] private AudioMixer m_AudioMixer;

        [SerializeField] private Transform m_InstanceRoot = null;
        
        [SerializeField]
        private AudioGroupConfig[] m_AudioGroupConfigs = null;
        
        private float _volume = 1f;
        private bool _enable = true;
        private AudioCategory[] _audioCategories = new AudioCategory[(int)AudioType.Max];
        private readonly float[] _categoriesVolume = new float[(int)AudioType.Max];
        public readonly Dictionary<string, AssetOperationHandle> AudioClipPool = new Dictionary<string, AssetOperationHandle>();
        public IResourceManager ResourceManager;
        private bool _bUnityAudioDisabled = false;

        #region Public Propreties

        /// <summary>
        /// 音频混响器。
        /// </summary>
        public AudioMixer MAudioMixer => m_AudioMixer;

        /// <summary>
        /// 实例化根节点。
        /// </summary>
        public Transform InstanceRoot => m_InstanceRoot;

        /// <summary>
        /// 总音量控制。
        /// </summary>
        public float Volume
        {
            get
            {
                if (_bUnityAudioDisabled)
                {
                    return 0.0f;
                }

                return _volume;
            }
            set
            {
                if (_bUnityAudioDisabled)
                {
                    return;
                }

                _volume = value;
                AudioListener.volume = _volume;
            }
        }

        /// <summary>
        /// 总开关。
        /// </summary>
        public bool Enable
        {
            get
            {
                if (_bUnityAudioDisabled)
                {
                    return false;
                }

                return _enable;
            }
            set
            {
                if (_bUnityAudioDisabled)
                {
                    return;
                }

                _enable = value;
                AudioListener.volume = _enable ? _volume : 0f;
            }
        }

        /// <summary>
        /// 音乐音量
        /// </summary>
        public float MusicVolume
        {
            get
            {
                if (_bUnityAudioDisabled)
                {
                    return 0.0f;
                }

                return _categoriesVolume[(int)AudioType.Music];
            }
            set
            {
                if (_bUnityAudioDisabled)
                {
                    return;
                }

                float volume = Mathf.Clamp(value, 0.0001f, 1.0f);
                _categoriesVolume[(int)AudioType.Music] = volume;
                m_AudioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20f);
            }
        }

        /// <summary>
        /// 音效音量
        /// </summary>
        public float SoundVolume
        {
            get
            {
                if (_bUnityAudioDisabled)
                {
                    return 0.0f;
                }

                return _categoriesVolume[(int)AudioType.Sound];
            }
            set
            {
                if (_bUnityAudioDisabled)
                {
                    return;
                }

                float volume = Mathf.Clamp(value, 0.0001f, 1.0f);
                _categoriesVolume[(int)AudioType.Sound] = volume;
                m_AudioMixer.SetFloat("SoundVolume", Mathf.Log10(volume) * 20f);
            }
        }

        /// <summary>
        /// 音效音量
        /// </summary>
        public float UISoundVolume
        {
            get
            {
                if (_bUnityAudioDisabled)
                {
                    return 0.0f;
                }

                return _categoriesVolume[(int)AudioType.UISound];
            }
            set
            {
                if (_bUnityAudioDisabled)
                {
                    return;
                }

                float volume = Mathf.Clamp(value, 0.0001f, 1.0f);
                _categoriesVolume[(int)AudioType.UISound] = volume;
                m_AudioMixer.SetFloat("UISoundVolume", Mathf.Log10(volume) * 20f);
            }
        }        
        
        /// <summary>
        /// 语音音量
        /// </summary>
        public float VoiceVolume
        {
            get
            {
                if (_bUnityAudioDisabled)
                {
                    return 0.0f;
                }

                return _categoriesVolume[(int)AudioType.Voice];
            }
            set
            {
                if (_bUnityAudioDisabled)
                {
                    return;
                }

                float volume = Mathf.Clamp(value, 0.0001f, 1.0f);
                _categoriesVolume[(int)AudioType.Voice] = volume;
                m_AudioMixer.SetFloat("VoiceVolume", Mathf.Log10(volume) * 20f);
            }
        }

        /// <summary>
        /// 音乐开关
        /// </summary>
        public bool MusicEnable
        {
            get
            {
                if (_bUnityAudioDisabled)
                {
                    return false;
                }

                if (m_AudioMixer.GetFloat("MusicVolume", out var db))
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
                if (_bUnityAudioDisabled)
                {
                    return;
                }
                
                _audioCategories[(int)AudioType.Music].Enable = value;

                // 音乐采用0音量方式，避免恢复播放时的复杂逻辑
                if (value)
                {
                    m_AudioMixer.SetFloat("MusicVolume", Mathf.Log10(_categoriesVolume[(int)AudioType.Music]) * 20f);
                }
                else
                {
                    m_AudioMixer.SetFloat("MusicVolume", -80f);
                }
            }
        }

        /// <summary>
        /// 音效开关。
        /// </summary>
        public bool SoundEnable
        {
            get
            {
                if (_bUnityAudioDisabled)
                {
                    return false;
                }

                return _audioCategories[(int)AudioType.Sound].Enable;
            }
            set
            {
                if (_bUnityAudioDisabled)
                {
                    return;
                }

                _audioCategories[(int)AudioType.Sound].Enable = value;
            }
        }
        
        /// <summary>
        /// 音效开关。
        /// </summary>
        public bool UISoundEnable
        {
            get
            {
                if (_bUnityAudioDisabled)
                {
                    return false;
                }

                return _audioCategories[(int)AudioType.UISound].Enable;
            }
            set
            {
                if (_bUnityAudioDisabled)
                {
                    return;
                }

                _audioCategories[(int)AudioType.UISound].Enable = value;
            }
        }

        /// <summary>
        /// 语音开关
        /// </summary>
        public bool VoiceEnable
        {
            get
            {
                if (_bUnityAudioDisabled)
                {
                    return false;
                }

                return _audioCategories[(int)AudioType.Voice].Enable;
            }
            set
            {
                if (_bUnityAudioDisabled)
                {
                    return;
                }

                _audioCategories[(int)AudioType.Voice].Enable = value;
            }
        }

        #endregion


        void Start()
        {
            Initialize();
        }

        /// <summary>
        /// 初始化音频模块。
        /// </summary>
        private void Initialize()
        {
            RootModule rootModule = GameEntry.GetModule<RootModule>();
            if (rootModule == null)
            {
                Log.Fatal("Base component is invalid.");
                return;
            }

            ResourceManager = GameFrameworkEntry.GetModule<IResourceManager>();

            if (m_InstanceRoot == null)
            {
                m_InstanceRoot = new GameObject("AudioModule Instances").transform;
                m_InstanceRoot.SetParent(gameObject.transform);
                m_InstanceRoot.localScale = Vector3.one;
            }

            try
            {
#if UNITY_EDITOR
                TypeInfo typeInfo = typeof(AudioSettings).GetTypeInfo();
                PropertyInfo propertyInfo = typeInfo.GetDeclaredProperty("unityAudioDisabled");
                _bUnityAudioDisabled = (bool)propertyInfo.GetValue(null);
                if (_bUnityAudioDisabled)
                {
                    return;
                }
#endif
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }

            if (m_AudioMixer == null)
            {
                m_AudioMixer = Resources.Load<AudioMixer>("AudioMixer");
            }

            for (int index = 0; index < (int)AudioType.Max; ++index)
            {
                AudioType audioType = (AudioType)index;
                AudioGroupConfig audioGroupConfig = m_AudioGroupConfigs.First(t => t.AudioType == audioType);
                _audioCategories[index] = new AudioCategory(audioGroupConfig.AgentHelperCount, m_AudioMixer, audioGroupConfig);
                _categoriesVolume[index] = audioGroupConfig.Volume;
            }
        }

        /// <summary>
        /// 重启音频模块。
        /// </summary>
        public void Restart()
        {
            if (_bUnityAudioDisabled)
            {
                return;
            }

            CleanSoundPool();

            for (int i = 0; i < (int)AudioType.Max; ++i)
            {
                if (_audioCategories[i] != null)
                {
                    for (int j = 0; j < _audioCategories[i].AudioAgents.Count; ++j)
                    {
                        if (_audioCategories[i].AudioAgents[j] != null)
                        {
                            _audioCategories[i].AudioAgents[j].Destroy();
                            _audioCategories[i].AudioAgents[j] = null;
                        }
                    }
                }

                _audioCategories[i] = null;
            }

            Initialize();
        }

        /// <summary>
        /// 播放，如果超过最大发声数采用fadeout的方式复用最久播放的AudioSource。
        /// </summary>
        /// <param name="type">声音类型</param>
        /// <param name="path">声音文件路径</param>
        /// <param name="bLoop">是否循环播放</param>>
        /// <param name="volume">音量（0-1.0）</param>
        /// <param name="bAsync">是否异步加载</param>
        /// <param name="bInPool">是否支持资源池</param>
        public AudioAgent Play(AudioType type, string path, bool bLoop = false, float volume = 1.0f, bool bAsync = false, bool bInPool = false)
        {
            if (_bUnityAudioDisabled)
            {
                return null;
            }

            AudioAgent audioAgent = _audioCategories[(int)type].Play(path, bAsync, bInPool);
            {
                if (audioAgent != null)
                {
                    audioAgent.IsLoop = bLoop;
                    audioAgent.Volume = volume;
                }

                return audioAgent;
            }
        }

        /// <summary>
        /// 停止某类声音播放。
        /// </summary>
        /// <param name="type">声音类型</param>
        /// <param name="fadeout">是否渐消</param>
        public void Stop(AudioType type, bool fadeout)
        {
            if (_bUnityAudioDisabled)
            {
                return;
            }

            _audioCategories[(int)type].Stop(fadeout);
        }

        /// <summary>
        /// 停止所有声音。
        /// </summary>
        /// <param name="fadeout">是否渐消</param>
        public void StopAll(bool fadeout)
        {
            if (_bUnityAudioDisabled)
            {
                return;
            }

            for (int i = 0; i < (int)AudioType.Max; ++i)
            {
                if (_audioCategories[i] != null)
                {
                    _audioCategories[i].Stop(fadeout);
                }
            }
        }
        
        /// <summary>
        /// 预先加载AudioClip，并放入对象池。
        /// </summary>
        /// <param name="list"></param>AudioClip的AssetPath集合。
        public void PutInAudioPool(List<string> list)
        {
            if (_bUnityAudioDisabled)
            {
                return;
            }

            foreach (string path in list)
            {
                if (AudioClipPool != null && !AudioClipPool.ContainsKey(path))
                {
                    AssetOperationHandle assetData = ResourceManager.LoadAssetGetOperation<AudioClip>(path);
                    AudioClipPool?.Add(path, assetData);
                }
            }
        }

        /// <summary>
        /// 将部分AudioClip从对象池移出。
        /// </summary>
        /// <param name="list"></param>AudioClip的AssetPath集合
        public void RemoveClipFromPool(List<string> list)
        {
            if (_bUnityAudioDisabled)
            {
                return;
            }

            foreach (string path in list)
            {
                if (AudioClipPool.ContainsKey(path))
                {
                    AudioClipPool[path].Dispose();
                    AudioClipPool.Remove(path);
                }
            }
        }

        /// <summary>
        /// 清空AudioClip的对象池。
        /// </summary>
        public void CleanSoundPool()
        {
            if (_bUnityAudioDisabled)
            {
                return;
            }

            foreach (var dic in AudioClipPool)
            {
                dic.Value.Dispose();
            }

            AudioClipPool.Clear();
        }

        /// <summary>
        /// 音频模块轮询。
        /// </summary>
        private void Update()
        {
            for (int i = 0; i < _audioCategories.Length; ++i)
            {
                if (_audioCategories[i] != null)
                {
                    _audioCategories[i].Update(GameTime.deltaTime);
                }
            }
        }
    }
}