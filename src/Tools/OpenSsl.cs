using System;
using System.Collections.Generic;
using System.Text;

namespace AzureLetsEncrypt.Tools
{
    public class OpenSsl
    {
        private Shell.DisplayConsole DisplayInConsole => Shell.DisplayConsole.Command | Shell.DisplayConsole.Output | Shell.DisplayConsole.Error;
        private string PrivateKey => "mydomain.key";
        
        public bool GeneratePrivateKey()
        {
            var console = Shell.Execute("openssl", $"genrsa -out \"{PrivateKey}\" 2048", DisplayInConsole);

            return Shell.RunSuccessfully(console);
        }

        public bool IsCommandAvailable()
        {
            var console = Shell.Execute("openssl", "version", Shell.DisplayConsole.None);

            return console.Output.Contains("OpenSSL") && Shell.RunSuccessfully(console);
        }

        public bool ExtractEmbededOpenSsl()
        {
            return true;
        }
    }
}
