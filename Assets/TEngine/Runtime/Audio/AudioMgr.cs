using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;

namespace TEngine
{
    public enum AudioType
    {
        Sound,
        Music,
        Voice,
        Max
    }

    public class AudioMgr : UnitySingleton<AudioMgr>
    {
        private AudioAgent[] _audioAgents = new AudioAgent[(int)AudioType.Max];
        public Dictionary<string, int> _soundConfigDic = new Dictionary<string, int>();
        public Dictionary<string, AssetData> AudioClipPool = new Dictionary<string, AssetData>();
        private float _volume = 1f;
        private bool _enable = true;
        private bool _bUnityAudioDisabled = false;

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

        protected override void OnLoad()
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
                //_soundConfigDic = JsonConvert.DeserializeObject<Dictionary<string, int>>(str);
            }
            catch (Exception e)
            {
                TLogger.LogError(e.ToString());
            }
        }

        #region 外部调用播放操作
        public TAudio Play(AudioType type, string path, bool bLoop = false, float volume = 1.0f, bool bAsync = false, bool bInPool = false)
        {
            if (_bUnityAudioDisabled)
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
            if (_bUnityAudioDisabled)
            {
                return;
            }
            _audioAgents[(int)type].Stop(fadeout);
        }

        public void StopAll(bool fadeout)
        {
            if (_bUnityAudioDisabled)
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
            if (_bUnityAudioDisabled)
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
            if (_bUnityAudioDisabled)
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
            if (_bUnityAudioDisabled)
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
            if (_bUnityAudioDisabled)
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
            if (!_bEnable) return null;
            int freeChannel = -1;
            float duration = -1;
            int num = 0;
            for (int i = 0; i < _audioObjects.Count; ++i)
            {
                if (_audioObjects[i] != null && _audioObjects[i]._assetData != null && _audioObjects[i].IsFinish == false)
                {
                    if (path.Equals(_audioObjects[i]._assetData.Path))
                        num++;
                }
            }

            for (int i = 0; i < _audioObjects.Count; i++)
            {
                if (AudioMgr.Instance._soundConfigDic != null && AudioMgr.Instance._soundConfigDic.ContainsKey(path) && AudioMgr.Instance._soundConfigDic[path] == num)
                {
                    if (_audioObjects[i] != null && _audioObjects[i]._assetData != null && path == _audioObjects[i]._assetData.Path)
                    {
                        if (_audioObjects[i].Duration > duration)
                        {
                            duration = _audioObjects[i].Duration;
                            freeChannel = i;
                        }
                    }
                }
                else
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
            }

            if (freeChannel >= 0)
            {
                if (_audioObjects[freeChannel] == null)
                    _audioObjects[freeChannel] = TAudio.Create(path, bAsync, _audioMixerGroup, bInPool);
                else
                    _audioObjects[freeChannel].Load(path, bAsync, bInPool);
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
}
