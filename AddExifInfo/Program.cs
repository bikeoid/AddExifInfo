using System;
using System.IO;
using CommandLine.Utility;


namespace AddExifInfo
{
    /// <summary>
    /// Program to add Exif info to 360 image files extracted from GoPro Max
    /// </summary>
    class Program
    {

        private static Arguments CommandLine;

        static void Main(string[] args)
        {
            try
            {
                RunProgram(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\n********\nProblem encountered: " + ex.ToString());
            }

        }



        private static void RunProgram(string[] args)
        {
            // Command line parsing
            CommandLine = new Arguments(args);

            var goProImages = CommandLine["GoProImages"];

            if (goProImages == null)
            {
                ShowHelp();
                return;
            }

            if (!Directory.Exists(goProImages))
            {
                Console.WriteLine($"GoPro Images Directory not found: {goProImages} ");
                ShowHelp();
                return;
            }

            var migrateSequence = new ProcessVideoSequence();

            migrateSequence.Migrate(goProImages);

        }




        private static void ShowHelp()
        {

            Console.WriteLine();
            Console.WriteLine("AddExifInfo Usage:");
            Console.WriteLine(@"  AddExifInfo /GoProImages=""SampledVideoImageSequences"" ");
            Console.WriteLine("  For example: ");
            Console.WriteLine(@"  AddExifInfo  /GoProImages="".\MPImportUpload"" ");

        }

    }
}
