using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TEngine
{
    public partial class FTask
    {
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FTask Create(bool isFromPool = true)
        {
            var task = isFromPool ? Pool<FTask>.Rent() : new FTask();
            task._isFromPool = isFromPool;
            return task;
        }
    }

    public partial class FTask<T>
    {
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FTask<T> Create(bool isFromPool = true)
        {
            var task = isFromPool ? Pool<FTask<T>>.Rent() : new FTask<T>();
            task._isFromPool = isFromPool;
            return task;
        }
    }
}