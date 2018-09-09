using AzureLetsEncrypt.Configuration;
using AzureLetsEncrypt.Tools;
using System;
using System.IO;
using System.Linq;
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

            Shell.WriteTitle($"Generating keys... {DateTime.Now:HH:mm:ss}");
            Shell.WriteTitle($"---------------------------");
            Shell.WriteTitle($"Current folder is {Directory.GetCurrentDirectory()}");

            // OpenSSL is available ?
            do
            {
                Shell.WriteTitle($"> Check if OpenSSL is available.");
                isCommandAvailable = openssl.IsCommandAvailable();

                if (!isCommandAvailable)
                {
                    Shell.WriteQuestion("  Please, download OpenSsl.exe from https://openssl.org");
                    Shell.WriteQuestion("  or use an embeded version (v1.0.2d).");
                    string key = Shell.WriteQuestionAndWait("  [S] Stop, [R] Retry or [X] Extract an embeded OpenSSL.exe ?",
                                                            new[] { "S", "R", "X" });

                    if (key == "S") return;
                    if (key == "X") openssl.ExtractEmbededOpenSsl();
                }

            } while (isCommandAvailable == false);

            // Create a private key: mydomain.pfx
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

            // LE64 is available ?
            do
            {
                Shell.WriteTitle($"> Check if le64.exe (Let's Encrypt) is available.");
                isCommandAvailable = letsEncrypt.IsCommandAvailable();

                if (!isCommandAvailable)
                {
                    Shell.WriteQuestion("  Please, download le64.exe from https://github.com/do-know/Crypt-LE/releases ");
                    Shell.WriteQuestion("  or use an embeded version (v0.31).");
                    string key = Shell.WriteQuestionAndWait("  [S] Stop, [R] Retry or [X] Extract an embeded le64.exe ?",
                                                            new[] { "S", "R", "X" });

                    if (key == "S") return;
                    if (key == "X") letsEncrypt.ExtractEmbededLE64();
                }

            } while (isCommandAvailable == false);


            // Domains list
            string[] domains;
            Shell.WriteTitle($"> Sets the domains list to validate (domains separated by comma or space).");
            Shell.WriteTitle($"  Sample: mydomain.com,www.mydomain.com");
            do
            {
                domains = Shell.WriteQuestionAndWait("  Domains list?").Split(',', ';', ' ');
                domains = domains.Select(i => i.Trim().ToLower()).Where(i => i.Length > 0).ToArray();
            } while (domains.Length <= 0);

            // Ask the path of .well-known/acme-challenge       
            const string ACME_CHALLENGE_SUBPATH = ".well-known\\acme-challenge";
            string defaultPath = Path.Combine(Directory.GetCurrentDirectory(), ACME_CHALLENGE_SUBPATH);
            Shell.WriteTitle($"> Folder to generate Let's Encrypt files for verificationr.");
            string acmeChallengePath = Path.Combine(Directory.GetCurrentDirectory(), ACME_CHALLENGE_SUBPATH);

            Shell.WriteQuestion($"  Let's Encrypt will check a file in \"http://[mydomain.com]/.well-known/acme-challenge\" folder.");
            Shell.WriteQuestion($"  Sets this local folder path to generate files for verification.");
            Shell.WriteQuestion($"  [Enter] = {defaultPath}");
            do
            {
                acmeChallengePath = Shell.WriteQuestionAndWait($"  Your \"acme-challenge\" folder?");
                if (String.IsNullOrEmpty(acmeChallengePath)) acmeChallengePath = defaultPath;

                if (Directory.Exists(acmeChallengePath) == false)
                {
                    string key = Shell.WriteQuestionAndWait("  This folder doesn't exist. Create? [Y] Yes, [N] No.",
                                                        new[] { "Y", "N" });

                    if (key == "Y") Directory.CreateDirectory(acmeChallengePath);
                }

            } while (Directory.Exists(acmeChallengePath) == false);

            // Generates a validated certificate
            Shell.WriteTitle($"> Ask to LetsEncrypt to generates a validated certificate.");
            if (letsEncrypt.ValidateCRT(domains, acmeChallengePath) == false) return;

            // Generates a validated certificate
            Shell.WriteTitle($"> Export the certificate into a PFX file.");
            string password = Shell.WriteQuestionAndWait("  Write a password to secure the private key of the PFX:");

            if (letsEncrypt.CreatePFX("mydomain.pfx", password) == false) return;
        }
    }
}
