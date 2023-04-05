using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using YooAsset;

namespace TEngine
{
    public class ChannelNumConfig
    {
        public int MaxChannelNum;
    }

    /// <summary>
    /// 音效管理，为游戏提供统一的音效播放接口。
    /// </summary>
    /// <remarks>场景3D音效挂到场景物件、技能3D音效挂到技能特效上，并在AudioSource的Output上设置对应分类的AudioMixerGroup</remarks>
    public class AudioManager : GameFrameworkModuleBase
    {
        private AudioMixer _audioMixer;
        private float _volume = 1f;
        private bool _enable = true;
        public Dictionary<string, int> _soundConfigDic = new Dictionary<string, int>();
        private int _audioChannelMaxNum = 0;
        AudioCategory[] _audioCategories = new AudioCategory[(int)AudioType.Max];
        float[] _categoriesVolume = new float[(int)AudioType.Max];
        public Dictionary<string, AssetOperationHandle> _audioClipPool = new Dictionary<string, AssetOperationHandle>();

        private bool _bUnityAudioDisabled = false;

        /// <summary>
        /// 总音量控制
        /// </summary>
        public float Volume
        {
            get
            {
                if (_bUnityAudioDisabled)
                    return 0.0f;
                return _volume;
            }
            set
            {
                if (_bUnityAudioDisabled)
                    return;
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
                if (_bUnityAudioDisabled)
                    return false;
                return _enable;
            }
            set
            {
                if (_bUnityAudioDisabled)
                    return;
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
                    return 0.0f;
                return _categoriesVolume[(int)AudioType.Music];
            }
            set
            {
                if (_bUnityAudioDisabled)
                    return;
                float volume = Mathf.Clamp(value, 0.0001f, 1.0f);
                _categoriesVolume[(int)AudioType.Music] = volume;

                _audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20f);
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
                    return 0.0f;
                return _categoriesVolume[(int)AudioType.Sound];
            }
            set
            {
                if (_bUnityAudioDisabled)
                    return;
                float volume = Mathf.Clamp(value, 0.0001f, 1.0f);
                _categoriesVolume[(int)AudioType.Sound] = volume;
                _audioMixer.SetFloat("SoundVolume", Mathf.Log10(volume) * 20f);
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
                    return 0.0f;
                return _categoriesVolume[(int)AudioType.Voice];
            }
            set
            {
                if (_bUnityAudioDisabled)
                    return;
                float volume = Mathf.Clamp(value, 0.0001f, 1.0f);
                _categoriesVolume[(int)AudioType.Voice] = volume;
                _audioMixer.SetFloat("VoiceVolume", Mathf.Log10(volume) * 20f);
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
                    return false;
                float db;
                if (_audioMixer.GetFloat("MusicVolume", out db))
                    return db > -80f;
                else
                    return false;
            }
            set
            {
                if (_bUnityAudioDisabled)
                    return;
                // 音乐采用0音量方式，避免恢复播放时的复杂逻辑
                if (value)
                    _audioMixer.SetFloat("MusicVolume", Mathf.Log10(_categoriesVolume[(int)AudioType.Music]) * 20f);
                else
                    _audioMixer.SetFloat("MusicVolume", -80f);
            }
        }

        /// <summary>
        /// 音效开关
        /// </summary>
        public bool SoundEnable
        {
            get
            {
                if (_bUnityAudioDisabled)
                    return false;
                return _audioCategories[(int)AudioType.Sound].Enable;
            }
            set
            {
                if (_bUnityAudioDisabled)
                    return;
                _audioCategories[(int)AudioType.Sound].Enable = value;
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
                    return false;
                return _audioCategories[(int)AudioType.Voice].Enable;
            }
            set
            {
                if (_bUnityAudioDisabled)
                    return;
                _audioCategories[(int)AudioType.Voice].Enable = value;
            }
        }

        internal AudioMixer audioMixer
        {
            get { return _audioMixer; }
        }

        void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                TypeInfo typeInfo = typeof(AudioSettings).GetTypeInfo();
                PropertyInfo propertyInfo = typeInfo.GetDeclaredProperty("unityAudioDisabled");
                _bUnityAudioDisabled = (bool)propertyInfo.GetValue(null);
                if (_bUnityAudioDisabled)
                {
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }

