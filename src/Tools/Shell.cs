using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AzureLetsEncrypt.Tools
{
    public class Shell
    {
        public static (string Output, string Error) Execute(string command, string args, DisplayConsole display)
        {
            string output = string.Empty;
            string error = string.Empty;

            if ((display & DisplayConsole.Command) != 0)
            {
                WriteCommand(command, args);
            }

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                    FileName = command,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    ErrorDialog = false,
                }
            };

            try
            {
                process.Start();
                output = process.StandardOutput.ReadToEnd();
                error = process.StandardOutput.ReadToEnd();
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                error = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);
            }

            if ((display & DisplayConsole.Output) != 0)
            {
                WriteOutput(output);
            }

            if ((display & DisplayConsole.Error) != 0)
            {
                WriteError(error);
            }

            return (output, error);
        }

        public static bool RunSuccessfully((string Output, string Error) console)
        {
            return String.IsNullOrEmpty(console.Error);
        }

        public static void WriteTitle(string text)
        {
            if (String.IsNullOrEmpty(text.Trim())) return;

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void WriteQuestion(string text)
        {
            if (String.IsNullOrEmpty(text.Trim())) return;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static string WriteQuestionAndWait(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{text} ");
            Console.ResetColor();
            return Console.ReadLine();
        }

        public static string WriteQuestionAndWait(string text, string[] options)
        {
            string solution = string.Empty;
            do
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"{text} ");
                Console.ResetColor();
                solution = Console.ReadLine().ToUpper();
            } while (!options.Contains(solution));
            return solution;
        }

        public static void WriteCommand(string command, string args)
        {
            if (String.IsNullOrEmpty(command.Trim())) return;

            Console.WriteLine($"[Command] {command} {args}");
        }

        public static void WriteOutput(string output)
        {
            if (String.IsNullOrEmpty(output.Trim())) return;

            Console.WriteLine(output);
        }

        public static void WriteError(string error)
        {
            if (String.IsNullOrEmpty(error.Trim())) return;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ResetColor();
        }

        [Flags]
        public enum DisplayConsole
        {
            None = 0,
            Output = 1,
            Error = 2,
            Command = 4
        }
    }
}
