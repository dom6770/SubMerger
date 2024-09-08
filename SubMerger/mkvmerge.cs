using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

class Subtitles {
    public string GermanFull { get; set; }
    public string GermanForced { get; set; }
    public string EnglishFull { get; set; }
    public string EnglishForced { get; set; }
}

class mkvmerge {
    public static void Initialize(string episodeFolder) {
        try {
            string mkvInputPath = Directory.GetFiles(episodeFolder, "*.mkv").First();
            string mkvInputName = Path.GetFileNameWithoutExtension(mkvInputPath);
            string mkvOutputPath = mkvInputPath.Remove(mkvInputPath.Length - 4, 4) + "_new.mkv";

            string[] subtitlesIdx = Directory.GetFiles(episodeFolder, "*.idx");
            string[] subtitlesSub = Directory.GetFiles(episodeFolder, "*.sub");

            var subtitles = new Subtitles {
                GermanFull = subtitlesIdx.FirstOrDefault(s => s.EndsWith(".idx") && !s.Contains("forced") && !s.Contains("eng")),
                GermanForced = subtitlesIdx.FirstOrDefault(s => s.Contains(".forced.idx") && !s.Contains("eng")),
                EnglishFull = subtitlesIdx.FirstOrDefault(s => s.Contains(".eng.idx") && !s.Contains("forced")),
                EnglishForced = subtitlesIdx.FirstOrDefault(s => s.Contains(".eng.forced.idx"))
            };


            Console.WriteLine("\n┌── Mapped Subtitles");
            Console.WriteLine("├── German Full: " + subtitles.GermanFull);
            Console.WriteLine("├── German Forced: " + subtitles.GermanForced);
            Console.WriteLine("├── English Full: " + subtitles.EnglishFull);
            Console.WriteLine("└── English Forced: " + subtitles.EnglishForced + "\n");

            // Console.WriteLine("Press any key to continue...");
            // Console.ReadKey();

            var mkvmergeArgs = new[]{
                "--output", mkvOutputPath, "--quiet", "--no-subtitles", mkvInputPath
            }.ToList();

            if(File.Exists(subtitles.EnglishFull)) {
                mkvmergeArgs.AddRange(new[] { "--language", "0:eng", "--track-name", "0:Full", subtitles.EnglishFull });
            }

            if(File.Exists(subtitles.EnglishForced)) {
                mkvmergeArgs.AddRange(new[] { "--language", "0:eng", "--track-name", "0:Forced", subtitles.EnglishForced });
            }

            if(File.Exists(subtitles.GermanFull)) {
                mkvmergeArgs.AddRange(new[] { "--language", "0:deu", "--track-name", "0:Full", subtitles.GermanFull });
            }

            if(File.Exists(subtitles.GermanForced)) {
                mkvmergeArgs.AddRange(new[] { "--language", "0:deu", "--track-name", "0:Forced", subtitles.GermanForced });
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

    public static void Run(string[] mkvmergeArgs) {
        ProcessStartInfo command = new() {
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