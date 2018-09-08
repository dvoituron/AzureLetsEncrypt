using AzureLetsEncrypt.Helpers;
using System;
using System.Xml.Linq;
using System.Xml.XPath;

namespace AzureLetsEncrypt.Tools
{
    public class LetsEncrypt
    {
        public Shell.DisplayConsole DefaultTraces => Shell.DisplayConsole.Output | Shell.DisplayConsole.Error;

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

        public void ValidateCRT()
        {
            // To Execute...
            // le.exe --key account.key --csr mydomain.csr --csr-key mydomain.key --crt mydomain.crt --domains "www.dvoituron.com,dvoituron.com" --path D:\home\site\wwwroot\.well-known\acme-challenge\ --generate-missing --unlink --live
        }

        public void CreatePFX()
        {
            // To Execute...
            // openssl pkcs12 -export -out mydomain.pfx -inkey mydomain.key -in mydomain.crt -passout pass:MyPassword
        }
    }
}
