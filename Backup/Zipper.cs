#region

using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;

#endregion

namespace FileBackupper {
    internal class Zipper {
        private const string SavingVerb = "Saving";
        private const string AddingVerb = "Adding";
        private readonly string logFilePath;
        private readonly ZipFile zip;
        private long currentEntrySavedBytes;
        private int entriesSaved;
        private int entriesToSave;
        private long numberOfBytesSaved;
        private long numberOfBytesToSave;
        private int numberOfEntriesAdded;
        private int numberOfEntriesToAdd;
        private ProgressBar progressBar;

        public Zipper(ZipFile zip, string logFileName) {
            //this.logFilePath = Path.Combine(Path.GetTempPath(), logFileName);
            this.logFilePath = Log.LogFilePath;

            this.zip = zip;
            this.zip.TempFileFolder = Path.GetTempPath();
            this.zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
            this.zip.AlternateEncoding = Encoding.UTF8;
            this.zip.AlternateEncodingUsage = ZipOption.Always;

            this.zip.AddProgress += HandleZipAddEvent;
            this.zip.SaveProgress += HandleZipSaveEvent;

            this.numberOfEntriesAdded = 0;
            this.numberOfEntriesToAdd = -1;
            this.numberOfBytesSaved = 0;

            this.entriesSaved = 0;
            this.entriesToSave = -1;

            this.currentEntrySavedBytes = 0;
        }

        private void HandleZipSaveEvent(object sender, SaveProgressEventArgs e) {
            switch (e.EventType) {
                case ZipProgressEventType.Saving_EntryBytesRead:
                    this.currentEntrySavedBytes = e.BytesTransferred;
                    this.UpdateSaveProgress(e.CurrentEntry.FileName, false);
                    break;
                case ZipProgressEventType.Saving_BeforeWriteEntry:
                    this.entriesSaved = e.EntriesSaved + 1;
                    this.entriesToSave = e.EntriesTotal;
                    this.UpdateSaveProgress(e.CurrentEntry.FileName);
                    if (e.EntriesSaved + 1 == e.EntriesTotal) {
                        //Log.Info($"Dumping logs to '{this.logFilePath}'", false);
                        //Log.WriteEventsToFile(this.logFilePath);
                    }

                    break;
                case ZipProgressEventType.Saving_AfterWriteEntry:
                    this.numberOfBytesSaved += this.currentEntrySavedBytes;
                    this.currentEntrySavedBytes = 0;
                    if (e.EntriesSaved == e.EntriesTotal && !this.progressBar.IsDisposed) {
                        this.progressBar.UpdateProgress(1, "Saving complete");
                    }

                    break;
            }
        }

        private void UpdateSaveProgress(string currentEntry, bool shouldLog = true) {
            var progress = (this.numberOfBytesSaved + this.currentEntrySavedBytes) / (double) this.numberOfBytesToSave;
            this.UpdateProgress(progress, this.entriesSaved, this.entriesToSave, SavingVerb, currentEntry, shouldLog);
        }

        private void UpdateAddProgress(string currentEntry, bool shouldLog = true) {
            if (this.numberOfEntriesAdded == this.numberOfEntriesToAdd) {
                this.progressBar.UpdateProgress(1, $"{AddingVerb} complete");
                return;
            }

            var progress = ++this.numberOfEntriesAdded / (double) (this.numberOfEntriesToAdd);
            this.UpdateProgress(progress, this.numberOfEntriesAdded, this.numberOfEntriesToAdd, AddingVerb, currentEntry, shouldLog);
        }

        private void UpdateProgress(double progress, int filesCompleted, int filesCount, string verb, string entryName, bool shouldLog) {
            if (this.progressBar.IsDisposed) {
                return;
            }

            var fileCountProgress = $"Files: {filesCompleted}/{filesCount}";
            var message = $"{fileCountProgress} {verb} '{entryName}'";
            if (shouldLog) {
                Log.Info(message, false);
            }

            this.progressBar.UpdateProgress(progress, message);
        }

        internal void AddItems(BackupItemList backupItems, bool addLogFile = true) {
            Log.Info("Adding files to zip file");
            this.numberOfEntriesToAdd = backupItems.Count();
            this.numberOfBytesToSave = backupItems.SizeOfSourcesInBytes;
            this.progressBar = new ProgressBar();
            using (progressBar) {
                using (this.zip) {
                    foreach (var backupItem in backupItems) {
                        this.zip.AddBackupItem(backupItem);
                    }

                    if (addLogFile) {
                        this.zip.AddFile(Log.LogFilePath, string.Empty);
                    }
                }
            }
        }

        private void HandleZipAddEvent(object sender, AddProgressEventArgs e) {
            switch (e.EventType) {
                case ZipProgressEventType.Adding_AfterAddEntry:
                    this.UpdateAddProgress(e.CurrentEntry.FileName);
                    break;
            }
        }

        internal void Save() {
            Log.Info("Compressing backup zip");
            this.progressBar = new ProgressBar();
            using (progressBar) {
                using (this.zip) {
                    this.zip.Save();
                }
            }
        }
    }
}