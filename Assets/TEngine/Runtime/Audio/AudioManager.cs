using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TEngine
{
    public enum AudioType
    {
        Sound,
        Music,
        Voice,
        Max
    }

    public class AudioManager : UnitySingleton<AudioManager>
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
}
