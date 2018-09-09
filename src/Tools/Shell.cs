﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AzureLetsEncrypt.Tools
{
    public class Shell
    {
        public static string LogFilename => Path.Combine(Directory.GetCurrentDirectory(), $"{DateTime.Today:yyMMdd}-AzureLetsEncrypt.log");

        public static DisplayConsole Display { get; set; } = Shell.DisplayConsole.Command | Shell.DisplayConsole.Output | Shell.DisplayConsole.Error;

        public static (string Output, string Error) Execute(string command, string args, string runSuccessfullyMessage)
        {
            return Execute(command, args, runSuccessfullyMessage, Display);
        }

        public static (string Output, string Error) Execute(string command, string args, string runSuccessfullyMessage, DisplayConsole display)
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
                process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
                {
                    output += e.Data;
                    error += ((Process)sender).StandardError.ReadToEnd();
                });

                process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
                {
                    error += ((Process)sender).StandardError.ReadToEnd();
                });

                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                error = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);
            }

            if (!String.IsNullOrEmpty(runSuccessfullyMessage) && error.Contains(runSuccessfullyMessage))
            {
                output += error;
                error = string.Empty;
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

        public static bool RunSuccessfully((string Output, string Error) console, string valueToCheck)
        {
            return console.Output.Contains(valueToCheck) || console.Error.Contains(valueToCheck);
        }

        public static void WriteTitle(string text)
        {
            if (String.IsNullOrEmpty(text.Trim())) return;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(text);
            Console.ResetColor();

            Log($"{text}");
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

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[Command] {command} {args}");
            Console.ResetColor();

            Log($"[Command] {command} {args}");
        }

        public static void WriteOutput(string output)
        {
            if (String.IsNullOrEmpty(output.Trim())) return;

            Console.WriteLine(output);

            Log(output);
        }

        public static void WriteError(string error)
        {
            if (String.IsNullOrEmpty(error.Trim())) return;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ResetColor();

            Log(error);
        }

        public static void Log(string message)
        {
            File.AppendAllText(LogFilename, Environment.NewLine + message);
        }

        [Flags]
        public enum DisplayConsole
        {
            None = 0,
            Output = 1,
            Error = 2,
            Command = 4,
        }
    }
}
