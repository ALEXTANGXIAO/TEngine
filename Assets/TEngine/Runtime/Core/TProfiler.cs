using System.Diagnostics;
using UnityEngine.Profiling;

namespace TEngine
{
    public class TProfiler
    {
        private static int m_profileLevel = -1;
        private static int m_currLevel = 0;
        private static int m_sampleLevel = 0;

        public static void SetProfileLevel(int level)
        {
            m_profileLevel = level;
        }

        [Conditional("UNITY_EDITOR")]
        public static void BeginFirstSample(string name)
        {
            m_currLevel++;
            if (m_profileLevel >= 0 && m_currLevel > m_profileLevel)
            {
                return;
            }

            m_sampleLevel++;
            Profiler.BeginSample(name);
        }

        [Conditional("UNITY_EDITOR")]
        public static void EndFirstSample()
        {
            if (m_currLevel <= m_sampleLevel)
            {
                Profiler.EndSample();
                m_sampleLevel--;
            }

            m_currLevel--;
        }

        [Conditional("UNITY_EDITOR")]
        public static void BeginSample(string name)
        {
            m_currLevel++;
            if (m_profileLevel >= 0 && m_currLevel > m_profileLevel)
            {
                return;
            }

            m_sampleLevel++;
            Profiler.BeginSample(name);
        }

        [Conditional("UNITY_EDITOR")]
        public static void EndSample()
        {
            if (m_currLevel <= m_sampleLevel)
            {
                Profiler.EndSample();
                m_sampleLevel--;
            }

            m_currLevel--;
        }
    }
}
