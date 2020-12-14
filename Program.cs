using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SubtitlesApp {
    class App {
        private static void Main() {
            Console.Title = "SubtitleApp";

            Console.Write("Season Folder: ");
            string seasonFolder = Console.ReadLine();
            //string seasonFolder = @"E:\DOWNLOAD\Preacher.S03.German.DL.1080p.BluRay.x264-iNTENTiON";

            string[] episodeFolderList = Directory.GetDirectories(seasonFolder);

            Write("---", ConsoleColor.DarkRed); Write("STARTING", ConsoleColor.Black); WriteLine("---", ConsoleColor.DarkRed);
            WriteLine(" ", ConsoleColor.Black);
            WriteLine("We found " + episodeFolderList.Length + " episodes in the season folder", ConsoleColor.DarkBlue);
            WriteLine(" ", ConsoleColor.Black); WriteLine(" ", ConsoleColor.Black);

            int count = 1;

            foreach(string episodeFolder in episodeFolderList) {
                Write(" #" + count++.ToString(), ConsoleColor.DarkGreen);
                WriteLine(" " + episodeFolder, ConsoleColor.DarkGreen);
                WriteLine(" ", ConsoleColor.Black);
                WriteLine("      File count: " + Directory.GetFiles(episodeFolder).Length + " ", ConsoleColor.DarkGray);
                WriteLine(" ", ConsoleColor.Black);

                string[] allFiles = Directory.GetFiles(episodeFolder);

                foreach(string file in allFiles) {
                    WriteLine("      " + file, ConsoleColor.Black);
                }

                WriteLine(" ", ConsoleColor.Black);

                Folder.DeleteNFO(episodeFolder);
                if(Folder.CheckForSubFolder(episodeFolder)) { 
                    WriteLine("      We found a subtitle folder, proceeding moving files and deleting folder...", ConsoleColor.Black); 
                    Folder.MoveFiles(episodeFolder); 
                }
                if(Directory.GetFiles(episodeFolder).Length > 1) { 
                    WriteLine("      Folder containts more than two files, proceeding importing subtitles into mkv...", ConsoleColor.Black); 
                    MKV.Import(episodeFolder); 
                } else {
                    WriteLine("      We couldn't find any subtitles, proceeding with next episode...", ConsoleColor.Black);
                }
                Console.WriteLine();
            }
            //Console.Read();
        }
        public static void Write(string inputText, ConsoleColor inputColor) {
            Console.BackgroundColor = inputColor;
            Console.Write(" " + inputText + " ");
            Console.ResetColor();
        }
        public static void WriteLine(string inputText, ConsoleColor inputColor) {
            Console.BackgroundColor = inputColor;
            Console.WriteLine(inputText);
            Console.ResetColor();
        }
    }
    class Folder {
        public static bool CheckForSubFolder(string episodeFolder) {
            return Directory.Exists(episodeFolder + @"\Subs");
        }
        public static void MoveFiles(string episodeFolder) {
            string subFolder = episodeFolder + @"\Subs";

            try {
                IEnumerable<FileInfo> files = Directory.GetFiles(subFolder).Select(f => new FileInfo(f));

                App.WriteLine("   ", ConsoleColor.Black);
                foreach(var file in files) {
                    File.Move(file.FullName, Path.Combine(episodeFolder, file.Name));
                    App.WriteLine("      File " + file.ToString() + " moved", ConsoleColor.Black);
                }
                
                if(IsDirectoryEmpty(subFolder)) { 
                    Directory.Delete(subFolder);
                    App.WriteLine("      Folder " + subFolder.ToString() + " deleted", ConsoleColor.Black);
                };
                App.WriteLine("   ", ConsoleColor.Black);
            }
            catch(Exception e) { Console.WriteLine(e.ToString()); }

        }
        public static bool IsDirectoryEmpty(string path) {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }
        public static void DeleteNFO(string episodeFolder) {
            try {
                foreach(string nfo in Directory.EnumerateFiles(episodeFolder, "*.nfo")) {
                    File.Delete(nfo);
                    App.WriteLine(episodeFolder + "  | .NFO deleted", ConsoleColor.DarkGreen);
                }
            }
            catch(Exception e) { Console.WriteLine(e.ToString()); };
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

                string subtitleEngFull = mkvInputName + "-eng";
                string subtitleEngForced = mkvInputName + "-eng-forced";
                string subtitleGerFull = mkvInputName;
                string subtitleGerForced = mkvInputName + "-forced";

                string mkvmerge = "mkvmerge -o '" + mkvOutputPath + "' --no-subtitles '" + mkvInputPath + "' ";
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
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Gray;
                RunCommand(mkvmerge);
                Console.ResetColor();
                Folder.RenameFile(mkvOutputPath, mkvInputPath);
                Folder.DeleteSubtitleFiles(subtitlesIdx);
                Folder.DeleteSubtitleFiles(subtitlesSub);

                App.WriteLine("  DONE  ", ConsoleColor.DarkGreen);
                Console.WriteLine();
            } catch(Exception e) { App.WriteLine(e.ToString(), ConsoleColor.DarkRed); }
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
