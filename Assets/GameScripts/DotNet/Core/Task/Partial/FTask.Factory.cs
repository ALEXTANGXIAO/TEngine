using System;

namespace TEngine
{
    public partial class FTask
    {
        public static FTaskCompleted CompletedTask => new();

        public static FTask Run(Func<FTask> factory)
        {
            return factory();
        }

        public static FTask<T> Run<T>(Func<FTask<T>> factory)
        {
            return factory();
        }

        public static FTask<T> FromResult<T>(T value)
        {
            var sAwaiter = FTask<T>.Create();
            sAwaiter.SetResult(value);
            return sAwaiter;
        }
    }
}