using UnityEngine;

namespace TEngine.Runtime
{
    public sealed partial class DebuggerComponent
    {
        private sealed class SystemInformationWindow : ScrollableDebuggerWindowBase
        {
            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("<b>System Information</b>");
                GUILayout.BeginVertical("box");
                {
                    DrawItem("Device Unique ID", SystemInfo.deviceUniqueIdentifier);
                    DrawItem("Device Name", SystemInfo.deviceName);
                    DrawItem("Device Type", SystemInfo.deviceType.ToString());
                    DrawItem("Device Model", SystemInfo.deviceModel);
                    DrawItem("Processor Type", SystemInfo.processorType);
                    DrawItem("Processor Count", SystemInfo.processorCount.ToString());
                    DrawItem("Processor Frequency", string.Format("{0} MHz", SystemInfo.processorFrequency.ToString()));
                    DrawItem("System Memory Size", string.Format("{0} MB", SystemInfo.systemMemorySize.ToString()));
                    DrawItem("Operating System Family", SystemInfo.operatingSystemFamily.ToString());
                    DrawItem("Operating System", SystemInfo.operatingSystem);
                    DrawItem("Battery Status", SystemInfo.batteryStatus.ToString());
                    DrawItem("Battery Level", GetBatteryLevelString(SystemInfo.batteryLevel));
                    DrawItem("Supports Audio", SystemInfo.supportsAudio.ToString());
                    DrawItem("Supports Location Service", SystemInfo.supportsLocationService.ToString());
                    DrawItem("Supports Accelerometer", SystemInfo.supportsAccelerometer.ToString());
                    DrawItem("Supports Gyroscope", SystemInfo.supportsGyroscope.ToString());
                    DrawItem("Supports Vibration", SystemInfo.supportsVibration.ToString());
                    DrawItem("Genuine", Application.genuine.ToString());
                    DrawItem("Genuine Check Available", Application.genuineCheckAvailable.ToString());
                }
                GUILayout.EndVertical();
            }

            private string GetBatteryLevelString(float batteryLevel)
            {
                if (batteryLevel < 0f)
                {
                    return "Unavailable";
                }

                return batteryLevel.ToString("P0");
            }
        }
    }
}
