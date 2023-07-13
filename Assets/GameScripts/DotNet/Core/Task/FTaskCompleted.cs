using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TEngine
{
    [AsyncMethodBuilder(typeof(AsyncFTaskCompletedMethodBuilder))]
    [StructLayout(LayoutKind.Auto)]
    public struct FTaskCompleted : INotifyCompletion
    {
        [DebuggerHidden]
        public FTaskCompleted GetAwaiter()
        {
            return this;
        }

        [DebuggerHidden] public bool IsCompleted => true;

        [DebuggerHidden]
        public void GetResult()
        {
        }

        [DebuggerHidden]
        public void OnCompleted(Action continuation)
        {
        }

        [DebuggerHidden]
        public void UnsafeOnCompleted(Action continuation)
        {
        }
    }
}