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

            // Read configuration parameters
            var appSettings = new AppSettings(); 

            // Generate a certificate validated by Let's Encrypt
            var encrypted = new LetsEncrypt(appSettings).Start();

            if (encrypted)
            {
                Shell.WriteConfirmation($"Certificate successfully exported to {appSettings.Certificate.Keys.Pfx}.");

                var azure = new AzureCertificate(appSettings).Connect();
            }
            else
                Shell.WriteError("GENERATIION FAILED.");

            Console.WriteLine($"Finished in {watcher.ElapsedMilliseconds} ms.");

            if (Debugger.IsAttached)
                Console.ReadKey();
        }
    }
}
