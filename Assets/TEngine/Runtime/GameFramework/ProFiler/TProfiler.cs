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

        [Conditional("FIRST_PROFILER")]
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

        [Conditional("FIRST_PROFILER")]
        public static void EndFirstSample()
        {
            if (m_currLevel <= m_sampleLevel)
            {
                Profiler.EndSample();
                m_sampleLevel--;
            }

            m_currLevel--;
        }

        [Conditional("T_PROFILER")]
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

        [Conditional("T_PROFILER")]
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