#region

using System.IO;

#endregion

namespace FileBackupper {
internal class BackupItem {
    internal BackupItem(FileInfo source, string target) {
        this.Source = source;
        this.Target = target;
    }

    internal FileInfo Source { get; }

    internal string Target { get; }
}
}