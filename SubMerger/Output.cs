using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Linq;

class Output {

    public static void WriteHeader(string inputPath, int subfoldersCount) {
        Console.Write(
        "Start Time: {0}\n" +
        "|- Folder: {1}\\({2})\n" +
        "|- Type: ",
        DateTime.Now.ToString("ddd dd.MM.yyyy HH:mm:ss"),inputPath,subfoldersCount);
    }

    public static void WriteSeasonInfo(string inputPath) {
        string[] dirArray = Directory.GetDirectories(inputPath);
        int amountSubTotal = Folder.CountSubtitlesTotal(inputPath);
        int amountSubFullEng = Folder.CountFullEngSubfiles(inputPath);
        int amountSubMissingEng = amountSubTotal - amountSubFullEng;
        int amountSubMissing = dirArray.Length - amountSubTotal;


        // Header - Type
        Console.WriteLine("Season");

        // Header - Episodes Amount
        int episodeCount = 0;
        foreach(string dir in Directory.GetDirectories(inputPath))
            if(Regex.Match(dir,@"S[0-9]{2}E[0-9]{2}").Success)
                episodeCount++;
        Console.Write("|- Episodes: {0}\n",episodeCount);

        // Header - Subtitles
        Console.WriteLine("|- Subtitles");
        // Header - Subtitles Total Found
        Console.Write("|  - Full     {0}  ( ",amountSubFullEng.ToString("00"));
        foreach(string dir in dirArray)
            if(Directory.Exists(Path.Combine(dir, "Subs")) && Directory.GetFiles(Path.Combine(dir, "Subs"),"*eng.*").Any())
                Console.Write(Regex.Match(dir,@"E[0-9]{2}").Groups[0].Value + " ");
        Console.Write(")\n");

        // Header - Subtitles Missing Eng Found
        Console.Write("|  - !Full    {0}  ( ",amountSubMissingEng.ToString("00"));
        foreach(string dir in dirArray)
            if(Directory.Exists(Path.Combine(dir, "Subs")) && !Directory.GetFiles(Path.Combine(dir, "Subs"),"*eng.*").Any())
                Console.Write(Regex.Match(dir,@"E[0-9]{2}").Groups[0].Value + " ");
        Console.Write(")\n");

        // Header - Subtitles Missing Eng Found
        Console.Write("|  - Missing  {0}  ( ",amountSubMissing.ToString("00"));
        foreach(string dir in dirArray)
            if(!Directory.Exists(Path.Combine(dir, "Subs")))
                Console.Write(Regex.Match(dir,@"E[0-9]{2}").Groups[0].Value + " ");
        Console.WriteLine(")\n");
    }
    public static void WriteInfo(string inputPath) {
        Console.WriteLine("Single File");
        // Header - Subtitles
        Console.Write("|- Subtitles ");

        if(Directory.Exists(Path.Combine(inputPath, "Subs"))) {
            Console.Write("found in the Subs subdirectory\n");
            if(Directory.GetFiles(Path.Combine(inputPath, "Subs"),"*.idx").Length > 0)
                foreach(string file in Directory.GetFiles(Path.Combine(inputPath, "Subs"),"*.idx"))
                    Console.WriteLine("|-> " + file.Remove(0,inputPath.Length + 6));
        } else if(Directory.Exists(inputPath)) {
            Console.Write("found in the root directory\n");
            if (Directory.GetFiles(inputPath, "*.idx").Length > 0)
                foreach (string file in Directory.GetFiles(inputPath, "*.idx"))
                    Console.WriteLine("|-> " + file);
        } else Console.Write("missing");
        Console.WriteLine("");
    }
}