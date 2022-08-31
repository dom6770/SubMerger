using System;
class Program {
    private static int Main(string[] args) {
        
        //subMerger.inputName = args[0];
        //subMerger.inputPath = args.Length > 1 ? args[1] : DateTime.Now.ToString("dd-MM_HH-mm");

        SubMerger subMerger = new(
            args[0],
            args.Length > 1 ? args[1] : DateTime.Now.ToString("dd-MM_HH-mm")
            );

        return subMerger.Run();
    }
}