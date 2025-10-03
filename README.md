# CleanWavFiles

A C# console application to delete all .wav files in the same directory as a given .rpp file, except those referenced in the .rpp file.

## Usage

1. Build the project:
   ```sh
   dotnet build
   ```
2. Run the program:
   ```sh
   dotnet run --project CleanWavFiles.csproj <path-to-rpp-file>
   ```

This will delete all .wav files in the .rpp file's directory that are not referenced in the .rpp file.
