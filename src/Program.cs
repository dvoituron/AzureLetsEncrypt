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

#if DEBUG
            // Tests
            //args = new[] { "--domains=mydomain.com,www.mydomain.com", "--password=My@Password" };
            //args = new[] { "--domains=mydomain.com,www.mydomain.com", "--password=My@Password", "--path=C:\\_Temp" };
            //args = new[] { "--help" };
#endif

            var watcher = new Stopwatch();

            try
            {

                // Read configuration parameters
                var appSettings = new AppSettings(args);

                // Help already displayed
                if (appSettings.AskHelp)
                    return;

                // Some flags are required
                else if (appSettings.Certificate.Domains.Length == 0 || String.IsNullOrEmpty(appSettings.Certificate.Password))
                    throw new ArgumentException("Sets corrects domains and password using appSettings.json or command line (--help).");

                // Generate a certificate validated by Let's Encrypt
                bool encrypted = false;

                if (String.IsNullOrEmpty(appSettings.Certificate.Commands.CreatePrivateKey) && System.IO.File.Exists(appSettings.Certificate.Keys.Pfx))
                    encrypted = true;
                else
                    encrypted = new LetsEncrypt(appSettings).Start();

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
                else
                    Environment.Exit(-1);

            }
            catch (Exception ex)
            {
                Shell.WriteError(ex.Message);
                Shell.WriteError(ex.InnerException?.Message);
                Shell.WriteError("GENERATIION FAILED.");
                Environment.Exit(-1);
            }

            Console.WriteLine($"Finished in {watcher.ElapsedMilliseconds} ms.");

            if (Debugger.IsAttached)
                Console.ReadKey();
        }
    }
}
