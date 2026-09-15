using Digimezzo.Foundation.Core.Utils;
using Digimezzo.Foundation.Core.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Core.IO
{
    public sealed class FileOperations
    {
        public static List<FolderPathInfo> GetValidFolderPaths(long folderId, string directory, string[] validExtensions)
        {
            var folderPaths = new ConcurrentBag<FolderPathInfo>();

            try
            {
                // Use OS-native EnumerateFiles with AllDirectories — much faster than
                // manual recursion because it leverages OS file-system APIs directly
                // and avoids C# stack recursion overhead.
                IEnumerable<string> files;
                try
                {
                    files = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories);
                }
                catch (Exception ex)
                {
                    LogClient.Error("Error enumerating files in directory '{0}'. Exception: {1}", directory, ex.Message);
                    return new List<FolderPathInfo>();
                }

                // HashSet for O(1) extension lookup instead of Array.Contains O(n)
                var validExtSet = new HashSet<string>(validExtensions, StringComparer.OrdinalIgnoreCase);

                // Parallelize the per-file stat calls (DateModifiedTicks) since each
                // is an independent I/O call and these dominate scan time on large libraries
                Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, file =>
                {
                    try
                    {
                        if (validExtSet.Contains(Path.GetExtension(file)))
                        {
                            folderPaths.Add(new FolderPathInfo(folderId, file, FileUtils.DateModifiedTicks(file)));
                        }
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Error getting folder path info for file '{0}'. Exception: {1}", file, ex.Message);
                    }
                });
            }
            catch (Exception ex)
            {
                LogClient.Error("Unexpected error while getting folder paths for directory '{0}'. Exception: {1}", directory, ex.Message);
            }

            return folderPaths.ToList();
        }

        public static bool IsDirectoryContentAccessible(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return false;
            }

            try
            {
                var watcher = new FileSystemWatcher(directoryPath) { EnableRaisingEvents = true, IncludeSubdirectories = true };
                watcher.Dispose();
                watcher = null;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

