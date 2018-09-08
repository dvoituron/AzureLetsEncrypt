using Microsoft.Extensions.Configuration;
using System;
using System.IO;

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
        
        public void Load()
        {
            string environment = Environment.GetEnvironmentVariable("Environment");

            var builder = new ConfigurationBuilder()
                               .SetBasePath(Directory.GetCurrentDirectory())
                               .AddJsonFile($"appsettings.json")                                  // Production values
                               .AddJsonFile($"appsettings.{environment}.json", optional: true)    // Set Environment variable in Properties / Debug
                               .AddEnvironmentVariables();

            var configuration = builder.Build();

            configuration.Bind(this);
        }
    }

    public class Azure
    {
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
        public string TenantID { get; set; }
    }

    public class Certificate
    {
        public CertificateScript Script { get; set; }
        public CertificatePfx Pfx { get; set; }
        public CertificateAzure Azure { get; set; }
    }

    public class CertificateScript
    {
        public string[] Before { get; set; }
        public string[] After { get; set; }
    }

    public class CertificatePfx
    {
        public string Filename { get; set; }
        public string Password { get; set; }
    }

    public class CertificateAzure
    {
        public string WebAppResourceId { get; set; }
        public string ResourceGroup { get; set; }
        public string[] Hostnames { get; set; }
    }

}
