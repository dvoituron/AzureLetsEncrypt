using AzureLetsEncrypt.Helpers;
using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;

namespace AzureLetsEncrypt.Tools
{
    public class LetsEncrypt
    {
        public string PrivateKey => "mydomain.key";
        public string CertificateSigningRequestKey => "mydomain.csr";
        public string DomainCertificateKey => "mydomain.crt";
        public string AccountKey => "account.key";

        public void CreateNewWebConfig(string fileWebConfig)
        {
            string defaultWebConfig = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<configuration>
  <system.webServer>
	<staticContent>
      <mimeMap fileExtension=""."" mimeType=""text/xml"" />
    </staticContent>
  </system.webServer>
</configuration>
";

            System.IO.File.WriteAllText(fileWebConfig, defaultWebConfig);
        }

        public bool CheckSectionMimeMap(string fileWebConfig)
        {
            var webConfig = XDocument.Load(fileWebConfig);
            var mimeMap = webConfig.XPathSelectElement("/configuration/system.webServer/staticContent/mimeMap[@fileExtension=\".\"]");
            if (mimeMap?.Attribute("mimeType") != null) return true;
            return false;
        }

        public void AddSectionMimeMap(string fileWebConfig)
        {
            var webConfig = XDocument.Load(fileWebConfig);
            var staticContent = webConfig.GetOrAdd("configuration")
                                         .GetOrAdd("system.webServer")
                                         .GetOrAdd("staticContent");            
            var mimeMap = new XElement("mimeMap");
            mimeMap.Add(new XAttribute("fileExtension", "."));
            mimeMap.Add(new XAttribute("mimeType", "text/xml"));

            staticContent.Add(mimeMap);

            webConfig.Save(fileWebConfig);
        }

        public bool IsCommandAvailable()
        {
            var console = Shell.Execute("le64", string.Empty, Shell.DisplayConsole.None);

            return console.Output.Contains("ZeroSSL Crypt::LE client") && Shell.RunSuccessfully(console);
        }

        public void ExtractEmbededLE64()
        {
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("AzureLetsEncrypt.Tools.le64.exe"))
            {
                using (var file = new FileStream("le64.exe", FileMode.Create, FileAccess.Write))
                {
                    resource.CopyTo(file);
                }
            }
        }

        public bool ValidateCRT(string[] domains, string path)
        {
            string arguments = $" --key {AccountKey}" +                             // Account key file
                               $" --csr {CertificateSigningRequestKey}" +           // Certificate Signing Request file
                               $" --csr-key {PrivateKey}" +                         // Key for Certificate Signing Request
                               $" --crt {DomainCertificateKey}" +                   // Name for the domain certificate file
                               $" --domains \"{String.Join(',', domains)}\"" +      // Domains list (separator = ,)
                               $" --path \"{path}\"" +                              // Absolute path to .well-known/acme-challenge/
                               $" --generate-missing" +                             // Generate missing files (key, csr and csr-key)
                               $" --unlink" +                                       // Remove challenge files automatically
                               $" --live";                                          // Use the live server instead of the test one

            var console = Shell.Execute("le64", arguments);

            return Shell.RunSuccessfully(console);
        }

        public bool CreatePFX(string pfxName, string password)
        {
            var console = Shell.Execute("openssl", $"pkcs12 -export -out {pfxName} -inkey {PrivateKey} -in {DomainCertificateKey} -passout pass:{password}");

            return Shell.RunSuccessfully(console);
        }
    }
}
