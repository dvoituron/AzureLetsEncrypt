using System;
using System.IO;
using System.Reflection;

namespace AzureLetsEncrypt.Tools
{
    public class OpenSsl
    {
        public string PrivateKey => "mydomain.key";
        public string AccountKey => "account.key";

        public bool IsCommandAvailable()
        {
            var console = Shell.Execute("openssl", "version", runSuccessfullyMessage: "OpenSSL", display: Shell.DisplayConsole.None);

            return Shell.RunSuccessfully(console);
        }

        public void ExtractEmbededOpenSsl()
        {
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("AzureLetsEncrypt.Tools.openssl.exe"))
            {
                using (var file = new FileStream("openssl.exe", FileMode.Create, FileAccess.Write))
                {
                    resource.CopyTo(file);
                }
            }
        }

        public bool GeneratePrivateKey()
        {
            if (File.Exists(PrivateKey)) File.Delete(PrivateKey);

            var console = Shell.Execute("openssl", $"genrsa -out \"{PrivateKey}\" 2048", runSuccessfullyMessage: "Generating RSA private key");

            return Shell.RunSuccessfully(console);
        }

        public bool GenerateAccountKey()
        {
            if (File.Exists(AccountKey)) File.Delete(AccountKey);

            var console = Shell.Execute("openssl", $"genrsa -out \"{AccountKey}\" 4096", runSuccessfullyMessage: "Generating RSA private key");

            return Shell.RunSuccessfully(console, "Generating RSA private key");
        }
    }
}
