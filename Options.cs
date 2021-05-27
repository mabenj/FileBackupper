#region

using CommandLine;

#endregion

namespace FileBackupper {
public class Options {
    [Option('d', "directory", Required = false, HelpText = "Directory where the backup file is stored")]
    public string TargetDirectory { get; set; }

    [Option('c', "csv", Required = true,
        HelpText =
            "Comma delimited file containing the paths to backup in format: <file/directory to backup>,<subfolder in backup file>")]
    public string PathsFile { get; set; }

    [Option('z', "zip", Required = false, HelpText = "Specifies whether the resulting backup file should be zipped")]
    public bool ShouldZip { get; set; }
}
}