using System.Diagnostics;
using System.IO;
using System;
using System.Linq;
using System.Text.Json;

class mkvmerge {
    private static string _mkvMergePath;
    private static string _pwshPath;
    public static void Initialize(string episodeFolder) {
        try {
            LoadConfig("config.json");
            string mkvInputPath = Directory.GetFiles(episodeFolder, "*.mkv").First();
            string mkvInputName = Path.GetFileNameWithoutExtension(mkvInputPath);
            string mkvOutputPath = mkvInputPath.Remove(mkvInputPath.Length - 4, 4); mkvOutputPath += "_new.mkv";
            string[] subtitlesIdx = Directory.GetFiles(episodeFolder, "*.idx");
            string[] subtitlesSub = Directory.GetFiles(episodeFolder, "*.sub");

            string subtitleEngFull = mkvInputName + "*eng";
            string subtitleEngForced = mkvInputName + "*eng*forced";
            string subtitleGerFull = mkvInputName;
            string subtitleGerForced = mkvInputName + "*forced";

            string mkvmergeArgs = "-o '" + mkvOutputPath + "' -q --no-subtitles '" + mkvInputPath + "' ";
            if(Folder.SubfilesExist(episodeFolder,subtitleEngFull)) {
                mkvmergeArgs += "--language 0:eng " +
                                "--track-name 0:Full '"
                                + Directory.GetFiles(episodeFolder,subtitleEngFull + ".idx")[0] + "' '"
                                + Directory.GetFiles(episodeFolder,subtitleEngFull + ".sub")[0] + "' ";
            }
            if(Folder.SubfilesExist(episodeFolder,subtitleEngForced)) {
                mkvmergeArgs += "--language 0:eng " +
                                "--track-name 0:Forced '"
                                + Directory.GetFiles(episodeFolder,subtitleEngForced + ".idx")[0] + "' '"
                                + Directory.GetFiles(episodeFolder,subtitleEngForced + ".sub")[0] + "' ";
            }
            if(Folder.SubfilesExist(episodeFolder,subtitleGerFull)) {
                mkvmergeArgs += "--language 0:deu " +
                                "--track-name 0:Full '"
                                + Directory.GetFiles(episodeFolder,subtitleGerFull + ".idx")[0] + "' '"
                                + Directory.GetFiles(episodeFolder,subtitleGerFull + ".sub")[0] + "' ";
            }
            if(Folder.SubfilesExist(episodeFolder,subtitleGerForced)) {
                mkvmergeArgs += "--language 0:deu " +
                                "--track-name 0:Forced '"
                                + Directory.GetFiles(episodeFolder,subtitleGerForced + ".idx")[0] + "' '"
                                + Directory.GetFiles(episodeFolder,subtitleGerForced + ".sub")[0] + "' ";
            }

            if(File.Exists(mkvOutputPath)) { File.Delete(mkvOutputPath); }
            Run(mkvmergeArgs);
            Folder.RenameFile(mkvOutputPath,mkvInputPath);
            Folder.DeleteSubtitleFiles(subtitlesIdx);
            Folder.DeleteSubtitleFiles(subtitlesSub);
        }
        catch(Exception e) {
            Console.WriteLine(e.ToString());
        }
    }
    public static void Run(string mkvmergeArgs) {
        ProcessStartInfo command = new(_pwshPath) {
            Arguments = $"-command \"& {_mkvMergePath} {mkvmergeArgs}\""
        };
        // Console.WriteLine("ProcessStartInfo: " + command.Arguments);
        Process pwsh = Process.Start(command);
        pwsh.WaitForExit();
    }
    private static void LoadConfig(string configFilePath) {
        if(!File.Exists(configFilePath)) {
            Console.WriteLine("Error: Config file config.json not found.");
            Environment.Exit(1); // Exit if config file is missing
        }

        var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configFilePath));
        _mkvMergePath = config.MkvMergePath;
        _pwshPath = config.PwshPath;

        // Console.WriteLine("_mkvMergePath: " + _mkvMergePath);
        // Console.WriteLine("_pwshPath: " + _pwshPath);
    }
}