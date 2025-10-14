namespace DummyWavMaker;

class Program
{
    static void Main(string[] args)
    {
        ArgumentsValidation.ValidateArguments(args);

        string destinationPath = args[0];
        int numberOfFiles = int.Parse(args[1]);

        WavFileGenerator.GenerateWavFiles(destinationPath, numberOfFiles);
    }
}