            _audioMixer = Resources.Load<AudioMixer>("AudioMixer");
            
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
                _audioCategories[i] = new AudioCategory(channelMaxNum, audioMixer.FindMatchingGroups(((AudioType)i).ToString())[0]);
                _categoriesVolume[i] = 1.0f;
            }
        }

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
                    for (int j = 0; j < _audioCategories[i]._audioObjects.Count; ++j)
                    {
                        if (_audioCategories[i]._audioObjects[j] != null)
                        {
                            _audioCategories[i]._audioObjects[j].Destroy();
                            _audioCategories[i]._audioObjects[j] = null;
                        }
                    }
                }

                _audioCategories[i] = null;
            }

            Initialize();
        }

        /// <summary>
        /// 播放，如果超过最大发声数采用fadeout的方式复用最久播放的AudioSource
        /// </summary>
        /// <param name="type">声音类型</param>
        /// <param name="path">声音文件路径，通过右键菜单Get Asset Path获取的路径</param>
        /// <param name="bLoop">是否循环播放</param>>
        /// <param name="volume">音量（0-1.0）</param>
        /// <param name="bAsync">是否异步加载</param>
        public AudioData Play(AudioType type, string path, bool bLoop = false, float volume = 1.0f, bool bAsync = false, bool bInPool = false)
        {
            if (_bUnityAudioDisabled)
            {
                return null;
            }
            AudioData audioData = _audioCategories[(int)type].Play(path, bAsync, bInPool);
            {
                if (audioData != null)
                {
                    audioData.IsLoop = bLoop;
                    audioData.Volume = volume;
                }

                return audioData;
            }
        }

        /// <summary>
        /// 停止某类声音播放
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
        /// 停止所有声音
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
        /// 修改最大的音效播放上限，
        /// </summary>
        /// <param name="num"></param> 最大播放数量
        public void ChangeAudioChannelMaxNum(int num)
        {
            if (_bUnityAudioDisabled)
            {
                return;
            }
            if (num >= _audioChannelMaxNum)
            {
                _audioCategories[(int)AudioType.Sound].AddAudio(num - _audioChannelMaxNum);
                _audioChannelMaxNum = num;
            }
            else
            {
                Stop(AudioType.Sound, true);
                _audioChannelMaxNum = num;
                _audioCategories[(int)AudioType.Sound].Enable = false;
                _audioCategories[(int)AudioType.Sound] = new AudioCategory(_audioChannelMaxNum, _audioMixer.FindMatchingGroups((AudioType.Sound).ToString())[0]);
                _categoriesVolume[(int)AudioType.Sound] = 1.0f;
            }
        }


        /// <summary>
        /// 预先加载AudioClip，并放入对象池
        /// </summary>
        /// <param name="list"></param>AudioClip的AssetPath集合
        public void PutInAudioPool(List<string> list)
        {
            if (_bUnityAudioDisabled)
            {
                return;
            }
            foreach (string path in list)
            {
                if (_audioClipPool != null && !_audioClipPool.ContainsKey(path))
                {
                    AssetOperationHandle assetData = AssetManager.Instance.GetAsset(path, false);
                    _audioClipPool?.Add(assetData.Path, assetData);
                }
            }
        }

        /// <summary>
        /// 将部分AudioClip从对象池移出
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
                if (_audioClipPool.ContainsKey(path))
                {
                    _audioClipPool[path].Dispose();
                    _audioClipPool.Remove(path);
                }
            }
        }

        /// <summary>
        /// 清空AudioClip的对象池
        /// </summary>
        public void CleanSoundPool()
        {
            if (_bUnityAudioDisabled)
            {
                return;
            }
            foreach (var dic in _audioClipPool)
            {
                dic.Value.Dispose();
            }

            _audioClipPool.Clear();
        }

        private void Update()
        {
            for (int i = 0; i < _audioCategories.Length; ++i)
            {
                if (_audioCategories[i] != null)
                {
                    _audioCategories[i].Update(Time.deltaTime);
                }
            }
        }
    }
}