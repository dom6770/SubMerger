using System;
class Program {
    private static int Main(string[] args) {
        SubMerger subMerger = new();
        subMerger.inputName = args[0];
        subMerger.inputPath = args.Length > 1 ? args[1] : DateTime.Now.ToString("dd-MM_HH-mm");

        return subMerger.Run(args);
    }
}