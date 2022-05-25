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
            }
            catch (Exception e)
            {
                TLogger.LogError(e.ToString());
            }
        }

    }
}
