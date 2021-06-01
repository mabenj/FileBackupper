#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using SearchOption = System.IO.SearchOption;

#endregion

namespace FileBackupper {
internal class BackupItemList {
    private readonly List<BackupItem> items;
    private readonly List<Tuple<FileInfo, Exception>> errorFiles;

    private ProgressBar progressBar;
    private long sizeOfAllSources;
    private long sizeOfBackedUpSources;

    private BackupItemList() {
        this.items = new List<BackupItem>();
        this.errorFiles = new List<Tuple<FileInfo, Exception>>();
        this.sizeOfAllSources = 0;
        this.sizeOfBackedUpSources = 0;
    }

    internal void Add(BackupItem backupItem) {
        this.items.Add(backupItem);
        this.sizeOfAllSources += backupItem.Source.GetSize();
    }

    internal static BackupItemList CreateFromCsv(string pathsCsv, DirectoryInfo targetDirectory) {
        if (!File.Exists(pathsCsv)){
            throw new FileNotFoundException($"Could not find '{pathsCsv}'", pathsCsv);
        }

        var backupItems = new BackupItemList();
        using var csvParser = new TextFieldParser(pathsCsv) {
            Delimiters = new[] { "," },
            CommentTokens = new[] { "#" },
            HasFieldsEnclosedInQuotes = true
        };
        Log.Info($"Parsing paths from '{pathsCsv}'");
        while (!csvParser.EndOfData){
            var fields = csvParser.ReadFields();
            var sourcePath = fields.First().TrimStart('/', '\\');
            var targetSubFolder = fields.Length > 1 ? fields[1].TrimStart('/', '\\') : string.Empty;

            var sourceIsFolder = Helper.IsDirectory(sourcePath);

            if (!File.Exists(sourcePath) && !sourceIsFolder){
                Log.Warning($"Could not resolve '{sourcePath}'");
                continue;
            }

            if (string.IsNullOrWhiteSpace(targetSubFolder) && sourceIsFolder){
                targetSubFolder = Path.GetFileName(sourcePath);
            }

            FileSystemInfo itemSource;
            if (sourceIsFolder){
                itemSource = new DirectoryInfo(sourcePath);
            } else{
                itemSource = new FileInfo(sourcePath);
            }

            var itemTargetDirectory = targetDirectory;
            if (!string.IsNullOrWhiteSpace(targetSubFolder)){
                itemTargetDirectory = targetDirectory.CreateSubdirectory(targetSubFolder);
            }

            backupItems.Add(new BackupItem(itemSource, itemTargetDirectory));
        }

        return backupItems;
    }

    internal void PerformBackup(bool ignoreErrors) {
        this.progressBar = new ProgressBar();
        using (progressBar){
            foreach (var item in this.items){
                if (item.Source is DirectoryInfo){
                    this.BackupDirectory(item.Source.FullName, item.Target.FullName, ignoreErrors);
                } else{
                    var destinationFile = Path.Combine(item.Target.FullName, Path.GetFileName(item.Source.FullName));
                    this.BackupFile((FileInfo) item.Source, new FileInfo(destinationFile), ignoreErrors);
                }
            }
        }

        foreach (var (file, exception) in this.errorFiles){
            Log.Error($"Could not backup file '{file.FullName}' ({exception.Message})");
        }
    }

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
    private void BackupFile(FileInfo sourceFile, FileInfo targetFile, bool ignoreErrors = false) {
        try{
            var progress = this.sizeOfBackedUpSources / (double) this.sizeOfAllSources;
            Log.Info($"Backing up file '{sourceFile.FullName}'", false);
            this.progressBar.UpdateProgress(progress, $"Backing up file '{sourceFile.Name}'");
            File.Copy(sourceFile.FullName, targetFile.FullName, true);
        } catch (Exception e){
            this.errorFiles.Add(Tuple.Create(sourceFile, e));
            Log.Error($"Could not backup '{sourceFile.Name}'", e, false);
            if (!ignoreErrors){
                throw;
            }
        }

        this.sizeOfBackedUpSources += sourceFile.GetSize();
    }

    private void BackupDirectory(string sourcePath, string targetPath, bool ignoreErrors = false) {
        foreach (var dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories)){
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }

        foreach (var newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories)){
            this.BackupFile(new FileInfo(newPath), new FileInfo(newPath.Replace(sourcePath, targetPath)), ignoreErrors);
        }
    }
}
}