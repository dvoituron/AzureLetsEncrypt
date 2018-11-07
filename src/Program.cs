using AzureLetsEncrypt.Configuration;
using AzureLetsEncrypt.Engine;
using System;
using System.Diagnostics;

namespace AzureLetsEncrypt
{
    public class Program
    {
        static void Main(string[] args)
        {
            var watcher = new Stopwatch();

            var appSettings = new AppSettings();
            var encrypt = new LetsEncrypt(appSettings);

            encrypt.Start();

            Console.WriteLine($"Finished in {watcher.ElapsedMilliseconds} ms.");

            if (System.Diagnostics.Debugger.IsAttached)
                Console.ReadKey();
        }
    }
}
