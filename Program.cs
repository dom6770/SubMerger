using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SubtitlesApp {
    class App {
        private static void Main() {
            Console.Title = "SubtitleApp";

            //Console.Write("Season Folder: ");
            // string pathToSeasonFolder = Console.ReadLine();
            string seasonFolder = @"C:\Mr Robot - S02";

            string[] episodeFolderList = Directory.GetDirectories(seasonFolder);

            foreach(string episodeFolder in episodeFolderList) {
                if(Subtitles.CheckForSubFolder(episodeFolder)) {
                    Subtitles.MoveFiles(episodeFolder);
                    Subtitles.ImportToMkv(episodeFolder);
                } else {}
                DeleteNFO(episodeFolder);
                Console.WriteLine();
            }
        }

        public static void WriteLine(string text, ConsoleColor ccolor) {
            //string colorFull = "ConsoleColor." + color;
            //ConsoleColor ccolor = colorFull;

            Console.BackgroundColor = ccolor;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void DeleteNFO(string episodeFolder) {
            try {
                foreach(string nfo in Directory.EnumerateFiles(episodeFolder, "*.nfo")) {
                    File.Delete(nfo);
                    WriteLine(episodeFolder + "  | .NFO deleted", ConsoleColor.DarkGreen);
                }
            } catch(Exception e) { Console.WriteLine(e.ToString()); };
        }
    }
    class Subtitles {
        public static bool CheckForSubFolder(string episodeFolder) {
            if(Directory.Exists(episodeFolder + @"\Subs")) {
                App.WriteLine(episodeFolder + "  | Subtitle folder found!", ConsoleColor.DarkGreen);
                return true;
            } else {
                App.WriteLine(episodeFolder + "  | Error: Subtitle folder NOT found", ConsoleColor.DarkRed);
                return false;
            }
        }

        public static void MoveFiles(string episodeFolder) {
            string subFolder = episodeFolder + @"\Subs";

            try {
                IEnumerable<FileInfo> files = Directory.GetFiles(subFolder).Select(f => new FileInfo(f));
                foreach(var file in files) {
                    File.Move(file.FullName, Path.Combine(episodeFolder, file.Name));
                    App.WriteLine("| moving " + file.ToString(), ConsoleColor.DarkGreen);
                }
                
                if(IsDirectoryEmpty(subFolder)) { 
                    Directory.Delete(subFolder);
                    App.WriteLine("| deleting " + subFolder.ToString(), ConsoleColor.DarkGreen);
                };
            }
            catch(Exception e) { Console.WriteLine(e.ToString()); }

        }

        public static void ImportToMkv(string episodeFolder) {
            try {

            } catch(Exception e) {
                App.WriteLine(e.ToString(), ConsoleColor.DarkRed);
            }
        }

        public static bool IsDirectoryEmpty(string path) {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }
    }
}
