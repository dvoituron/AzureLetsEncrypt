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

            try
            {
                // Read configuration parameters
                var appSettings = new AppSettings();

                // Generate a certificate validated by Let's Encrypt
                var encrypted = new LetsEncrypt(appSettings).Start();

                if (encrypted)
                    Shell.WriteConfirmation($"Certificate successfully generated to {appSettings.Certificate.Keys.Pfx}.");
                else
                {
                    Shell.WriteError("GENERATIION FAILED.");
                }

                // Upload to Azure
                if (encrypted && !String.IsNullOrEmpty(appSettings.Azure?.ClientId))
                { 
                    var azure = new AzureCertificate(appSettings).Connect().Upload();
                    Shell.WriteConfirmation($"Certificate successfully uploaded to Azure.");
                }

                if (encrypted)
                    Shell.WriteConfirmation($"Download and REMOVE your certificates saved in {appSettings.Certificate.Folders.Store}.");
            }
            catch (Exception ex)
            {
                Shell.WriteError(ex.Message);
                Shell.WriteError(ex.InnerException?.Message);
                Shell.WriteError("GENERATIION FAILED.");
            }

            Console.WriteLine($"Finished in {watcher.ElapsedMilliseconds} ms.");

            if (Debugger.IsAttached)
                Console.ReadKey();
        }
    }
}
