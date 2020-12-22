using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace SubtitlesApp {
    class App {
        private static int Main(string[] args) {
            // MOVIE/SE E:\DOWNLOAD\TestMovie.2018.German.1080p.BluRay.x264-RAiNBOW\*.mkv                                                      (maybe folder Subs and Sample!) (subfolders >= 2)
            // COMPLETE E:\DOWNLOAD\TestSeries.S01.German.1080p.BluRay.x264-RAiNBOW\Test.S01E01.German.1080p.BluRay.x264-RAiNBOW\*.mkv
            //                                  args[0] = folder                   |              subfolder   

            string      root                = args[0];
            string[]    subfolders          = Directory.GetDirectories(root);
            using       StreamWriter log    = new StreamWriter(@"E:\DOWNLOAD\.scripts\logs\" + args[1] + ".log");
            string      xyz                 = Folder.DetermineType(root);
            
            log.AutoFlush = true; // writes any text instantly to the file, with false it only writes when returning
            log.WriteLine(DateTime.Now.ToString("dd.MM HH:mm:ss") + "\n - " + root + "\\(" + subfolders.Length + ")\n");

            try {
                int i = 0;
            
                if(subfolders.Length > 1 && !Folder.DoesExist(root, "Sample")) { // #1: Check for multiple folders (indicates a full season) #2: If a sample folder exists it's more likely a movie or single episode
                    int countEngSubs = Folder.CountExistingSubfiles(root);

                    if(countEngSubs != subfolders.Length) {
                        int missingSubs = subfolders.Length - countEngSubs;
                        Console.WriteLine("WARNING! Subtitles are MISSING (" + missingSubs + ")");
                        log.WriteLine("WARNING! Subtitles are MISSING (MISSING: " + missingSubs + ") (FOUND: " + countEngSubs + ", TOTAL: " + subfolders.Length + ")");
                        Folder.WriteAllMissingSubtitles(subfolders, log);
                        Thread.Sleep(10000);
                    }

                    foreach(string episodeFolder in subfolders) {
                        i++;
                        log.Write("(M) mkvmerge: #" + i.ToString() + " of " + subfolders.Length + " (" + episodeFolder.Remove(0, root.Length).Remove(0,1) + ")");
                        Console.Write("(M) mkvmerge: #" + i.ToString() + " of " + subfolders.Length + " (" + episodeFolder.Remove(0, root.Length).Remove(0,1) + ")");

                        Folder.DeleteNFO(episodeFolder);
                        if(Folder.DoesExist(episodeFolder, "subs")) { Folder.MoveFiles(episodeFolder); }
                        if(Directory.GetFiles(episodeFolder).Length > 1) { MKV.Import(episodeFolder); }

                        log.Write("\tcompleted\n");
                        Console.Write("\tcompleted\n");
                    }

                    log.Write("\ndone");
                    Console.Write("done");
                    return 0;
                } else { // < 1 for single episodes or movies
                    log.WriteLine("(S) mkvmerge in progress");
                    Console.WriteLine("(S) mkvmerge in progress");

                    Folder.DeleteNFO(root);
                    if(Folder.DoesExist(root, "subs")) { Folder.MoveFiles(root); }
                    if(Directory.GetFiles(root).Length > 1) { MKV.Import(root); }

                    log.Write("\ndone");
                    Console.Write("done");
                    return 0; // 0 -> Success
                }
            } catch(Exception e) { log.WriteLine(e.ToString()); return 1; }

            //return 0;
        }
        public static void Write(string type, string message, bool newline) {

        }

    }
    class Folder {
        public static string DetermineType(string root) {
            if(1 == 3) {
                return "single";
            } else if(1 == 2) {
                return "multiple";
            } else {
                return null;
            }
        }
        public static bool DoesExist(string path, string folder) {
            return Directory.Exists(path + @"\" + folder);
        }
        public static void MoveFiles(string episodeFolder) {
            string subFolder = episodeFolder + @"\Subs";

            try {
                IEnumerable<FileInfo> files = Directory.GetFiles(subFolder).Select(f => new FileInfo(f));

                foreach(var file in files) {
                    File.Move(file.FullName, Path.Combine(episodeFolder, file.Name));
                }
                
                if(IsDirectoryEmpty(subFolder)) { 
                    Directory.Delete(subFolder);
                };
            }
            catch(Exception e) { Console.WriteLine(e.ToString()); }

        }
        public static bool IsDirectoryEmpty(string path) {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }
        public static void DeleteNFO(string episodeFolder) {
            try {
                foreach(string nfo in Directory.EnumerateFiles(episodeFolder, "*.nfo")) { File.Delete(nfo); }
            } catch(Exception e) { Console.WriteLine(e.ToString()); };
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
        public static void WriteAllMissingSubtitles(string[] subfolders, StreamWriter log) {
            foreach(string subfolder in subfolders)
                if(!Directory.Exists(subfolder + @"\Subs\"))
                    log.WriteLine("|- " + subfolder.Remove(0, 0).Remove(0, 1));
            log.Write("\n");
        }
        public static int CountExistingSubfiles(string folder) {
            int countEngSubs = 0;
            foreach(string subfolder in Directory.GetDirectories(folder))
                if(Directory.Exists(subfolder + @"\Subs\"))
                    foreach(string subsfolder in Directory.GetFiles(subfolder + @"\Subs", "*eng.sub"))
                        countEngSubs++;
            return countEngSubs;
        }
    }
    class MKV {
        public static void Import(string episodeFolder) {
            try {
                string      mkvInputPath    = Directory.GetFiles(episodeFolder, "*.mkv").First();
                string      mkvInputName    = Path.GetFileNameWithoutExtension(mkvInputPath);
                string      mkvOutputPath   = mkvInputPath.Remove(mkvInputPath.Length - 4, 4); mkvOutputPath += "_new.mkv";
                string[]    subtitlesIdx = Directory.GetFiles(episodeFolder, "*.idx");
                string[]    subtitlesSub = Directory.GetFiles(episodeFolder, "*.sub");

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
                    mkvmerge += "--language 0:ger " +
                                "--track-name 0:Full '"
                                + Directory.GetFiles(episodeFolder, subtitleGerFull + ".idx")[0] + "' '" 
                                + Directory.GetFiles(episodeFolder, subtitleGerFull + ".sub")[0] + "' ";
                }
                if(Folder.SubfilesExist(episodeFolder, subtitleGerForced)) {
                    mkvmerge += "--language 0:ger " +
                                "--track-name 0:Forced '"
                                + Directory.GetFiles(episodeFolder, subtitleGerForced + ".idx")[0] + "' '"
                                + Directory.GetFiles(episodeFolder, subtitleGerForced + ".sub")[0] + "' ";
                }

                if(File.Exists(mkvOutputPath)) { File.Delete(mkvOutputPath); }
                
                RunCommand(mkvmerge);
                Folder.RenameFile(mkvOutputPath, mkvInputPath);
                Folder.DeleteSubtitleFiles(subtitlesIdx);
                Folder.DeleteSubtitleFiles(subtitlesSub);
            } catch(Exception e) { Console.WriteLine(e.ToString()); }
        }
        public static void RunCommand(string command) {
            ProcessStartInfo cmdsi = new ProcessStartInfo("powershell.exe");
            cmdsi.Arguments = command;
            Process cmd = Process.Start(cmdsi);
            // status output in percentage
            cmd.WaitForExit();
        }
    }
}
