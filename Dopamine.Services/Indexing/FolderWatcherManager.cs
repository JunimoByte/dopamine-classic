using Digimezzo.Foundation.Core.Logging;
using Dopamine.Core.Helpers;
using Dopamine.Data.Entities;
using Dopamine.Data.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Dopamine.Services.Indexing
{
    internal class FolderWatcherManager
    {
        private IFolderRepository folderRepository;
        private IList<GentleFolderWatcher> watchers = new List<GentleFolderWatcher>();
        private readonly object watchersLock = new object();

        public event EventHandler FoldersChanged = delegate { };

        public FolderWatcherManager(IFolderRepository folderRepository)
        {
            this.folderRepository = folderRepository;
        }

        private void Watcher_FolderChanged(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.FoldersChanged(this, new EventArgs());
            });
        }

        public async Task StartWatchingAsync()
        {
            this.StopWatching();

            List<Folder> folders = await this.folderRepository.GetFoldersAsync();

            lock (this.watchersLock)
            {
                foreach (Folder fol in folders)
                {
                    if (Directory.Exists(fol.Path))
                    {
                        try
                        {
                            var watcher = new GentleFolderWatcher(fol.Path, true, 500);
                            watcher.FolderChanged += Watcher_FolderChanged;
                            this.watchers.Add(watcher);
                            watcher.Resume();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error($"Could not watch folder '{fol.Path}', even though it exists. Please check folder permissions. Exception: {ex.Message}");
                        }
                    }
                }
            }
        }

        public void StopWatching()
        {
            lock (this.watchersLock)
            {
                if (this.watchers.Count == 0)
                {
                    return;
                }

                for (int i = this.watchers.Count - 1; i >= 0; i--)
                {
                    this.watchers[i].FolderChanged -= Watcher_FolderChanged;
                    this.watchers[i].Dispose();
                    this.watchers.RemoveAt(i);
                }
            }
        }

        public Task StopWatchingAsync()
        {
            this.StopWatching();
            return Task.CompletedTask;
        }
    }
}