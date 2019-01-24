using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;

namespace AzureLetsEncrypt.Configuration
{
    public class AppSettings
    {
        public AppSettings()
        {
            Load();
        }

        public Azure Azure { get; set; }
        public Certificate Certificate { get; set; }

        private void Load()
        {
            string environment = Environment.GetEnvironmentVariable("Environment");

            var builder = new ConfigurationBuilder()
                               .SetBasePath(Directory.GetCurrentDirectory())
                               .AddJsonFile($"appsettings.json")                                  // Production values
                               .AddJsonFile($"appsettings.{environment}.json", optional: true)    // Set Environment variable in Properties / Debug
                               .AddEnvironmentVariables();

            var configuration = builder.Build();

            configuration.Bind(this);

            Certificate.Keys.Private =                      ReplaceFieldsByValues(Certificate.Keys.Private);
            Certificate.Keys.Identifier =                   ReplaceFieldsByValues(Certificate.Keys.Identifier);
            Certificate.Keys.Signing =                      ReplaceFieldsByValues(Certificate.Keys.Signing);
            Certificate.Keys.Certificate =                  ReplaceFieldsByValues(Certificate.Keys.Certificate);
            Certificate.Keys.Pfx =                          ReplaceFieldsByValues(Certificate.Keys.Pfx);
            Certificate.Keys.Password =                     ReplaceFieldsByValues(Certificate.Keys.Password);
            Certificate.Commands.CreatePrivateKey =         ReplaceFieldsByValues(Certificate.Commands.CreatePrivateKey);
            Certificate.Commands.CreateLetsEncryptKey =     ReplaceFieldsByValues(Certificate.Commands.CreateLetsEncryptKey);
            Certificate.Commands.CreateCertificateRequest = ReplaceFieldsByValues(Certificate.Commands.CreateCertificateRequest);
            Certificate.Commands.ConvertToPfx =             ReplaceFieldsByValues(Certificate.Commands.ConvertToPfx);
        }

        private string ReplaceFieldsByValues(string value)
        {
            value = value.Replace("{folders.wwwroot}", Certificate.Folders.WwwRoot)
                         .Replace("{folders.store}", Certificate.Folders.Store)
                         .Replace("{keys.private}", Certificate.Keys.Private)
                         .Replace("{keys.identifier}", Certificate.Keys.Identifier)
                         .Replace("{keys.signing}", Certificate.Keys.Signing)
                         .Replace("{keys.certificate}", Certificate.Keys.Certificate)
                         .Replace("{keys.pfx}", Certificate.Keys.Pfx)
                         .Replace("{keys.password}", Certificate.Keys.Password)                         
                         .Replace("{domains.0}", Certificate.Domains.ElementAtOrDefault(0)?.Replace('.', '-'))
                         .Replace("{domains.1}", Certificate.Domains.ElementAtOrDefault(1)?.Replace('.', '-'))
                         .Replace("{domains.2}", Certificate.Domains.ElementAtOrDefault(2)?.Replace('.', '-'))
                         .Replace("{domains.3}", Certificate.Domains.ElementAtOrDefault(3)?.Replace('.', '-'))
                         .Replace("{domains}", String.Join(',', Certificate.Domains))
                         .Replace("{password}", Certificate.Password);
            return value;
        }
    }

    #region SUB-CLASSES

    public class Azure
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TenantId { get; set; }
    }

    public class Certificate
    {
        public string Password { get; set; }
        public string[] Domains { get; set; }
        public Keys Keys { get; set; }
        public Commands Commands { get; set; }
        public Folders Folders { get; set; }
    }

    public class Keys
    {
        public string Private { get; set; }
        public string Identifier { get; set; }
        public string Signing { get; set; }
        public string Certificate { get; set; }
        public string Pfx { get; set; }
        public string Password { get; set; }
    }

    public class Commands
    {
        public string CreatePrivateKey { get; set; }
        public string CreateLetsEncryptKey { get; set; }
        public string CreateCertificateRequest { get; set; }
        public string ConvertToPfx { get; set; }
    }

    public class Folders
    {
        public string WwwRoot { get; set; }
        public string Store { get; set; }
    }

    #endregion
}
