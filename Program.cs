#region

using System;
using System.IO;
using CommandLine;

#endregion

namespace FileBackupper {
    internal class Program {
        internal const string TimestampFormat = "yyyy-MM-dd-HH-mm-ss";

        private static int Main(string[] args) {
            return Parser.Default.ParseArguments<Options>(args).MapResult(Execute, _ => 1);
        }

        private static int Execute(Options options) {
            var targetFilePath = GetBackupTargetPath(string.IsNullOrWhiteSpace(options.AppendTo) ? options.TargetDirectory : options.AppendTo);
            var logFileName = $"Backup_{DateTime.Now.ToString(TimestampFormat)}.log";
            try {
                var backupItems = BackupItemList.CreateFromCsv(options.Config);
                Backupper.Backup(backupItems, targetFilePath, logFileName);
            } catch (Exception e) {
                var logFilePath = Path.Combine(Path.GetDirectoryName(targetFilePath) ?? string.Empty, logFileName);
                Log.WriteEventsToFile(logFilePath);
                Log.Error($"Backing up failed. See '{logFilePath}' for more details. ({e.Message})", e);
                return 1;
            } finally {
                //TODO cleanup
            }

            return 0;
        }

        private static string GetBackupTargetPath(string providedTarget = default) {
            var target = string.IsNullOrWhiteSpace(providedTarget) ? Directory.GetCurrentDirectory() : providedTarget;
            var targetFile = File.Exists(target) ? target : Path.Combine(target, $"Backup_{DateTime.Now.ToString(TimestampFormat)}.zip");
            return targetFile;
        }
    }
}