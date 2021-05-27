#region

using System.IO;
using System.Linq;

#endregion

namespace FileBackupper {
internal static class Helper {
    internal static long GetSize(this FileSystemInfo fileSystemInfo) {
        if (fileSystemInfo is DirectoryInfo directoryInfo){
            var files = directoryInfo.GetFiles();
            var size = files.Sum(fileInfo => fileInfo.Length);
            var directories = directoryInfo.GetDirectories();
            size += directories.Sum(directory => directory.GetSize());

            return size;
        } else{
            return ((FileInfo) fileSystemInfo).Length;
        }
    }

    internal static bool IsDirectory(string path) {
        try{
            var attribute = File.GetAttributes(path);
            return attribute.HasFlag(FileAttributes.Directory);
        } catch{
            return false;
        }
    }


    private static void DeleteDirectory(string targetDir) {
        File.SetAttributes(targetDir, FileAttributes.Normal);

        var files = Directory.GetFiles(targetDir);
        var dirs = Directory.GetDirectories(targetDir);

        foreach (var file in files){
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }

        foreach (var dir in dirs){
            DeleteDirectory(dir);
        }

        Directory.Delete(targetDir, false);
    }
}
}