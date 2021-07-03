#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.VisualBasic.FileIO;
using SearchOption = System.IO.SearchOption;

#endregion

namespace FileBackupper {
    internal class BackupItemList: IEnumerable<BackupItem> {
        private readonly List<BackupItem> items;

        private BackupItemList() {
            this.items = new List<BackupItem>();
            this.ErrorFiles = new List<Tuple<FileInfo, Exception>>();
            this.SizeOfSourcesInBytes = 0;
        }

        internal List<Tuple<FileInfo, Exception>> ErrorFiles {
            get;
        }

        public IEnumerator<BackupItem> GetEnumerator() {
            return this.items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        internal void Add(FileSystemInfo source, string targetSubFolder = "") {
            switch (source) {
                case DirectoryInfo directoryInfo: {
                    var topFileSystemInfos = directoryInfo.GetFileSystemInfos("*", SearchOption.TopDirectoryOnly);
                    foreach (var fileSystemInfo in topFileSystemInfos) {
                        this.Add(fileSystemInfo, Path.Combine(targetSubFolder, directoryInfo.Name));
                    }

                    break;
                }
                case FileInfo fileInfo:
                    this.items.Add(new BackupItem(fileInfo, targetSubFolder));
                    this.SizeOfSourcesInBytes += source.GetSize();
                    break;
            }
        }

        internal long SizeOfSourcesInBytes {
            get;
            private set;
        }

            internal static BackupItemList CreateFromCsv(string configCsv) {
            if (!File.Exists(configCsv)) {
                throw new FileNotFoundException($"Could not find '{configCsv}'", configCsv);
            }

            var backupItems = new BackupItemList();
            using var csvParser = new TextFieldParser(configCsv) {
                Delimiters = new[] { "," },
                CommentTokens = new[] { "#" },
                HasFieldsEnclosedInQuotes = true
            };
            Log.Info($"Parsing paths from '{configCsv}'");
            while (!csvParser.EndOfData) {
                var fields = csvParser.ReadFields();
                var sourcePath = fields.First().TrimStart('/', '\\');
                var targetSubFolder = fields.Length > 1 ? fields[1].TrimStart('/', '\\') : string.Empty;

                var sourceIsFolder = sourcePath.IsDirectoryPath();

                if (!sourceIsFolder && !File.Exists(sourcePath)) {
                    Log.Warning($"Could not resolve path '{sourcePath}'");
                    continue;
                }

                FileSystemInfo itemSource;
                if (sourceIsFolder) {
                    itemSource = new DirectoryInfo(sourcePath);
                } else {
                    itemSource = new FileInfo(sourcePath);
                }

                backupItems.Add(itemSource, targetSubFolder);
            }

            return backupItems;
        }
    }
}