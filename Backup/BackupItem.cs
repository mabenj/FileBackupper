#region

using System.IO;

#endregion

namespace FileBackupper {
internal class BackupItem {
    internal BackupItem(FileSystemInfo source, DirectoryInfo target) {
        this.Source = source;
        this.Target = target;
    }

    internal FileSystemInfo Source { get; }

    internal DirectoryInfo Target { get; }
}
}