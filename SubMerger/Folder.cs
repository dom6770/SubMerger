﻿using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

class Folder {
    public static List<string> GetQueue(string inputPath) {
        string[] dirArray = Directory.GetDirectories(inputPath);
        List<string> queue = new();
        foreach(string dir in dirArray) {
            if(Directory.Exists(Path.Combine(dir, "Subs")) && Directory.GetFiles(Path.Combine(dir, "Subs"),"*eng*").Any())
                queue.Add(dir);
        };
        return queue;
    }
    public static void MoveSubsToRoot(string inputPath) {
        string subtitlesPath = Directory.GetDirectories(inputPath)
            .FirstOrDefault(d => string.Equals(Path.GetFileName(d), "Subs", StringComparison.OrdinalIgnoreCase));
        
        if(Directory.Exists(subtitlesPath)) {
            try {
                IEnumerable<FileInfo> files = Directory.GetFiles(subtitlesPath).Select(f => new FileInfo(f));       // get every file from the
                foreach(var file in files)
                    File.Move(file.FullName, Path.Combine(inputPath, file.Name));
                if(!Directory.EnumerateFileSystemEntries(subtitlesPath).Any())
                    Directory.Delete(subtitlesPath);
            }
            catch(Exception e) { Console.WriteLine(e.ToString()); }
        }
    }
    public static void RenameFile(string from,string to) {
        if(File.Exists(from) && File.Exists(to)) {
            File.Delete(to);
            File.Move(from,to);
        }
    }
    public static void DeleteSubtitleFiles(string[] subFiles) {
        foreach(string subFile in subFiles) {
            File.Delete(subFile);
        }
    }
    public static bool SubfilesExist(string episodeFolder,string wildcard) {
        return Directory.GetFiles(episodeFolder,wildcard + ".idx").Any() && Directory.GetFiles(episodeFolder,wildcard + ".sub").Any();
    }
    public static int CountFullEngSubfiles(string folder) {
        int i = 0;
        foreach(string dir in Directory.GetDirectories(folder)) {
            if(Directory.Exists(Path.Combine(dir, "Subs"))) {
                foreach(string subsfolder in Directory.GetFiles(Path.Combine(dir, "Subs"),"*eng.sub")) {
                    i++;
                }
            }
        }
        return i;
    }
    public static int CountSubtitlesTotal(string inputPath) {
        int i = 0;
        foreach(string dir in Directory.GetDirectories(inputPath))
            if(Directory.Exists(Path.Combine(dir, "Subs")))
                i++;
        return i;
    }
}