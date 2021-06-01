#region

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

#endregion

namespace FileBackupper {
internal class Backupper {
    private readonly BackupItemList backupItems;
    private readonly bool shouldZip;
    private readonly DirectoryInfo targetDirectory;


    public Backupper(string pathsCsv, string targetPath, bool shouldZip = true) {
        this.targetDirectory =
            new DirectoryInfo(string.IsNullOrWhiteSpace(targetPath) ? Directory.GetCurrentDirectory() : targetPath);
        this.backupItems = BackupItemList.CreateFromCsv(pathsCsv, this.targetDirectory);
        this.shouldZip = shouldZip;
    }


    internal void Backup(string logFilePath) {
        Log.Info($"Starting backup to target folder '{this.targetDirectory}'");
        this.targetDirectory.Create();
        this.backupItems.PerformBackup(true);

        if (this.shouldZip){
            this.ZipResult(logFilePath);
        } else{
            Log.WriteEventsToFile(logFilePath);
        }
        Log.Info("Backup complete");
    }

    private void ZipResult(string logFilePath) {
        Log.Info($"Creating a zip archive from '{this.targetDirectory.FullName}'");
        Log.WriteEventsToFile(logFilePath);
        ZipFile.CreateFromDirectory(this.targetDirectory.FullName,
            Path.ChangeExtension(this.targetDirectory.FullName, ".zip"),
            CompressionLevel.Optimal, false, null);
        Log.Info($"Deleting working folder '{this.targetDirectory.FullName}'");
        this.targetDirectory.Delete(true);
    }
}
}