#if !UNITY_EDITOR
using System;
using System.Threading.Tasks;
using com.IvanMurzak.ReflectorNet.Utils;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public static class MainThreadInstaller
    {
        public static void Init()
        {
            MainThread.IsMainThread = () => MainThreadDispatcher.IsMainThread;
            MainThread.PushToMainThread = RunAsync;
        }
        public static T Run<T>(Func<T> func) => RunAsync(func).Result;
        public static Task<T> RunAsync<T>(Func<T> func)
        {
            var tcs = new TaskCompletionSource<T>();

            MainThreadDispatcher.Enqueue(() =>
            {
                try
                {
                    T result = func();
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }

        public static void Run(Action action) => RunAsync(action).Wait();
        public static Task RunAsync(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();

            MainThreadDispatcher.Enqueue(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }
    }
}
#endif