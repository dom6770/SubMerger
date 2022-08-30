using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System;

class SubMerger {
    public int Run(string[] args) {
        // MOVIE/SE E:\DOWNLOAD\TestMovie.2018.German.1080p.BluRay.x264-RAiNBOW\*.mkv                                                      (maybe folder Subs and Sample!) (subfolders >= 2)
        // COMPLETE E:\DOWNLOAD\TestSeries.S01.German.1080p.BluRay.x264-RAiNBOW\Test.S01E01.German.1080p.BluRay.x264-RAiNBOW\*.mkv
        //                                  args[0] = folder                   |              subfolder   

        // arguments to variables
        // args[0] should be path to the movie/season folder
        // args[1] should be the folder name of the media

        string inputPath = args[0];
        string inputName = args.Length > 1 ? args[1] : DateTime.Now.ToString("dd-MM_HH-mm");

        // other variables
        string[] subfolders = Directory.GetDirectories(inputPath); // get all folders in the path
        int dirArrayCount = subfolders.Length;                    // amount of folders
        Array.Sort(subfolders);                                  // sorts from A-Z to have a correct episode order

        string logFile = @"E:\TOOLS\SABnzbd\logs" + inputName + ".log";

        // logger
        using StreamWriter log = new StreamWriter(logFile) {
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
                Output.WriteSeasonInfo(log,inputPath);

                List<string> queue = Folder.GetQueue(inputPath);
                int progress = 0;

                foreach(string item in queue) {
                    progress++;
                    Output.Write(log,DateTime.Now.ToString("HH:mm:ss") + " | (M) mkvmerge: #" + progress.ToString("00") + " of " + queue.Count + " \t(" + Regex.Match(item,@"E[0-9]{2}").Groups[0].Value + ")\t");
                    Folder.MoveSubsToRoot(item);
                    mkvmerge.Initialize(item);
                    log.Write("\tcompleted\n");
                    Console.WriteLine("\ncompleted");
                }

                if(progress == 0 && queue.Count == 0) {
                    Output.WriteLine(log,"Exiting...");
                    return 0;
                } else if(progress == queue.Count) {
                    Output.WriteLine(log,DateTime.Now.ToString("HH:mm:ss") + " | (M) mkvmerge done");
                    return 0; // 0 -> Success
                } else return 1;

            } else if(!isSeasonFolder) {
                Output.WriteInfo(log,inputPath);

                if(Directory.Exists(inputPath + @"\Subs")) {
                    Output.WriteLine(log,DateTime.Now.ToString("HH:mm:ss") + " | (S) mkvmerge in progress");
                    Folder.MoveSubsToRoot(inputPath);
                    mkvmerge.Initialize(inputPath);
                    Output.WriteLine(log,DateTime.Now.ToString("HH:mm:ss") + " | (S) mkvmerge done");
                    return 0;
                } else {
                    Output.WriteLine(log,"Exiting...");
                    return 0;
                }
            } else {
                Output.WriteLine(log,"Error, could not determine type!");
                return 1;
            }
        }
        catch(Exception e) {
            Output.WriteLine(log,e.ToString());
            return 1;
        };
        return 1;
    }

}