using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

class mkvmerge {
    public static void Initialize(string episodeFolder) {
        try {
            string mkvInputPath = Directory.GetFiles(episodeFolder, "*.mkv").First();
            string mkvInputName = Path.GetFileNameWithoutExtension(mkvInputPath);
            string mkvOutputPath = mkvInputPath.Remove(mkvInputPath.Length - 4, 4) + "_new.mkv";

            string[] subtitlesIdx = Directory.GetFiles(episodeFolder, "*.idx");
            string[] subtitlesSub = Directory.GetFiles(episodeFolder, "*.sub");

            string subtitleEngFull = mkvInputName + "*eng";
            string subtitleEngForced = mkvInputName + "*eng*forced";
            string subtitleGerFull = mkvInputName;
            string subtitleGerForced = mkvInputName + "*forced";

            var mkvmergeArgs = new[]{
                "--output", mkvOutputPath, "--quiet", "--no-subtitles", mkvInputPath
            }.ToList();

            if(Folder.SubfilesExist(episodeFolder, subtitleEngFull)) {
                mkvmergeArgs.AddRange(new[] {
                    "--language", "0:eng", "--track-name", "0:Full",
                    Directory.GetFiles(episodeFolder, subtitleEngFull + ".idx")[0],
                    Directory.GetFiles(episodeFolder, subtitleEngFull + ".sub")[0]
                });
            }

            if(Folder.SubfilesExist(episodeFolder, subtitleEngForced)) {
                mkvmergeArgs.AddRange(new[] {
                    "--language", "0:eng", "--track-name", "0:Forced",
                    Directory.GetFiles(episodeFolder, subtitleEngForced + ".idx")[0],
                    Directory.GetFiles(episodeFolder, subtitleEngForced + ".sub")[0]
                });
            }

            if(Folder.SubfilesExist(episodeFolder, subtitleGerFull)) {
                mkvmergeArgs.AddRange(new[] {
                    "--language", "0:deu", "--track-name", "0:Full",
                    Directory.GetFiles(episodeFolder, subtitleGerFull + ".idx")[0],
                    Directory.GetFiles(episodeFolder, subtitleGerFull + ".sub")[0]
                });
            }

            if(Folder.SubfilesExist(episodeFolder, subtitleGerForced)) {
                mkvmergeArgs.AddRange(new[] {
                    "--language", "0:deu", "--track-name", "0:Forced",
                    Directory.GetFiles(episodeFolder, subtitleGerForced + ".idx")[0],
                    Directory.GetFiles(episodeFolder, subtitleGerForced + ".sub")[0]
                });
            }

            if(File.Exists(mkvOutputPath)) { File.Delete(mkvOutputPath); }

            Run(mkvmergeArgs.ToArray());

            Folder.RenameFile(mkvOutputPath, mkvInputPath);
            Folder.DeleteSubtitleFiles(subtitlesIdx);
            Folder.DeleteSubtitleFiles(subtitlesSub);
        }
        catch(Exception e) {
            Console.WriteLine(e.ToString());
        }
    }

    public static void Run(string[] mkvmergeArgs)
    {
        ProcessStartInfo command = new()
        {
            FileName = "mkvmerge",          // Directly call mkvmerge
            Arguments = string.Join(" ", mkvmergeArgs), // Join arguments with spaces
            RedirectStandardOutput = true,  // Capture the output
            RedirectStandardError = true,   // Capture any errors
            UseShellExecute = false,        // Don't use a shell, run the command directly
            CreateNoWindow = true           // Run without creating a window
        };

        using Process process = Process.Start(command);
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (!string.IsNullOrEmpty(error)) {
            Console.WriteLine($"Error: {error}");
        }
        
    }
}