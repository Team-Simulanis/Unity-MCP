#if UNITY_EDITOR
using System;
using System.Threading.Tasks;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public static class MainThreadInstaller
    {
        public static void Init()
        {
            MainThread.IsMainThread = () => MainThreadDispatcher.IsMainThread;
            MainThread.PushToMainThread = RunAsync;
        }
        public static void Run<T>(Task task) => RunAsync(task).Wait();
        public static Task RunAsync(Task task)
        {
            if (MainThreadDispatcher.IsMainThread)
            {
                // Execute directly if already on the main thread
                return task;
            }
            var tcs = new TaskCompletionSource<bool>();

            void Execute()
            {
                try
                {
                    task.Wait();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
                finally
                {
                    EditorApplication.update -= Execute;
                }
            }

            EditorApplication.update += Execute;
            return tcs.Task;
        }

        public static T Run<T>(Task<T> task) => RunAsync(task).Result;
        public static Task<T> RunAsync<T>(Task<T> task)
        {
            if (MainThreadDispatcher.IsMainThread)
            {
                // Execute directly if already on the main thread
                return task;
            }
            var tcs = new TaskCompletionSource<T>();

            void Execute()
            {
                try
                {
                    T result = task.Result;
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
                finally
                {
                    EditorApplication.update -= Execute;
                }
            }

            EditorApplication.update += Execute;
            return tcs.Task;
        }

        public static T Run<T>(Func<T> func) => RunAsync(func).Result;
        public static Task<T> RunAsync<T>(Func<T> func)
        {
            if (MainThreadDispatcher.IsMainThread)
            {
                // Execute directly if already on the main thread
                return Task.FromResult(func());
            }
            var tcs = new TaskCompletionSource<T>();

            void Execute()
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
                finally
                {
                    EditorApplication.update -= Execute;
                }
            }

            EditorApplication.update += Execute;
            return tcs.Task;
        }

        public static void Run(Action action) => RunAsync(action).Wait();
        public static Task RunAsync(Action action)
        {
            if (MainThreadDispatcher.IsMainThread)
            {
                // Execute directly if already on the main thread
                action();
                return Task.CompletedTask;
            }
            var tcs = new TaskCompletionSource<bool>();

            void Execute()
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
                finally
                {
                    EditorApplication.update -= Execute;
                }
            }

            EditorApplication.update += Execute;
            return tcs.Task;
        }
    }
}
#endif