using UnityEngine;

namespace TEngine
{
    public static partial class Utility
    {
        /// <summary>
        /// 硬件设备性能适配工具相关的实用函数。
        /// </summary>
        public static class DevicePerformance
        {
            /// <summary>
            /// 获取设备性能评级。
            /// </summary>
            /// <returns>性能评级</returns>
            public static DevicePerformanceLevel GetDevicePerformanceLevel()
            {
                if (SystemInfo.graphicsDeviceVendorID == 32902)
                {
                    //集显
                    return DevicePerformanceLevel.Low;
                }
                else //NVIDIA 系列显卡（N卡）和AMD系列显卡。
                {
                    //根据目前硬件配置三个平台设置了不一样的评判标准（仅个人意见）。
                    //CPU核心数。
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
                    if (SystemInfo.processorCount <= 2)
#elif UNITY_STANDALONE_OSX || UNITY_IPHONE
                    if (SystemInfo.processorCount < 2)
#elif UNITY_ANDROID
                    if (SystemInfo.processorCount <= 4)
#endif
                    {
                        //CPU核心数<=2判定为低端。
                        return DevicePerformanceLevel.Low;
                    }
                    else
                    {
                        //显存。
                        int graphicsMemorySize = SystemInfo.graphicsMemorySize;
                        //内存。
                        int systemMemorySize = SystemInfo.systemMemorySize;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
                        if (graphicsMemorySize >= 4000 && systemMemorySize >= 8000)
                            return DevicePerformanceLevel.High;
                        else if (graphicsMemorySize >= 2000 && systemMemorySize >= 4000)
                            return DevicePerformanceLevel.Mid;
                        else
                            return DevicePerformanceLevel.Low;
#elif UNITY_STANDALONE_OSX || UNITY_IPHONE
                        if (graphicsMemorySize >= 4000 && systemMemorySize >= 8000)
                            return DevicePerformanceLevel.High;
                        else if (graphicsMemorySize >= 2000 && systemMemorySize >= 4000)
                            return DevicePerformanceLevel.Mid;
                        else
                            return DevicePerformanceLevel.Low;
#elif UNITY_ANDROID
                        if (graphicsMemorySize >= 6000 && systemMemorySize >= 8000)
                            return DevicePerformanceLevel.High;
                        else if (graphicsMemorySize >= 2000 && systemMemorySize >= 4000)
                            return DevicePerformanceLevel.Mid;
                        else
                            return DevicePerformanceLevel.Low;
#endif
                    }
                }
            }

            /// <summary>
            /// 根据手机性能修改项目设置。
            /// </summary>
            /// <param name="lowQuality">QualitySettings中对应Low的等级。</param>
            /// <param name="midQuality">QualitySettings中对应Mid的等级。</param>
            /// <param name="highQuality">QualitySettings中对应High的等级。</param>
            public static void ModifySettingsBasedOnPerformance(int lowQuality, int midQuality, int highQuality)
            {
                DevicePerformanceLevel level = GetDevicePerformanceLevel();
                switch (level)
                {
                    case DevicePerformanceLevel.Low:
                        QualitySettings.SetQualityLevel(lowQuality, true);
                        break;
                    case DevicePerformanceLevel.Mid:
                        QualitySettings.SetQualityLevel(midQuality, true);
                        break;
                    case DevicePerformanceLevel.High:
                        QualitySettings.SetQualityLevel(highQuality, true);
                        break;
                }
            }

            /// <summary>
            /// 根据机型配置自动设置质量。
            /// </summary>
            public static void ModifySettingsBasedOnPerformance()
            {
                DevicePerformanceLevel level = GetDevicePerformanceLevel();
                switch (level)
                {
                    case DevicePerformanceLevel.Low:
                        SetQualitySettings(QualityLevel.Low);
                        break;
                    case DevicePerformanceLevel.Mid:
                        SetQualitySettings(QualityLevel.Mid);
                        break;
                    case DevicePerformanceLevel.High:
                        SetQualitySettings(QualityLevel.High);
                        break;
                }
            }

            /// <summary>
            /// 根据自身需要调整各级别需要修改的设置，可根据需求修改低中高三种方案某一项具体设置。
            /// </summary>
            /// <param name="qualityLevel">质量等级。</param>
            public static void SetQualitySettings(QualityLevel qualityLevel)
            {
                switch (qualityLevel)
                {
                    case QualityLevel.Low:
                        //前向渲染使用的像素灯的最大数量，建议最少为1。
                        QualitySettings.pixelLightCount = 2;
                        //你可以设置使用最大分辨率的纹理或者部分纹理（低分辨率纹理的处理开销低）。选项有 0_完整分辨率，1_1/2分辨率，2_1/4分辨率，3_1/8分辨率。
                        QualitySettings.masterTextureLimit = 1;
                        //设置抗锯齿级别。选项有​​ 0_不开启抗锯齿，2_2倍，4_4倍和8_8倍采样。
                        QualitySettings.antiAliasing = 0;
                        //是否使用粒子软融合
                        QualitySettings.softParticles = false;
                        //启用实时反射探针，此设置需要用的时候再打开。
                        QualitySettings.realtimeReflectionProbes = false;
                        //如果启用，公告牌将面向摄像机位置而不是摄像机方向。似乎与地形系统有关，此处没啥必要打开。
                        QualitySettings.billboardsFaceCameraPosition = false;
                        //设置软硬阴影是否打开
                        QualitySettings.shadows = ShadowQuality.Disable;
                        //设置垂直同步方案，VSyncs数值需要在每帧之间传递，使用0为不等待垂直同步。值必须是0，1或2。
                        QualitySettings.vSyncCount = 0;
                        break;
                    case QualityLevel.Mid:
                        QualitySettings.pixelLightCount = 4;
                        QualitySettings.antiAliasing = 2;
                        QualitySettings.softParticles = false;
                        QualitySettings.realtimeReflectionProbes = true;
                        QualitySettings.billboardsFaceCameraPosition = true;
                        QualitySettings.shadows = ShadowQuality.HardOnly;
                        QualitySettings.vSyncCount = 2;
                        break;
                    case QualityLevel.High:
                        QualitySettings.pixelLightCount = 4;
                        QualitySettings.antiAliasing = 8;
                        QualitySettings.softParticles = true;
                        QualitySettings.realtimeReflectionProbes = true;
                        QualitySettings.billboardsFaceCameraPosition = true;
                        QualitySettings.shadows = ShadowQuality.All;
                        QualitySettings.vSyncCount = 2;
                        break;
                }
            }
        }

        public enum DevicePerformanceLevel
        {
            Low,
            Mid,
            High
        }

        public enum QualityLevel
        {
            Low,
            Mid,
            High
        }
    }
}