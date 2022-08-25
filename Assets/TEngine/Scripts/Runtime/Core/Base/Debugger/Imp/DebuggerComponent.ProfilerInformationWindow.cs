using UnityEngine;
using UnityEngine.Profiling;

namespace TEngine.Runtime
{
    public sealed partial class DebuggerComponent
    {
        private sealed class ProfilerInformationWindow : ScrollableDebuggerWindowBase
        {
            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("<b>Profiler Information</b>");
                GUILayout.BeginVertical("box");
                {
                    DrawItem("Supported", Profiler.supported.ToString());
                    DrawItem("Enabled", Profiler.enabled.ToString());
                    DrawItem("Enable Binary Log", Profiler.enableBinaryLog ? string.Format("True, {0}", Profiler.logFile) : "False");
                    DrawItem("Enable Allocation Callstacks", Profiler.enableAllocationCallstacks.ToString());
                    DrawItem("Area Count", Profiler.areaCount.ToString());
                    DrawItem("Max Used Memory", GetByteLengthString(Profiler.maxUsedMemory));
                    DrawItem("Mono Used Size", GetByteLengthString(Profiler.GetMonoUsedSizeLong()));
                    DrawItem("Mono Heap Size", GetByteLengthString(Profiler.GetMonoHeapSizeLong()));
                    DrawItem("Used Heap Size", GetByteLengthString(Profiler.usedHeapSizeLong));
                    DrawItem("Total Allocated Memory", GetByteLengthString(Profiler.GetTotalAllocatedMemoryLong()));
                    DrawItem("Total Reserved Memory", GetByteLengthString(Profiler.GetTotalReservedMemoryLong()));
                    DrawItem("Total Unused Reserved Memory", GetByteLengthString(Profiler.GetTotalUnusedReservedMemoryLong()));
                    DrawItem("Allocated Memory For Graphics Driver", GetByteLengthString(Profiler.GetAllocatedMemoryForGraphicsDriver()));
                    DrawItem("Temp Allocator Size", GetByteLengthString(Profiler.GetTempAllocatorSize()));
                    DrawItem("Marshal Cached HGlobal Size", GetByteLengthString(Utility.Marshal.CachedHGlobalSize));
                }
                GUILayout.EndVertical();
            }
        }
    }
}
