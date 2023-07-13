using System.Collections.Generic;

namespace TEngine
{
    public partial class FTask
    {
        public static async FTask WhenAll(List<FTask> tasks)
        {
            if (tasks.Count <= 0)
            {
                return;
            }
            
            var count = tasks.Count;
            var sTaskCompletionSource = Create();
     
            foreach (var task in tasks)
            {
                RunSTask(sTaskCompletionSource, task).Coroutine();
            }

            await sTaskCompletionSource;

            async FVoid RunSTask(FTask tcs, FTask task)
            {
                await task;
                count--;
                
                if (count <= 0)
                {
                    tcs.SetResult();
                }
            }
        }
        
        public static async FTask Any(params FTask[] tasks)
        {
            if (tasks == null || tasks.Length <= 0)
            {
                return;
            }

            var tcs = FTask.Create();

            int count = 1;
            
            foreach (FTask task in tasks)
            {
                RunSTask(task).Coroutine();
            }
            
            await tcs;

            async FVoid RunSTask(FTask task)
            {
                await task;

                count--;

                if (count == 0)
                {
                    tcs.SetResult();
                }
            }
        }
    }

    public partial class FTask<T>
    {
        public static async FTask WhenAll(List<FTask<T>> tasks)
        {
            if (tasks.Count <= 0)
            {
                return;
            }
            
            var count = tasks.Count;
            var sTaskCompletionSource = FTask.Create();

            foreach (var task in tasks)
            {
                RunSTask(sTaskCompletionSource, task).Coroutine();
            }

            await sTaskCompletionSource;

            async FVoid RunSTask(FTask tcs, FTask<T> task)
            {
                await task;
                count--;
                if (count == 0)
                {
                    tcs.SetResult();
                }
            }
        }
        
        public static async FTask WhenAll(params FTask<T>[] tasks)
        {
            if (tasks == null || tasks.Length <= 0)
            {
                return;
            }
            
            var count = tasks.Length;
            var tcs = FTask.Create();

            foreach (var task in tasks)
            {
                RunSTask(task).Coroutine();
            }

            await tcs;

            async FVoid RunSTask(FTask<T> task)
            {
                await task;
                count--;
                if (count == 0)
                {
                    tcs.SetResult();
                }
            }
        }

        public static async FTask WaitAny(params FTask<T>[] tasks)
        {
            if (tasks == null || tasks.Length <= 0)
            {
                return;
            }

            var tcs = FTask.Create();

            int count = 1;
            
            foreach (FTask<T> task in tasks)
            {
                RunSTask(task).Coroutine();
            }
            
            await tcs;

            async FVoid RunSTask(FTask<T> task)
            {
                await task;

                count--;

                if (count == 0)
                {
                    tcs.SetResult();
                }
            }
        }
    }
}