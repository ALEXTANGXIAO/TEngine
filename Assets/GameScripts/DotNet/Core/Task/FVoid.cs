using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TEngine
{
    [AsyncMethodBuilder(typeof(AsyncFVoidMethodBuilder))]
    [StructLayout(LayoutKind.Auto)]
    internal struct FVoid : ICriticalNotifyCompletion
    {
        [DebuggerHidden]
        public void Coroutine()
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