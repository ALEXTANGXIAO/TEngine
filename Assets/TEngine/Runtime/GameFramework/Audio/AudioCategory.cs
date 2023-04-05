using System.Collections.Generic;
using UnityEngine.Audio;

namespace TEngine
{
    public class AudioCategory
    {
        public List<AudioData> _audioObjects;
        AudioMixerGroup _audioMixerGroup;
        int _maxChannel;
        bool _bEnable = true;

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


        public AudioCategory(int maxChannel, AudioMixerGroup audioMixerGroup)
        {
            _maxChannel = maxChannel;
            _audioObjects = new List<AudioData>();
            for (int i = 0; i < _maxChannel; i++)
            {
                AudioData audioData = new AudioData();
                audioData.Init(audioMixerGroup);
                _audioObjects.Add(audioData);
            }

            _audioMixerGroup = audioMixerGroup;
        }

        public void AddAudio(int num)
        {
            _maxChannel += num;
            for (int i = 0; i < num; i++)
            {
                _audioObjects.Add(null);
            }
        }


        public AudioData Play(string path, bool bAsync, bool bInPool = false)
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
                    _audioObjects[freeChannel] = AudioData.Create(path, bAsync, _audioMixerGroup, bInPool);
                else
                    _audioObjects[freeChannel].Load(path, bAsync, bInPool);
                return _audioObjects[freeChannel];
            }
            else
            {
                Log.Error($"Here is no channel to play audio {path}");
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