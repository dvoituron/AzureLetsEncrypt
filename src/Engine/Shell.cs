using System;
using System.Diagnostics;
using System.IO;

namespace AzureLetsEncrypt.Engine
{
    public class Shell
    {
        public static string LogFilename => Path.Combine(Directory.GetCurrentDirectory(), $"{DateTime.Today:yyMMdd}-AzureLetsEncrypt.log");

        public static RedirectedConsole Execute(string commandAndArgs)
        {
            commandAndArgs = commandAndArgs.Trim();
            int endOfCommand = commandAndArgs.IndexOf(' ');

            if (endOfCommand > 0)
            {
                return Execute(commandAndArgs.Substring(0, endOfCommand), 
                               commandAndArgs.Substring(endOfCommand + 1));
            }
            else
            {
                return Execute(commandAndArgs, null);
            }
        }

        public static RedirectedConsole Execute(string command, string args)
        {
            using (var process = new Process())
            {
                var console = new RedirectedConsole()
                {
                    Command = $"{command} {args}"
                };

                WriteCommandLog(console.Command);

                process.StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                    FileName = command,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    ErrorDialog = false,
                };

                process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
                {
                    console.Output += e.Data;
                    console.Output += ((Process)sender).StandardError.ReadToEnd();
                });

                process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
                {
                    console.Output += e.Data;
                    console.Output += ((Process)sender).StandardError.ReadToEnd();
                });

                try
                {
                    process.Start();
                    process.BeginOutputReadLine();
                    process.WaitForExit();
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    console.Output += ex.Message + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                }
                
                WriteTraceLog(console.Output);

                return console;
            };

        }

        public static void WriteCommandLog(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"$> {message}");
            Console.ResetColor();
            File.AppendAllText(LogFilename, $"{DateTime.Now.ToString("HH:mm:ss")} - {message}{Environment.NewLine}");
        }

        public static void WriteTraceLog(string message)
        {
            const string MARGIN = "   ";

            message = MARGIN + message;
            message = message.Replace("\n", "\n" + MARGIN).Replace("\r", string.Empty);
            Console.WriteLine(message);
            File.AppendAllText(LogFilename, $"{DateTime.Now.ToString("HH:mm:ss")} - {message}{Environment.NewLine}");
        }

        public static void WriteConfirmation(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{message}");
            Console.ResetColor();
            File.AppendAllText(LogFilename, $"{DateTime.Now.ToString("HH:mm:ss")} - {message}{Environment.NewLine}");
        }

        public static void WriteError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR: {message}");
            Console.ResetColor();
            File.AppendAllText(LogFilename, $"{DateTime.Now.ToString("HH:mm:ss")} - ERROR: {message}{Environment.NewLine}");
        }

        public class RedirectedConsole
        {
            public string Command { get; set; }
            public string Output { get; set; }
        }
    }
}
