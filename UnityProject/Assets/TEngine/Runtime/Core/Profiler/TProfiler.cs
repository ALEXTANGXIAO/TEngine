using System.Diagnostics;
using UnityEngine.Profiling;

namespace TEngine
{
    /// <summary>
    /// 游戏框架Profiler分析器类。
    /// </summary>
    public class TProfiler
    {
        private static int _profileLevel = -1;
        private static int _currLevel = 0;
        private static int _sampleLevel = 0;

        /// <summary>
        /// 设置分析器等级。
        /// </summary>
        /// <param name="level">调试器等级。</param>
        public static void SetProfileLevel(int level)
        {
            _profileLevel = level;
        }

        /// <summary>
        /// 开始使用自定义采样分析一段代码。
        /// </summary>
        /// <param name="name">用于在Profiler窗口中标识样本的字符串。</param>
        [Conditional("FIRST_PROFILER")]
        public static void BeginFirstSample(string name)
        {
            _currLevel++;
            if (_profileLevel >= 0 && _currLevel > _profileLevel)
            {
                return;
            }

            _sampleLevel++;
            Profiler.BeginSample(name);
        }

        /// <summary>
        /// 结束本次自定义采样分析。
        /// </summary>
        [Conditional("FIRST_PROFILER")]
        public static void EndFirstSample()
        {
            if (_currLevel <= _sampleLevel)
            {
                Profiler.EndSample();
                _sampleLevel--;
            }

            _currLevel--;
        }

        /// <summary>
        /// 开始使用自定义采样分析一段代码。
        /// </summary>
        /// <param name="name">用于在Profiler窗口中标识样本的字符串。</param>
        [Conditional("T_PROFILER")]
        public static void BeginSample(string name)
        {
            _currLevel++;
            if (_profileLevel >= 0 && _currLevel > _profileLevel)
            {
                return;
            }

            _sampleLevel++;
            Profiler.BeginSample(name);
        }

        /// <summary>
        /// 结束本次自定义采样分析。
        /// </summary>
        [Conditional("T_PROFILER")]
        public static void EndSample()
        {
            if (_currLevel <= _sampleLevel)
            {
                Profiler.EndSample();
                _sampleLevel--;
            }

            _currLevel--;
        }
    }
}