#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Collections.Generic;
using System.Threading;

namespace com.IvanMurzak.Unity.MCP.Common.Utils
{
    public static class MainThreadDispatcher
    {
        private static readonly Queue<Action> _executionQueue = new Queue<Action>();
        private static readonly object _lockObject = new object();

        public static void Enqueue(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            lock (_lockObject)
            {
                _executionQueue.Enqueue(action);
            }

            // For Unity integration, this would use the main thread context
            // In this simplified version, we just execute the action directly
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MainThreadDispatcher: {ex.Message}");
            }
        }

        public static void ExecuteQueuedActions()
        {
            lock (_lockObject)
            {
                while (_executionQueue.Count > 0)
                {
                    try
                    {
                        _executionQueue.Dequeue()?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error executing queued action: {ex.Message}");
                    }
                }
            }
        }
    }
} 