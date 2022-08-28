using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class SubMerger {
    private static int Main(string[] args) {
        // MOVIE/SE E:\DOWNLOAD\TestMovie.2018.German.1080p.BluRay.x264-RAiNBOW\*.mkv                                                      (maybe folder Subs and Sample!) (subfolders >= 2)
        // COMPLETE E:\DOWNLOAD\TestSeries.S01.German.1080p.BluRay.x264-RAiNBOW\Test.S01E01.German.1080p.BluRay.x264-RAiNBOW\*.mkv
        //                                  args[0] = folder                   |              subfolder   

        // arguments
        string inputPath = args[0];
        string inputName = args.Length > 1 ? args[1] : DateTime.Now.ToString("dd-MM_HH-mm");

        // variables
        string[] dirArray = Directory.GetDirectories(inputPath); // get all folders in the path
        int dirArrayCount = dirArray.Length;                    // amount of folders
        Array.Sort(dirArray);                                  // sorts from A-Z to have a correct episode order

        // logger
        using StreamWriter log = new StreamWriter(@"E:\TOOLS\SABnzbd\logs\" + inputName + ".log") {
            AutoFlush = true // writes any text instantly to the file, with false it only writes when returning
        };

        // determine the type of media (single mkv or multiple episodes) by matching a regex for S0x.
        bool isSeasonFolder = Regex.Match(inputPath, @"S[0-9]{2}[^E]").Success ? true : false;

        // Header
        Output.Write(log,
            "Start Time: " + DateTime.Now.ToString("ddd dd.MM.yyyy HH:mm:ss") +
            "\n|- Folder: " + inputPath + "\\(" + dirArrayCount + ")" +
            "\n|- Type: ");
        try {
            if(isSeasonFolder) {
                Output.WriteSeasonInfo(log, inputPath);

                List<string> queue = GetQueue(inputPath);
                int progress = 0;

                foreach(string item in queue) {
                    progress++;
                    Output.Write(log, DateTime.Now.ToString("HH:mm:ss") + " | (M) mkvmerge: #" + progress.ToString("00") + " of " + queue.Count + " \t(" + Regex.Match(item, @"E[0-9]{2}").Groups[0].Value + ")\t");
                    Folder.MoveSubsToRoot(item);
                    mkvmerge.Start(item);
                    log.Write("\tcompleted\n");
                    Console.WriteLine("\ncompleted");
                }

                if(progress == 0 && queue.Count == 0) {
                    Output.WriteLine(log, "Exiting...");
                    return 0;
                } else if(progress == queue.Count) {
                    Output.WriteLine(log, DateTime.Now.ToString("HH:mm:ss") + " | (M) mkvmerge done");
                    return 0; // 0 -> Success
                } else return 1;

            } else if(!isSeasonFolder) {
                Output.WriteInfo(log, inputPath);

                if(Directory.Exists(inputPath + @"\Subs") || Directory.GetFiles(inputPath, "*.sub").Any()) {
                    Output.WriteLine(log, DateTime.Now.ToString("HH:mm:ss") + " | (S) mkvmerge in progress");
                    if(Directory.Exists(inputPath + @"\Subs")) Folder.MoveSubsToRoot(inputPath);
                    mkvmerge.Start(inputPath);
                    Output.WriteLine(log, DateTime.Now.ToString("HH:mm:ss") + " | (S) mkvmerge done");
                    return 0;
                } else {
                    Output.WriteLine(log, "Exiting...");
                    return 0;
                } 
            }
        } catch(Exception e) {
            Output.WriteLine(log, e.ToString());
            return 1;
        };
        return 1;
    }
    public static List<string> GetQueue(string inputPath) {
        string[] dirArray = Directory.GetDirectories(inputPath);
        List<string> queue = new List<string>();
        foreach(string dir in dirArray) {
            if(Directory.Exists(dir + @"\Subs") && Directory.GetFiles(dir + @"\Subs\", "*eng.*").Any())
                queue.Add(dir);
        };
        return queue;

    }
}
class Output {
    public static void Write(StreamWriter log, string output) {
        Console.Write(output);
        log.Write(output);
    }
    public static void WriteLine(StreamWriter log, string output) {
        Console.WriteLine(output);
        log.WriteLine(output);
    }
    public static void WriteSeasonInfo(StreamWriter log, string inputPath) {
        string[] dirArray = Directory.GetDirectories(inputPath);
        int amountSubTotal = Folder.CountSubtitlesTotal(inputPath);
        int amountSubFullEng = Folder.CountFullEngSubfiles(inputPath);
        int amountSubMissingEng = amountSubTotal - amountSubFullEng;
        int amountSubMissing = dirArray.Length - amountSubTotal;

        // Header - Type
        Output.WriteLine(log, "Season");

        // Header - Episodes Amount
        int episodeCount = 0;
        foreach(string dir in Directory.GetDirectories(inputPath))
            if(Regex.Match(dir, @"S[0-9]{2}E[0-9]{2}").Success)
                episodeCount++;
        Write(log, "|- Episodes: " + episodeCount + "\n");

        // Header - Subtitles
        WriteLine(log, "|- Subtitles");
        // Header - Subtitles Total Found
        Write(log, "|  - Full     " + amountSubFullEng.ToString("00") + "  ( ");
        foreach(string dir in dirArray)
            if(Directory.Exists(dir + @"\Subs") && Directory.GetFiles(dir + @"\Subs\", "*eng.*").Any())
                Write(log, Regex.Match(dir, @"E[0-9]{2}").Groups[0].Value + " ");
        Write(log, ")\n");

        // Header - Subtitles Missing Eng Found
        Write(log, "|  - !Full    " + amountSubMissingEng.ToString("00") + "  ( ");
        foreach(string dir in dirArray)
            if(Directory.Exists(dir + @"\Subs") && !Directory.GetFiles(dir + @"\Subs\", "*eng.*").Any())
                Write(log, Regex.Match(dir, @"E[0-9]{2}").Groups[0].Value + " ");
        Write(log, ")\n");

        // Header - Subtitles Missing Eng Found
        Write(log, "|  - Missing  " + amountSubMissing.ToString("00") + "  ( ");
        foreach(string dir in dirArray)
            if(!Directory.Exists(dir + @"\Subs"))
                Write(log, Regex.Match(dir, @"E[0-9]{2}").Groups[0].Value + " ");
        WriteLine(log, ")\n");
    }
    public static void WriteInfo(StreamWriter log, string inputPath) {
        WriteLine(log, "Single File");
        // Header - Subtitles
        Write(log, "|- Subtitles");

        if(Directory.Exists(inputPath + @"\Subs")) {
            Write(log, "\n");
            if(Directory.GetFiles(inputPath + @"\Subs", "*.sub").Length > 0)
                foreach(string file in Directory.GetFiles(inputPath + @"\Subs", "*.sub"))
                    WriteLine(log, "|  - " + file.Remove(0, inputPath.Length + 6));
        } else Write(log, " missing");
        WriteLine(log, "");
    }
}
class Folder {
    public static void MoveSubsToRoot(string inputPath) {
        string subtitlesPath = inputPath + @"\Subs";
        if(Directory.Exists(subtitlesPath)) {
            try {
                IEnumerable<FileInfo> files = Directory.GetFiles(subtitlesPath).Select(f => new FileInfo(f));       // get every file from the
                foreach(var file in files) 
                    File.Move(file.FullName, Path.Combine(inputPath, file.Name)); 
                if(!Directory.EnumerateFileSystemEntries(subtitlesPath).Any())
                    Directory.Delete(subtitlesPath);
            } catch(Exception e) { Console.WriteLine(e.ToString()); }
        }
    }
    public static void RenameFile(string from, string to) {
        if(File.Exists(from) && File.Exists(to)) {
            File.Delete(to);
            File.Move(from, to);
        }
    }
    public static void DeleteSubtitleFiles(string[] subFiles) {
        foreach(string subFile in subFiles) { 
            File.Delete(subFile); 
        }
    }
    public static bool SubfilesExist(string episodeFolder, string wildcard) {
        return Directory.GetFiles(episodeFolder, wildcard + ".idx").Any() && Directory.GetFiles(episodeFolder, wildcard + ".sub").Any();
    }
    public static int CountFullEngSubfiles(string folder) {
        int i = 0;
        foreach(string dir in Directory.GetDirectories(folder)) {
            if(Directory.Exists(dir + @"\Subs\")) {
                foreach(string subsfolder in Directory.GetFiles(dir + @"\Subs", "*eng.sub")) {
                    i++;
                }
            }
        }
        return i;
    }
    public static int CountSubtitlesTotal(string inputPath) {
        int i = 0;
        foreach(string dir in Directory.GetDirectories(inputPath))
            if(Directory.Exists(dir + @"\Subs\"))
                i++;
        return i;
    }
}
class mkvmerge {
    public static void Start(string episodeFolder) {
        try {
            string mkvInputPath = Directory.GetFiles(episodeFolder, "*.mkv").First();
            string mkvInputName = Path.GetFileNameWithoutExtension(mkvInputPath);
            string mkvOutputPath = mkvInputPath.Remove(mkvInputPath.Length - 4, 4); mkvOutputPath += "_new.mkv";
            string[] subtitlesIdx = Directory.GetFiles(episodeFolder, "*.idx");
            string[] subtitlesSub = Directory.GetFiles(episodeFolder, "*.sub");

            string subtitleEngFull = mkvInputName + "*eng";
            string subtitleEngForced = mkvInputName + "*eng*forced";
            string subtitleGerFull = mkvInputName;
            string subtitleGerForced = mkvInputName + "*forced";

            string mkvmerge = "& 'C:\\Program Files\\MKVToolNix\\mkvmerge.exe' -o '" + mkvOutputPath + "' -q --no-subtitles '" + mkvInputPath + "' ";
            if(Folder.SubfilesExist(episodeFolder, subtitleEngFull)) {
                mkvmerge += "--language 0:eng " +
                            "--track-name 0:Full '"
                            + Directory.GetFiles(episodeFolder, subtitleEngFull + ".idx")[0] + "' '"
                            + Directory.GetFiles(episodeFolder, subtitleEngFull + ".sub")[0] + "' ";
            }
            if(Folder.SubfilesExist(episodeFolder, subtitleEngForced)) {
                mkvmerge += "--language 0:eng " +
                            "--track-name 0:Forced '"
                            + Directory.GetFiles(episodeFolder, subtitleEngForced + ".idx")[0] + "' '"
                            + Directory.GetFiles(episodeFolder, subtitleEngForced + ".sub")[0] + "' ";
            }
            if(Folder.SubfilesExist(episodeFolder, subtitleGerFull)) {
                mkvmerge += "--language 0:deu " +
                            "--track-name 0:Full '"
                            + Directory.GetFiles(episodeFolder, subtitleGerFull + ".idx")[0] + "' '"
                            + Directory.GetFiles(episodeFolder, subtitleGerFull + ".sub")[0] + "' ";
            }
            if(Folder.SubfilesExist(episodeFolder, subtitleGerForced)) {
                mkvmerge += "--language 0:deu " +
                            "--track-name 0:Forced '"
                            + Directory.GetFiles(episodeFolder, subtitleGerForced + ".idx")[0] + "' '"
                            + Directory.GetFiles(episodeFolder, subtitleGerForced + ".sub")[0] + "' ";
            }

            if(File.Exists(mkvOutputPath)) { File.Delete(mkvOutputPath); }
            Console.WriteLine(mkvmerge);
            RunCommand(mkvmerge);
            Folder.RenameFile(mkvOutputPath, mkvInputPath);
            Folder.DeleteSubtitleFiles(subtitlesIdx);
            Folder.DeleteSubtitleFiles(subtitlesSub);
        } catch(Exception e) {
            Console.WriteLine(e.ToString());
        }
    }
    public static void RunCommand(string command) {
        ProcessStartInfo cmdsi = new ProcessStartInfo("powershell.exe");
        cmdsi.Arguments = command;
        Process cmd = Process.Start(cmdsi);
        // status output in percentage
        cmd.WaitForExit();
    }
}