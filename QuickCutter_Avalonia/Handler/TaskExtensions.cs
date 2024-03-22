using System;
using System.Threading.Tasks;

namespace QuickCutter_Avalonia.Handler
{
    static class TaskExtensions
    {
        /// <summary>
        /// Safe fire and forget
        /// </summary>
        /// <param name="task"></param>
        /// <param name="OnCompleted"></param>
        /// <param name="OnError"></param>
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
