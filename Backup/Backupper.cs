#region

using System.IO;
using Ionic.Zip;

#endregion

namespace FileBackupper {
    internal class Backupper {
        internal static void Backup(BackupItemList backupItems, string targetFilePath, string logFileName) {
            var zip = File.Exists(targetFilePath) ? ZipFile.Read(targetFilePath) : new ZipFile(targetFilePath);
            var zipper = new Zipper(zip, logFileName);
            zipper.AddItems(backupItems);
            zipper.Save();
            Log.Info($"Backup complete. Backup zip location is '{targetFilePath}'");

            //foreach (var (file, exception) in this.backupItems.ErrorFiles) {
            //    Log.Error($"Could not backup file '{file.FullName}' ({exception.Message})");
            //}
        }
    }
}