using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks;

namespace PRTGInsight.Helpers
{
    public static class DispatcherQueueExtensions
    {
        public static Task EnqueueAsync(this DispatcherQueue dispatcherQueue, Action action)
        {
            var taskCompletionSource = new TaskCompletionSource();

            if (!dispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    action();
                    taskCompletionSource.SetResult();
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            }))
            {
                taskCompletionSource.SetException(
                    new InvalidOperationException("Failed to enqueue the operation to the dispatcher queue."));
            }

            return taskCompletionSource.Task;
        }

        public static Task<T> EnqueueAsync<T>(this DispatcherQueue dispatcherQueue, Func<T> function)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();

            if (!dispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    var result = function();
                    taskCompletionSource.SetResult(result);
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            }))
            {
                taskCompletionSource.SetException(
                    new InvalidOperationException("Failed to enqueue the operation to the dispatcher queue."));
            }

            return taskCompletionSource.Task;
        }
    }
}
