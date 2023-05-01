using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System;

class SubMerger {
    public string Path { get; set; }
    public string FolderName { get; set; }
    public string[] SubfoldersList { get; set; }
    public bool MultipleFiles { get; set; }

    public SubMerger(string argPath, string argName) {
        Path = argPath;
        FolderName = argName;

        SubfoldersList = Directory.GetDirectories(Path);
        Array.Sort(SubfoldersList);

        // this Regex checks if there's just "S02" tag in the directory name.
        // A movie would't have anything, a single episode S02E01 (f.ex.), so only a season folder would've only S01
        MultipleFiles = Regex.Match(Path, @"S[0-9]{2}[^E]").Success ? true : false;
    }
    

    public int Run() {
        // MOVIE/SE E:\DOWNLOAD\TestMovie.2018.German.1080p.BluRay.x264-RAiNBOW\*.mkv                                                      (maybe folder Subs and Sample!) (subfolders >= 2)
        // COMPLETE E:\DOWNLOAD\TestSeries.S01.German.1080p.BluRay.x264-RAiNBOW\Test.S01E01.German.1080p.BluRay.x264-RAiNBOW\*.mkv
        //                                  args[0] = folder                   |              subfolder   

        // arguments to variables
        // args[0] should be path to the movie/season folder
        // args[1] should be the folder name of the media

        // Header
        Output.WriteHeader(Path, SubfoldersList.Length);

        try {
            if(MultipleFiles) {
                Output.WriteSeasonInfo(Path);

                List<string> queue = Folder.GetQueue(Path);
                int progress = 0;

                foreach(string item in queue) {
                    progress++;
                    Console.Write(
                         "{0} | (M) mkvmerge: #{1} of {2} \t({3})\t", DateTime.Now.ToString("HH:mm:ss"), progress.ToString("00"), queue.Count, Regex.Match(item,@"E[0-9]{2}").Groups[0].Value);
                    Folder.MoveSubsToRoot(item);
                    mkvmerge.Initialize(item);
                    Console.WriteLine("\ncompleted");
                }

                if(progress == 0 && queue.Count == 0) {
                    Console.WriteLine("No files found. Exiting...");
                    return 1;
                } else if(progress == queue.Count) {
                    Console.WriteLine("{0} | (M) mkvmerge done", DateTime.Now.ToString("HH:mm:ss"));
                    return 0; // 0 -> Success
                } else return 1;

            } else if(!MultipleFiles) {
                Output.WriteInfo(Path);

                if(Directory.Exists(Path + @"\Subs")) {
                    Console.WriteLine("{0} | (S) mkvmerge in progress", DateTime.Now.ToString("HH:mm:ss"));
                    Folder.MoveSubsToRoot(Path);
                    mkvmerge.Initialize(Path);
                    Console.WriteLine("{0} | (S) mkvmerge done", DateTime.Now.ToString("HH:mm:ss"));
                    return 0;
                } else if(Folder.SubfilesExist(Path, "*")) {
                    Console.WriteLine("{0} | (S) mkvmerge in progress", DateTime.Now.ToString("HH:mm:ss"));
                    mkvmerge.Initialize(Path);
                    Console.WriteLine("{0} | (S) mkvmerge done", DateTime.Now.ToString("HH:mm:ss"));
                    return 0;
                } else {
                    Console.WriteLine("Exiting, no subtitles found...");
                    return 0;
                }
            } else {
                Console.WriteLine("Error, could not determine type!");
                return 1;
            }
        }
        catch(Exception e) {
            Console.WriteLine(e.ToString());
            return 1;
        };
    }

}