using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QuickCutter_Avalonia.Handler
{
    internal class DebugHandler
    {
        static private Stopwatch stopwatch = new Stopwatch();
        static public void StopwatchStart()
        {
            stopwatch.Restart();
        }

        static public void StopwatchStopAndPrintTime([CallerMemberName] string? name = null ) 
        {
            stopwatch.Stop();
            Debug.WriteLine($"{name} - Elapsed Time: {stopwatch.ElapsedMilliseconds}ms");
        }

        static public void PrintCurrentThreadID(string? message = null, [CallerMemberName] string? name = null)
        {
            if (!string.IsNullOrEmpty(message))
            {
                name += $" @ {message}";
            }
            Debug.WriteLine($"{name} - Thread ID: {Environment.CurrentManagedThreadId}");
        }
    }
}
