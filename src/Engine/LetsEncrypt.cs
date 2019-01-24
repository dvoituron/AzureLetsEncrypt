using System;
using System.IO;

namespace AzureLetsEncrypt.Engine
{
    public class LetsEncrypt
    {
        private Configuration.AppSettings AppSettings;

        public LetsEncrypt(Configuration.AppSettings appSettings)
        {
            AppSettings = appSettings;
        }

        public bool Start()
        {
            var console = new Shell.RedirectedConsole();
            var ok = false;

            // Create a ./Store folder
            Shell.WriteCommandLog($"Creation of '{AppSettings.Certificate.Folders.Store}' folder");
            PrepareStoreFolder();

            // openssl genrsa -out {keys.private}
            console = Shell.Execute(AppSettings.Certificate.Commands.CreatePrivateKey);
            ok = console.Output.Contains("Generating RSA private key");
            if (!ok) return false;

            // openssl genrsa -out {keys.identifier}
            console = Shell.Execute(AppSettings.Certificate.Commands.CreateLetsEncryptKey);
            ok = console.Output.Contains("Generating RSA private key");
            if (!ok) return false;

            // Create the .well-known\acme-challenge folder
            Shell.WriteCommandLog($"Creation of '{AppSettings.Certificate.Folders.WwwRoot}\\.well-known\\acme-challenge' folder");
            var wellknownMustBeDeleted = PrepareWellknownFolder();

            // le64
            console = Shell.Execute(AppSettings.Certificate.Commands.CreateCertificateRequest);
            ok = console.Output.Contains("enjoy your certificate!");

            // Delete the .well-know folder (if not existing before the process)
            if (wellknownMustBeDeleted)
            {
                Shell.WriteCommandLog($"Remove the '{AppSettings.Certificate.Folders.WwwRoot}\\.well-known\\acme-challenge' folder");
                RemoveWellknownFolder();
            }

            if (!ok) return false;

            // openssl pkcs12 - export
            console = Shell.Execute(AppSettings.Certificate.Commands.ConvertToPfx);
            ok = console.Output.Contains("state - done");
            if (!ok) return false;

            return true;
        }

        /// <summary>
        /// Creates the .\Store folder to write certificates
        /// </summary>
        private void PrepareStoreFolder()
        {
            string storeFolder = Path.Combine(Directory.GetCurrentDirectory(), AppSettings.Certificate.Folders.Store);

            if (!Directory.Exists(storeFolder))
                Directory.CreateDirectory(storeFolder);
        }

        /// <summary>
        /// Creates the {keys.wwwroot}\.well-known\acme-challenge folder And write a web.config file for permissions
        /// Returns True if this folder must be deleted after generation (if this folder doesn't exists)
        /// </summary>
        private bool PrepareWellknownFolder()
        {
            const string WEBCONFIG_CONTENT = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<configuration>
  <system.webServer>
	<staticContent>
      <clear />
      <mimeMap fileExtension=""."" mimeType=""text/xml"" />
    </staticContent>
  </system.webServer>
</configuration>
";
            string wellKnownFolder = Path.Combine(AppSettings.Certificate.Folders.WwwRoot, ".well-known");
            string acmeChallengeFolder = Path.Combine(AppSettings.Certificate.Folders.WwwRoot, ".well-known\\acme-challenge");
            string webConfigFilename = Path.Combine(acmeChallengeFolder, "Web.Config");

            bool mustBeDeletedLater = !Directory.Exists(wellKnownFolder);

            if (!Directory.Exists(wellKnownFolder))
                Directory.CreateDirectory(wellKnownFolder);

            if (!Directory.Exists(acmeChallengeFolder))
                Directory.CreateDirectory(acmeChallengeFolder);

            if (!File.Exists(webConfigFilename))
                File.WriteAllText(webConfigFilename, WEBCONFIG_CONTENT);

            return mustBeDeletedLater;
        }

        /// <summary>
        /// Remove the {keys.wwwroot}\.well-known\acme-challenge folder
        /// </summary>
        private void RemoveWellknownFolder()
        {
            string wellKnownFolder = Path.Combine(AppSettings.Certificate.Folders.WwwRoot, ".well-known");

            if (Directory.Exists(wellKnownFolder))
                Directory.Delete(wellKnownFolder, recursive: true);
        }
    }
}
