using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Dopamine.Core.Utils
{
    public static class MemoryUtils
    {
        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize", ExactSpelling = true, SetLastError = true)]
        private static extern bool SetProcessWorkingSetSize(IntPtr proc, IntPtr min, IntPtr max);

        /// <summary>
        /// Asynchronously performs a full Generation 2 garbage collection, compacts the Large Object Heap (LOH),
        /// and trims the process working set back to the Windows operating system.
        /// Ideal when the application is minimized or sent to the notification area (system tray).
        /// </summary>
        /// <param name="delayMilliseconds">Delay before trimming, allowing window animations to finish smoothly.</param>
        public static void TrimWorkingSet(int delayMilliseconds = 350)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (delayMilliseconds > 0)
                    {
                        await Task.Delay(delayMilliseconds);
                    }

                    // Request Large Object Heap compaction on the next full GC
                    GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;

                    // Collect unreachable objects and finalizers
                    GC.Collect(2, GCCollectionMode.Forced, true);
                    GC.WaitForPendingFinalizers();
                    GC.Collect(2, GCCollectionMode.Forced, true);

                    // Tell Windows to trim unneeded physical memory pages
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        using (Process currentProcess = Process.GetCurrentProcess())
                        {
                            SetProcessWorkingSetSize(currentProcess.Handle, (IntPtr)(-1), (IntPtr)(-1));
                        }
                    }
                }
                catch
                {
                    // Fail-safe: memory trimming should never crash or throw
                }
            });
        }
    }
}
