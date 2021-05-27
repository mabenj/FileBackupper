#region

using System;
using System.IO;
using CommandLine;
using FileBackupper.Backup;

#endregion

namespace FileBackupper {
internal class Program {
    private static readonly string Timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

    private static int Main(string[] args) {
        return Parser.Default.ParseArguments<Options>(args).MapResult(Execute, _ => 1);
    }

    private static int Execute(Options options) {
        var targetPath = GetBackupTargetPath(options.TargetDirectory);
        var logFilePath = Path.Combine(targetPath, $"Backup_{Timestamp}.log");
        try {
            var backupper = new Backupper(options.PathsFile, targetPath, options.ShouldZip);
            backupper.Backup(logFilePath);
        } catch (Exception e){
            Log.Error($"Backing up failed. See '{logFilePath}' for more details.", e);
            Log.WriteEventsToFile(logFilePath);

            return 1;
        }
        return 0;
    }

    private static string GetBackupTargetPath(string providedTarget = null) {
        var targetDirectory = string.IsNullOrWhiteSpace(providedTarget) ? Directory.GetCurrentDirectory() : providedTarget;
        var targetPath = Path.Combine(targetDirectory, $"Backup_{Timestamp}");
        return targetPath;
    }
}
}