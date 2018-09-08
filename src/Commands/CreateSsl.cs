using AzureLetsEncrypt.Configuration;
using AzureLetsEncrypt.Tools;
using System;
using System.IO;
using System.Xml.Linq;

namespace AzureLetsEncrypt.Commands
{
    public class CreateSsl
    {
        public void Interactive()
        {
            var openssl = new OpenSsl();
            var letsEncrypt = new LetsEncrypt();
            bool isCommandAvailable = false;

            Shell.WriteTitle("Generating keys...");
            Shell.WriteTitle("------------------");
            Shell.WriteTitle("Current folder is " + Directory.GetCurrentDirectory());
            // OpenSSL is available ?
            do
            {
                Shell.WriteTitle($"> Check if OpenSSL is available.");
                isCommandAvailable = openssl.IsCommandAvailable();

                if (!isCommandAvailable)
                {
                    Shell.WriteQuestion("  Please, download OpenSsl.exe from https://openssl.org or use an embeded version (v1.0.2d).");
                    string key = Shell.WriteQuestionAndWait("  [S] Stop, [R] Retry or [X] Extract an embeded OpenSSL.exe ?", 
                                                            new[] { "S", "R", "X" });

                    if (key == "S") return;
                    if (key == "X") openssl.ExtractEmbededOpenSsl();
                }

            } while (isCommandAvailable == false);

            // reate a private key: mydomain.pfx
            Shell.WriteTitle($"> Create a private key to encrypt your data: {openssl.PrivateKey}.");
            if (openssl.GeneratePrivateKey() == false) return;

            // Create a Let’s Encrypt account
            Shell.WriteTitle($"> Create a Let’s Encrypt account identification key: {openssl.AccountKey}");
            if (openssl.GenerateAccountKey() == false) return;

            // Ask where the Web.Config is located
            Shell.WriteTitle($"> Check the Web.Config (<mimeMap fileExtension=\".\" mimeType=\"text/xml\" />).");
            string webConfigPath = Directory.GetCurrentDirectory();
            if (File.Exists(Path.Combine(webConfigPath, "Web.Config")) == false)
            {
                Shell.WriteQuestion("  By default, Azure does not recognize files without extension.");
                Shell.WriteQuestion("  Sets the path where your web.config sould be located,");
                Shell.WriteQuestion("  to add <mimeMap fileExtension=\".\" mimeType=\"text/xml\" />.");
                do
                {
                    webConfigPath = Shell.WriteQuestionAndWait($"  Web.Config folder ([Enter] is the current folder)?");
                    if (String.IsNullOrEmpty(webConfigPath)) webConfigPath = Directory.GetCurrentDirectory();
                } while (Directory.Exists(webConfigPath) == false);
            }
            string fileWebConfig = Path.Combine(webConfigPath, "Web.Config");

            // Check if the web.config exists
            if (File.Exists(fileWebConfig) == false)
            {
                var key = Shell.WriteQuestionAndWait($"  The Web.Config doesn't exist in this folder. Create ? [Y] Yes / [S] Stop.", new[] { "Y", "S" });
                if (key == "Y") letsEncrypt.CreateNewWebConfig(fileWebConfig);
                if (key == "S") return;
            }

            // Check if the section staticContent/mimeMap exists.
            if (letsEncrypt.CheckSectionMimeMap(fileWebConfig) == false)
            {
                var key = Shell.WriteQuestionAndWait($"  The Web.Config doesn't contains this section <mimeMap />. Create ? [Y] Yes / [S] Stop.", new[] { "Y", "S" });
                if (key == "Y") letsEncrypt.AddSectionMimeMap(fileWebConfig);
                if (key == "S") return;
            }
        }
    }
}
