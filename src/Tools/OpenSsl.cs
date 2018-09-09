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
            var console = Shell.Execute("openssl", "version", Shell.DisplayConsole.None);

            return console.Output.Contains("OpenSSL") && Shell.RunSuccessfully(console);
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
            var console = Shell.Execute("openssl", $"genrsa -out \"{PrivateKey}\" 2048");

            return Shell.RunSuccessfully(console);
        }

        public bool GenerateAccountKey()
        {
            var console = Shell.Execute("openssl", $"genrsa -out \"{AccountKey}\" 4096");

            return Shell.RunSuccessfully(console);
        }
    }
}
