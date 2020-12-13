using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using 

namespace SubtitlesApp {
    class App {
        private static void Main() {
            Console.Title = "SubtitleApp";

            //Console.Write("Season Folder: ");
            //string seasonFolder = Console.ReadLine();
            string seasonFolder = @"C:\Mr Robot - S02";

            string[] episodeFolderList = Directory.GetDirectories(seasonFolder);

            foreach(string episodeFolder in episodeFolderList) {
                if(Folder.CheckForSubFolder(episodeFolder)) {
                    Folder.MoveFiles(episodeFolder);
                    MKV.Import(episodeFolder);
                } else {}
                Folder.DeleteNFO(episodeFolder);
                Console.WriteLine();
            }
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
    }
    class MKV {
        public static void Import(string episodeFolder) {
            try {
                string[] fileList = Directory.GetFiles(episodeFolder);


                
                //foreach(string file in fileList) {
                //    App.WriteLine(file, ConsoleColor.Cyan);
                //
                //    //string endsWithidxEnglishFull =     "eng.idx";
                //    //string endsWithsubEnglishFull =     "eng.sub";
                //    //string endsWithidxEnglishForced =   "eng-forced.idx";
                //    //string endsWithsubEnglishForced =   "eng-forced.sub";
                //    //string endsWithidxGermanFull =      ".idx";
                //    //string endsWithsubGermanFull =      ".sub";
                //    //string endsWithidxGermanForced =    "-forced.dix";
                //    string endsWithsubGermanForced =    "-forced.sub";
                //
                //    switch(file) {
                //        case germanForced gerForced when file.EndsWith(endsWithsubGermanForced):
                //            App.Write("hooray", ConsoleColor.Magenta);
                //    }
                //
                //    if(file.EndsWith(endsWithsubGermanForced)) {
                //        App.WriteLine("IT WORKED", ConsoleColor.Magenta);
                //    }
                //}
            } catch(Exception e) {
                App.WriteLine(e.ToString(), ConsoleColor.DarkRed);
            }
        }
    }
}
