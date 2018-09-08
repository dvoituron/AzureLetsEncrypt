using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AzureLetsEncrypt.Commands
{
    public class CreateSsl
    {
        public void Interactive()
        {
            var openssl = new Tools.OpenSsl();

            Tools.Shell.WriteTitle("Generating keys...");

            Tools.Shell.WriteTitle($"> Check if OpenSSL is available from {Directory.GetCurrentDirectory()}.");
            if (openssl.IsCommandAvailable() == false)
            {
                Tools.Shell.WriteQuestion("  Please, download OpenSsl.exe from https://openssl.org or use a embeded version (v1.0.2d).");
                var useEmbedded = Tools.Shell.WriteQuestionAndWait("  [S] Stop or [U] Use an embeded OpenSSL.exe ?", new[] { "S", "U" });
                if (useEmbedded == "S") return;
                
            }
                
            Tools.Shell.WriteTitle("> Create a private key to encrypt your data.");
            if (openssl.GeneratePrivateKey() == false) return;
            
            Tools.Shell.WriteTitle("> Create a Let’s Encrypt account identification key.");
            

            Tools.Shell.WriteTitle("> Update the Web.Config and add the staticContent/mimeMap.");
            
        }
    }
}
