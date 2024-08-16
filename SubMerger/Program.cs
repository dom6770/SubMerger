using System.Diagnostics;
using System;

class Program {
    private static int Main(string[] args) {

        //subMerger.inputName = args[0];
        //subMerger.inputPath = args.Length > 1 ? args[1] : DateTime.Now.ToString("dd-MM_HH-mm");

        if(args.Length == 0) {
            Console.WriteLine("Error: No arguments provided. Please specify the input path as first, and optionally the movie name as second argument.");
            return 1;         
        }

        if(!IsCommandAvailable("mkvmerge")) {
            Console.WriteLine("Error: mkvmerge not found in PATH. Please install mkvtoolnix");
            return 2;
        } 

        SubMerger subMerger = new(
            args[0],
            args.Length > 1 ? args[1] : DateTime.Now.ToString("dd-MM_HH-mm")
        );

        return subMerger.Run();
    }
    private static bool IsCommandAvailable(string command) {
        ProcessStartInfo psi = new() {
            FileName = "which",
            Arguments = command,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using Process process = Process.Start(psi);
        process.WaitForExit();
        return process.ExitCode == 0;
    }
}