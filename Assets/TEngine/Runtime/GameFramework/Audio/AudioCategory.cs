using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace TEngine
{
    /// <summary>
    /// 音频轨道（类别）。
    /// </summary>
    [Serializable]
    public class AudioCategory
    {
        [SerializeField]
        private AudioMixer _audioMixer = null;
        public List<AudioAgent> AudioAgents;
        private readonly AudioMixerGroup _audioMixerGroup;
        private int _maxChannel;
        private bool _bEnable = true;
        public AudioMixer AudioMixer => _audioMixer;
        public AudioMixerGroup AudioMixerGroup => _audioMixerGroup;

        public Transform InstanceRoot { private set; get; }

        public bool Enable
        {
            get => _bEnable;
            set
            {
                if (_bEnable != value)
                {
                    _bEnable = value;
                    if (!_bEnable)
                    {
                        foreach (var audioAgent in AudioAgents)
                        {
                            if (audioAgent != null)
                            {
                                audioAgent.Stop();
                            }
                        }
                    }
                }
            }
        }


        public AudioCategory(int maxChannel, AudioMixer audioMixer,AudioType audioType)
        {
            _audioMixer = audioMixer;
            _maxChannel = maxChannel;
            AudioMixerGroup[] audioMixerGroups = audioMixer.FindMatchingGroups(Utility.Text.Format("Master/{0}", audioType.ToString()));
            if (audioMixerGroups.Length > 0)
            {
                _audioMixerGroup = audioMixerGroups[0];
            }
            else
            {
                _audioMixerGroup = audioMixer.FindMatchingGroups("Master")[0];
            }
            AudioAgents = new List<AudioAgent>(16);
            InstanceRoot = new GameObject(Utility.Text.Format("Audio Category - {0}", _audioMixerGroup.name)).transform;
            InstanceRoot.SetParent(GameEntry.GetModule<AudioModule>().InstanceRoot);
            for (int index = 0; index < _maxChannel; index++)
            {
                AudioAgent audioAgent = new AudioAgent();
                audioAgent.Init(this, index);
                AudioAgents.Add(audioAgent);
            }
        }

        public void AddAudio(int num)
        {
            _maxChannel += num;
            for (int i = 0; i < num; i++)
            {
                AudioAgents.Add(null);
            }
        }


        public AudioAgent Play(string path, bool bAsync, bool bInPool = false)
        {
            if (!_bEnable)
            {
                return null;
            }

            int freeChannel = -1;
            float duration = -1;

            for (int i = 0; i < AudioAgents.Count; i++)
            {
                if (AudioAgents[i].assetOperationHandle == null || AudioAgents[i].IsFinish)
                {
                    freeChannel = i;
                    break;
                }
                else if (AudioAgents[i].Duration > duration)
                {
                    duration = AudioAgents[i].Duration;
                    freeChannel = i;
                }
            }

            if (freeChannel >= 0)
            {
                if (AudioAgents[freeChannel] == null)
                {
                    AudioAgents[freeChannel] = AudioAgent.Create(path, bAsync, this, bInPool);
                }
                else
                {
                    AudioAgents[freeChannel].Load(path, bAsync, bInPool);
                }

                return AudioAgents[freeChannel];
            }
            else
            {
                Log.Error($"Here is no channel to play audio {path}");
                return null;
            }
        }

        public void Stop(bool fadeout)
        {
            for (int i = 0; i < AudioAgents.Count; ++i)
            {
                if (AudioAgents[i] != null)
                {
                    AudioAgents[i].Stop(fadeout);
                }
            }
        }

        public void Update(float delta)
        {
            for (int i = 0; i < AudioAgents.Count; ++i)
            {
                if (AudioAgents[i] != null)
                {
                    AudioAgents[i].Update(delta);
                }
            }
        }
    }
}