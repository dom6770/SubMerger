using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SubtitlesApp {
    class App {
        private static int Main(string[] args) {
            System.Threading.Thread.Sleep(2000); //wait two seconds for SABnzbd PP

            string downloadFolder = args[0];
            string[] subfolderList = Directory.GetDirectories(downloadFolder);

            Console.WriteLine("Starting...");
            Console.WriteLine("downloadFolder: " + downloadFolder);
            foreach(string subfolder in subfolderList) { Console.WriteLine("subfolder: " + subfolder); }
            Console.WriteLine(subfolderList.Length + " Episodes found");
            Console.WriteLine("   ");

            int count = 1;

            //if(Folder.DoesExist(downloadFolder, "Sample")) { Directory.Delete(downloadFolder + @"\Sample"); }


            try {
                if(subfolderList.Count() > 1 && !Folder.DoesExist(downloadFolder, "Sample")) { // > 1 for full Seasons (multiple episodes inside the season folder)
                    foreach(string episodeFolder in subfolderList) {
                        Console.WriteLine(count++.ToString() + "/" + subfolderList.Length.ToString());

                        Folder.DeleteNFO(episodeFolder);
                        if(Folder.DoesExist(episodeFolder, "subs")) { Folder.MoveFiles(episodeFolder); }
                        if(Directory.GetFiles(episodeFolder).Length > 1) { MKV.Import(episodeFolder); }
                    }
                    Console.WriteLine("Script finished.");
                    return 0;
                } else { // < 1 for single episodes or movies
                    Console.WriteLine("Movie/Episode found - starting mkvmerge");

                    Folder.DeleteNFO(downloadFolder);
                    if(Folder.DoesExist(downloadFolder, "subs")) { Folder.MoveFiles(downloadFolder); }
                    if(Directory.GetFiles(downloadFolder).Length > 1) { MKV.Import(downloadFolder); }

                    Console.WriteLine("Script finished.");
                    return 0; // 0 -> Success
                }
            } catch(Exception e) { Console.WriteLine(e.ToString()); return 1; }
        }
    }
    class Folder {
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
        public static void BuildCommand(string episodeFolder) {

        }
        public static void RunCommand(string command) {
            ProcessStartInfo cmdsi = new ProcessStartInfo("powershell.exe");
            cmdsi.Arguments = command;
            Process cmd = Process.Start(cmdsi);
            cmd.WaitForExit();
        }
    }
}
