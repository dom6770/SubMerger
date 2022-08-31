using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System;

class SubMerger {
    public string Path { get; set; }
    public string FolderName { get; set; }
    public string[] SubfoldersList { get; set; }
    public bool IsSeasonFolder { get; set; }

    public SubMerger(string argPath, string argName) {
        Path = argPath;
        FolderName = argName;

        SubfoldersList = Directory.GetDirectories(Path);
        Array.Sort(SubfoldersList);

        IsSeasonFolder = Regex.Match(Path,@"S[0-9]{2}[^E]").Success ? true : false;
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
            if(IsSeasonFolder) {
                Output.WriteSeasonInfo(Path);

                List<string> queue = Folder.GetQueue(Path);
                int progress = 0;

                foreach(string item in queue) {
                    progress++;
                    Console.Write(DateTime.Now.ToString("HH:mm:ss") + " | (M) mkvmerge: #" + progress.ToString("00") + " of " + queue.Count + " \t(" + Regex.Match(item,@"E[0-9]{2}").Groups[0].Value + ")\t");
                    Folder.MoveSubsToRoot(item);
                    mkvmerge.Initialize(item);
                    Console.WriteLine("\ncompleted");
                }

                if(progress == 0 && queue.Count == 0) {
                    Console.WriteLine("No files found. Exiting...");
                    return 1;
                } else if(progress == queue.Count) {
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " | (M) mkvmerge done");
                    return 0; // 0 -> Success
                } else return 1;

            } else if(!IsSeasonFolder) {
                Output.WriteInfo(Path);

                if(Directory.Exists(Path + @"\Subs")) {
                    Console.WriteLine("{0} | (S) mkvmerge in progress", DateTime.Now.ToString("HH:mm:ss"));
                    Folder.MoveSubsToRoot(Path);
                    mkvmerge.Initialize(Path);
                    Console.WriteLine("{0} | (S) mkvmerge done", DateTime.Now.ToString("HH:mm:ss"));
                    return 0;
                } else {
                    Console.WriteLine("Exiting...");
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