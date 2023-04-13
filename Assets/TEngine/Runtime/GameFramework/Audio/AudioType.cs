namespace TEngine
{
    /// <summary>
    /// 音效分类，可分别关闭/开启对应分类音效。
    /// </summary>
    /// <remarks>命名与AudioMixer中分类名保持一致。</remarks>
    public enum AudioType
    {
        /// <summary>
        /// 声音
        /// </summary>
        Sound,
        /// <summary>
        /// UI声效
        /// </summary>
        UISound,
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
}