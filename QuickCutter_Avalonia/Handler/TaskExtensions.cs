using System;
using System.Threading.Tasks;

namespace QuickCutter_Avalonia.Handler
{
    static class TaskExtensions
    {
        public static async void Await(this Task task, Action? OnCompleted = null, Action<Exception>? OnError = null)
        {
            try
            {
                await task;
                OnCompleted?.Invoke();
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
            }
        }
    }
}
