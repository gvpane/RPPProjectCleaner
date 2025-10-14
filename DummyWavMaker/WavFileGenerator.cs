using System;
using System.IO;

namespace DummyWavMaker;

public class WavFileGenerator
{
    private const int SampleRate = 44100;  // Standard CD quality sample rate
    private const int DurationSeconds = 10;
    private const int BitsPerSample = 16;
    private const int Channels = 1;  // Mono

    public static void GenerateWavFiles(string destinationPath, int numberOfFiles)
    {
        for (int i = 1; i <= numberOfFiles; i++)
        {
            string filePath = Path.Combine(destinationPath, $"Dummy_{GenerateRandomNumber()}.wav");
            GenerateWavFile(filePath);
            Console.WriteLine($"Generated: {filePath}");
        }
    }

    private static void GenerateWavFile(string filePath)
    {
        int totalSamples = SampleRate * DurationSeconds;
        int bytesPerSample = BitsPerSample / 8;
        int dataSize = totalSamples * bytesPerSample * Channels;
        int fileSize = 36 + dataSize;  // 36 is header size minus 8

        using (var fs = new FileStream(filePath, FileMode.Create))
        using (var writer = new BinaryWriter(fs))
        {
            // Write WAV header
            writer.Write("RIFF".ToCharArray());                     // Chunk ID
            writer.Write(fileSize);                                 // Chunk Size
            writer.Write("WAVE".ToCharArray());                     // Format
            writer.Write("fmt ".ToCharArray());                     // Subchunk1 ID
            writer.Write(16);                                       // Subchunk1 Size (PCM)
            writer.Write((short)1);                                 // Audio Format (PCM)
            writer.Write((short)Channels);                          // Number of Channels
            writer.Write(SampleRate);                               // Sample Rate
            writer.Write(SampleRate * Channels * bytesPerSample);   // Byte Rate
            writer.Write((short)(Channels * bytesPerSample));       // Block Align
            writer.Write((short)BitsPerSample);                     // Bits Per Sample
            writer.Write("data".ToCharArray());                     // Subchunk2 ID
            writer.Write(dataSize);                                 // Subchunk2 Size

            // Generate and write white noise data
            Random random = new Random();
            for (int i = 0; i < totalSamples; i++)
            {
                // Generate random 16-bit sample (-32768 to 32767)
                short sample = (short)(random.Next(-32768, 32767));
                writer.Write(sample);
            }
        }
    }

    private static int GenerateRandomNumber()
    {
        Random random = new Random();
        return random.Next(1, 9999);
    }
}