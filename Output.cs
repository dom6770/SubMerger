using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Linq;

class Output {
    public static void Write(StreamWriter log,string output) {
        Console.Write(output);
        log.Write(output);
    }
    public static void WriteLine(StreamWriter log,string output) {
        Console.WriteLine(output);
        log.WriteLine(output);
    }
    public static void WriteSeasonInfo(StreamWriter log,string inputPath) {
        string[] dirArray = Directory.GetDirectories(inputPath);
        int amountSubTotal = Folder.CountSubtitlesTotal(inputPath);
        int amountSubFullEng = Folder.CountFullEngSubfiles(inputPath);
        int amountSubMissingEng = amountSubTotal - amountSubFullEng;
        int amountSubMissing = dirArray.Length - amountSubTotal;

        // Header - Type
        Output.WriteLine(log,"Season");

        // Header - Episodes Amount
        int episodeCount = 0;
        foreach(string dir in Directory.GetDirectories(inputPath))
            if(Regex.Match(dir,@"S[0-9]{2}E[0-9]{2}").Success)
                episodeCount++;
        Write(log,"|- Episodes: " + episodeCount + "\n");

        // Header - Subtitles
        WriteLine(log,"|- Subtitles");
        // Header - Subtitles Total Found
        Write(log,"|  - Full     " + amountSubFullEng.ToString("00") + "  ( ");
        foreach(string dir in dirArray)
            if(Directory.Exists(dir + @"\Subs") && Directory.GetFiles(dir + @"\Subs\","*eng.*").Any())
                Write(log,Regex.Match(dir,@"E[0-9]{2}").Groups[0].Value + " ");
        Write(log,")\n");

        // Header - Subtitles Missing Eng Found
        Write(log,"|  - !Full    " + amountSubMissingEng.ToString("00") + "  ( ");
        foreach(string dir in dirArray)
            if(Directory.Exists(dir + @"\Subs") && !Directory.GetFiles(dir + @"\Subs\","*eng.*").Any())
                Write(log,Regex.Match(dir,@"E[0-9]{2}").Groups[0].Value + " ");
        Write(log,")\n");

        // Header - Subtitles Missing Eng Found
        Write(log,"|  - Missing  " + amountSubMissing.ToString("00") + "  ( ");
        foreach(string dir in dirArray)
            if(!Directory.Exists(dir + @"\Subs"))
                Write(log,Regex.Match(dir,@"E[0-9]{2}").Groups[0].Value + " ");
        WriteLine(log,")\n");
    }
    public static void WriteInfo(StreamWriter log,string inputPath) {
        WriteLine(log,"Single File");
        // Header - Subtitles
        Write(log,"|- Subtitles");

        if(Directory.Exists(inputPath + @"\Subs")) {
            Write(log,"\n");
            if(Directory.GetFiles(inputPath + @"\Subs","*.sub").Length > 0)
                foreach(string file in Directory.GetFiles(inputPath + @"\Subs","*.sub"))
                    WriteLine(log,"|  - " + file.Remove(0,inputPath.Length + 6));
        } else Write(log," missing");
        WriteLine(log,"");
    }
}