using AzureLetsEncrypt.Configuration;
using AzureLetsEncrypt.Engine;
using System;
using System.Diagnostics;
using System.IO;

namespace AzureLetsEncrypt
{
    public class Program
    {
        static void Main(string[] args)
        {
            var watcher = new Stopwatch();

            var appSettings = new AppSettings();
            var encrypted = new LetsEncrypt(appSettings).Start();

            if (!encrypted)
                Shell.WriteError("GENERATIION FAILED.");

            Console.WriteLine($"Finished in {watcher.ElapsedMilliseconds} ms.");

            if (Debugger.IsAttached)
                Console.ReadKey();
        }
    }
}
