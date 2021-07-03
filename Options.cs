#region

using CommandLine;

#endregion

namespace FileBackupper {
    public class Options {
        [Option('d', "directory", Required = false, SetName = "Target", HelpText = "Directory where to save the backup file")]
        public string TargetDirectory {
            get;
            set;
        }

        [Option('c', "csv", Required = true,
            HelpText =
                "Comma delimited file containing the paths to backup in format: <file/directory to backup>,<subfolder in backup file>")]
        public string Config {
            get;
            set;
        }

        [Option('a', "append", Required = false, SetName = "Target", HelpText = "Specifies a previous backup file to append to")]
        public string AppendTo {
            get;
            set;
        }
    }
}