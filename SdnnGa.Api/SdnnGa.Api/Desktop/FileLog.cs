using System;
using System.IO;
using System.Text;

namespace SdnnGa.Api.Desktop;

/// <summary>
/// Дублює весь вивід <see cref="Console"/> у файл логу. У Release-збірці застосунок не має консолі
/// (WinExe), тому без цього логи <c>Console.WriteLine</c> губилися б.
/// </summary>
public static class FileLog
{
    public static string CurrentLogFile { get; private set; }

    public static void Initialize(string logDirectory)
    {
        Directory.CreateDirectory(logDirectory);
        CurrentLogFile = Path.Combine(logDirectory, $"sdnnga-{DateTime.Now:yyyyMMdd}.log");

        var fileWriter = new StreamWriter(
            new FileStream(CurrentLogFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite),
            new UTF8Encoding(false))
        {
            AutoFlush = true
        };

        Console.SetOut(TextWriter.Synchronized(new TeeTextWriter(Console.Out, fileWriter)));
        Console.SetError(TextWriter.Synchronized(new TeeTextWriter(Console.Error, fileWriter)));

        Console.WriteLine($"===== SDNN-GA started {DateTime.Now:yyyy-MM-dd HH:mm:ss} =====");
    }

    private sealed class TeeTextWriter : TextWriter
    {
        private readonly TextWriter _console;
        private readonly TextWriter _file;

        public TeeTextWriter(TextWriter console, TextWriter file)
        {
            _console = console;
            _file = file;
        }

        public override Encoding Encoding => _file.Encoding;

        public override void Write(char value)
        {
            Safe(() => _console.Write(value));
            Safe(() => _file.Write(value));
        }

        public override void Write(string value)
        {
            Safe(() => _console.Write(value));
            Safe(() => _file.Write(value));
        }

        public override void WriteLine(string value)
        {
            Safe(() => _console.WriteLine(value));
            Safe(() => _file.WriteLine(value));
        }

        public override void Flush()
        {
            Safe(_console.Flush);
            Safe(_file.Flush);
        }

        // Логування ніколи не повинно ламати застосунок (наприклад, якщо консолі немає).
        private static void Safe(Action action)
        {
            try
            {
                action();
            }
            catch (IOException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }
}
