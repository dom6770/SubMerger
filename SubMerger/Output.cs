﻿using System.IO;
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
            if(Directory.Exists(dir + @"\Subs") && Directory.GetFiles(dir + @"\Subs\","*eng.*").Any())
                Console.Write(Regex.Match(dir,@"E[0-9]{2}").Groups[0].Value + " ");
        Console.Write(")\n");

        // Header - Subtitles Missing Eng Found
        Console.Write("|  - !Full    {0}  ( ",amountSubMissingEng.ToString("00"));
        foreach(string dir in dirArray)
            if(Directory.Exists(dir + @"\Subs") && !Directory.GetFiles(dir + @"\Subs\","*eng.*").Any())
                Console.Write(Regex.Match(dir,@"E[0-9]{2}").Groups[0].Value + " ");
        Console.Write(")\n");

        // Header - Subtitles Missing Eng Found
        Console.Write("|  - Missing  {0}  ( ",amountSubMissing.ToString("00"));
        foreach(string dir in dirArray)
            if(!Directory.Exists(dir + @"\Subs"))
                Console.Write(Regex.Match(dir,@"E[0-9]{2}").Groups[0].Value + " ");
        Console.WriteLine(")\n");
    }
    public static void WriteInfo(string inputPath) {
        Console.WriteLine("Single File");
        // Header - Subtitles
        Console.Write("|- Subtitles ");

        if(Directory.Exists(inputPath + @"\Subs")) {
            Console.Write("found in the \\Subs directory\n");
            if(Directory.GetFiles(inputPath + @"\Subs","*.sub").Length > 0)
                foreach(string file in Directory.GetFiles(inputPath + @"\Subs","*.sub"))
                    Console.WriteLine("|-- " + file.Remove(0,inputPath.Length + 6));
        } else if(Directory.Exists(inputPath)) {
            Console.Write("found in the root directory\n");
            if (Directory.GetFiles(inputPath, "*.sub").Length > 0)
                foreach (string file in Directory.GetFiles(inputPath, "*.sub"))
                    Console.WriteLine("|-- " + file);
        } else Console.Write("missing");
        Console.WriteLine("");
    }
}