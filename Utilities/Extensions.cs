#region

using System.IO;
using System.Linq;
using Ionic.Zip;

#endregion

namespace FileBackupper {
    internal static class Extensions {
        internal static long GetSize(this FileSystemInfo fileSystemInfo) {
            if (fileSystemInfo is DirectoryInfo directoryInfo) {
                var files = directoryInfo.GetFiles();
                var size = files.Sum(fileInfo => fileInfo.Length);
                var directories = directoryInfo.GetDirectories();
                size += directories.Sum(directory => directory.GetSize());

                return size;
            }

            return ((FileInfo) fileSystemInfo).Length;
        }


        internal static void AddBackupItem(this ZipFile zip, BackupItem backupItem, bool overwriteIfModified = true) {
            if (overwriteIfModified && zip.ContainsEntry(Path.Combine(backupItem.Target, backupItem.Source.Name))) {
                var oldEntry = zip[Path.Combine(backupItem.Target, backupItem.Source.Name)];
                if (backupItem.Source.LastWriteTimeUtc <= oldEntry.ModifiedTime) {
                    // Skip untouched file
                    return;
                }

                Log.Warning($"Found newer modified file '{backupItem.Source.Name}'. Removing older file '{oldEntry.FileName}'.");
                zip.RemoveEntry(oldEntry);
            }

            zip.AddFile(backupItem.Source.FullName, backupItem.Target);
        }

        internal static bool IsDirectoryPath(this string path) {
            try {
                var attribute = File.GetAttributes(path);
                return attribute.HasFlag(FileAttributes.Directory);
            } catch {
                return false;
            }
        }


        private static void DeleteDirectory(this DirectoryInfo targetDir) {
            targetDir.Attributes = FileAttributes.Normal;

            var files = targetDir.GetFiles();
            var dirs = targetDir.GetDirectories();

            foreach (var file in files) {
                file.Attributes = FileAttributes.Normal;
                file.Delete();
            }

            foreach (var dir in dirs) {
                DeleteDirectory(dir);
            }

            targetDir.Delete(false);
        }
    }
}