using System;
using System.IO;
using System.Threading;

namespace Dopamine.Core.Helpers
{
    /// <summary>
    /// A folder watcher that is not too nervous when notifying of changes,
    /// acting as a thread-safe debouncer.
    /// </summary>
    public class GentleFolderWatcher : IDisposable
    {
        private FileSystemWatcher watcher;
        private Timer debounceTimer;
        private readonly int interval;
        private bool disposedValue = false;

        public event EventHandler FolderChanged = delegate { };

        public GentleFolderWatcher(string folderPath, bool includeSubdirectories, int intervalMilliSeconds = 500)
        {
            this.interval = intervalMilliSeconds;
            
            // Initialize the timer in an infinite wait state (disabled)
            this.debounceTimer = new Timer(OnTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);

            this.watcher = new FileSystemWatcher(folderPath)
            {
                IncludeSubdirectories = includeSubdirectories
            };

            this.watcher.Changed += OnChanged;
            this.watcher.Created += OnChanged;
            this.watcher.Deleted += OnChanged;
            this.watcher.Renamed += OnRenamed;
        }

        private void OnRenamed(object sender, RenamedEventArgs e) => Trigger();
        private void OnChanged(object sender, FileSystemEventArgs e) => Trigger();

        private void Trigger()
        {
            if (!disposedValue)
            {
                // Reset the timer to fire after the interval. If triggered again before interval, it resets.
                this.debounceTimer?.Change(this.interval, Timeout.Infinite);
            }
        }

        private void OnTimerElapsed(object state)
        {
            if (!disposedValue)
            {
                // Fire the event on the background ThreadPool thread, consumers handle their own UI dispatching.
                this.FolderChanged(this, EventArgs.Empty);
            }
        }

        public void Suspend()
        {
            if (this.watcher != null) this.watcher.EnableRaisingEvents = false;
            if (this.debounceTimer != null) this.debounceTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Resume()
        {
            if (this.watcher != null && !this.disposedValue)
            {
                this.watcher.EnableRaisingEvents = true;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (this.watcher != null)
                    {
                        this.watcher.EnableRaisingEvents = false;
                        this.watcher.Dispose();
                        this.watcher = null;
                    }

                    if (this.debounceTimer != null)
                    {
                        this.debounceTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        this.debounceTimer.Dispose();
                        this.debounceTimer = null;
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}