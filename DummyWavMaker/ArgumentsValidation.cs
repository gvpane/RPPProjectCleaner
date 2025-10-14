using System;
using System.IO;

namespace DummyWavMaker;

public class ArgumentsValidation
{
    public static void ValidateArguments(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Two arguments are required.");
            Console.WriteLine("Destination path and number of wav files to generate.");
            Environment.Exit(1);
        }

        if (string.IsNullOrWhiteSpace(args[0]))
        {
            Console.WriteLine("The first argument must be a valid directory path.");
            Environment.Exit(1);
        }

        if (!Directory.Exists(args[0]))
        {
            Console.WriteLine("The specified directory does not exist.");
            Environment.Exit(1);
        }

        if (!int.TryParse(args[1], out int numFiles) || numFiles < 1 && numFiles > 9999)
        {
            Console.WriteLine("The second argument must be a number greater than 0 and less than or equal to 9999.");
            Environment.Exit(1);
        }
    }
}
