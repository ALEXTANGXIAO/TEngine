using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace TEngine
{
    /// <summary>
    /// 音效管理，为游戏提供统一的音效播放接口。
    /// </summary>
    /// <remarks>场景3D音效挂到场景物件、技能3D音效挂到技能特效上，并在AudioSource的Output上设置对应分类的AudioMixerGroup</remarks>
    public class AudioModule : Module
    {
        [SerializeField] private AudioMixer m_AudioMixer;

        [SerializeField] private Transform m_InstanceRoot = null;

        [SerializeField] private AudioGroupConfig[] m_AudioGroupConfigs = null;

        public IAudioModule AudioModuleImp;

        #region Public Propreties

        /// <summary>
        /// 音频混响器。
        /// </summary>
        public AudioMixer MAudioMixer => m_AudioMixer;

        /// <summary>
        /// 实例化根节点。
        /// </summary>
        public Transform InstanceRoot
        {
            get => m_InstanceRoot;
            set => m_InstanceRoot = value;
        }

        /// <summary>
        /// 总音量控制。
        /// </summary>
        public float Volume
        {
            get => AudioModuleImp.Volume;
            set => AudioModuleImp.Volume = value;
        }

        /// <summary>
        /// 总开关。
        /// </summary>
        public bool Enable
        {
            get => AudioModuleImp.Enable;
            set => AudioModuleImp.Enable = value;
        }

        /// <summary>
        /// 音乐音量。
        /// </summary>
        public float MusicVolume
        {
            get => AudioModuleImp.MusicVolume;
            set => AudioModuleImp.MusicVolume = value;
        }

        /// <summary>
        /// 音效音量。
        /// </summary>
        public float SoundVolume
        {
            get => AudioModuleImp.SoundVolume;
            set => AudioModuleImp.SoundVolume = value;
        }

        /// <summary>
        /// UI音效音量。
        /// </summary>
        public float UISoundVolume
        {
            get => AudioModuleImp.UISoundVolume;
            set => AudioModuleImp.UISoundVolume = value;
        }

        /// <summary>
        /// 语音音量。
        /// </summary>
        public float VoiceVolume
        {
            get => AudioModuleImp.VoiceVolume;
            set => AudioModuleImp.VoiceVolume = value;
        }

        /// <summary>
        /// 音乐开关
        /// </summary>
        public bool MusicEnable
        {
            get => AudioModuleImp.MusicEnable;
            set => AudioModuleImp.MusicEnable = value;
        }

        /// <summary>
        /// 音效开关。
        /// </summary>
        public bool SoundEnable
        {
            get => AudioModuleImp.SoundEnable;
            set => AudioModuleImp.SoundEnable = value;
        }

        /// <summary>
        /// UI音效开关。
        /// </summary>
        public bool UISoundEnable
        {
            get => AudioModuleImp.UISoundEnable;
            set => AudioModuleImp.UISoundEnable = value;
        }

        /// <summary>
        /// 语音开关。
        /// </summary>
        public bool VoiceEnable
        {
            get => AudioModuleImp.VoiceEnable;
            set => AudioModuleImp.VoiceEnable = value;
        }

        #endregion

        /// <summary>
        /// 初始化音频模块。
        /// </summary>
        void Start()
        {
            if (AudioModuleImp == null)
            {
                AudioModuleImp = ModuleImpSystem.GetModule<AudioModuleImp>();
            }

            if (m_InstanceRoot == null)
            {
                m_InstanceRoot = new GameObject("AudioModule Instances").transform;
                m_InstanceRoot.SetParent(gameObject.transform);
                m_InstanceRoot.localScale = Vector3.one;
            }

            AudioModuleImp.Initialize(m_AudioGroupConfigs, m_InstanceRoot, m_AudioMixer);
        }

        /// <summary>
        /// 重启音频模块。
        /// </summary>
        public void Restart()
        {
            AudioModuleImp.Restart();
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
            return AudioModuleImp.Play(type, path, bLoop, volume, bAsync, bInPool);
        }

        /// <summary>
        /// 停止某类声音播放。
        /// </summary>
        /// <param name="type">声音类型。</param>
        /// <param name="fadeout">是否渐消。</param>
        public void Stop(AudioType type, bool fadeout)
        {
            AudioModuleImp.Stop(type, fadeout);
        }

        /// <summary>
        /// 停止所有声音。
        /// </summary>
        /// <param name="fadeout">是否渐消。</param>
        public void StopAll(bool fadeout)
        {
            AudioModuleImp.StopAll(fadeout);
        }

        /// <summary>
        /// 预先加载AudioClip，并放入对象池。
        /// </summary>
        /// <param name="list">AudioClip的AssetPath集合。</param>
        public void PutInAudioPool(List<string> list)
        {
            AudioModuleImp.PutInAudioPool(list);
        }

        /// <summary>
        /// 将部分AudioClip从对象池移出。
        /// </summary>
        /// <param name="list">AudioClip的AssetPath集合。</param>
        public void RemoveClipFromPool(List<string> list)
        {
            AudioModuleImp.RemoveClipFromPool(list);
        }

        /// <summary>
        /// 清空AudioClip的对象池。
        /// </summary>
        public void CleanSoundPool()
        {
            AudioModuleImp.CleanSoundPool();
        }
    }
}