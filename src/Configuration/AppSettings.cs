using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

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
                               .AddJsonFile($"appsettings.json", optional: true)                           // Production values
                               .AddJsonFile($"appsettings.{environment}.json", optional: true)             // Set Environment variable in Properties / Debug
                               .AddJsonFile($"appsettings.{Environment.UserName}.json", optional: true)    // Development username
                               .AddEnvironmentVariables();

            var configuration = builder.Build();

            configuration.Bind(this);

            if (Certificate == null)
                SetDefaultConfiguration();

            Certificate.Keys.Private = ReplaceFieldsByValues(Certificate.Keys.Private);
            Certificate.Keys.Identifier = ReplaceFieldsByValues(Certificate.Keys.Identifier);
            Certificate.Keys.Signing = ReplaceFieldsByValues(Certificate.Keys.Signing);
            Certificate.Keys.Certificate = ReplaceFieldsByValues(Certificate.Keys.Certificate);
            Certificate.Keys.Pfx = ReplaceFieldsByValues(Certificate.Keys.Pfx);
            Certificate.Keys.Password = ReplaceFieldsByValues(Certificate.Keys.Password);
            Certificate.Commands.CreatePrivateKey = ReplaceFieldsByValues(Certificate.Commands.CreatePrivateKey);
            Certificate.Commands.CreateLetsEncryptKey = ReplaceFieldsByValues(Certificate.Commands.CreateLetsEncryptKey);
            Certificate.Commands.CreateCertificateRequest = ReplaceFieldsByValues(Certificate.Commands.CreateCertificateRequest);
            Certificate.Commands.ConvertToPfx = ReplaceFieldsByValues(Certificate.Commands.ConvertToPfx);
        }

        private string ReplaceFieldsByValues(string value)
        {
            if (String.IsNullOrEmpty(value)) return String.Empty;

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

        public void OverwriteWithCommandLine(string[] args)
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
            var isHelp = args.Contains("--help") || args.Contains("/help");
            var domains = args.FirstOrDefault(i => i.StartsWith("--domains="))?.Substring(10);
            var password = args.FirstOrDefault(i => i.StartsWith("--password="))?.Substring(11);

            Console.WriteLine($"AzureLetsEncrypt {version} - Twitter:@DenisVoituron");
            Console.WriteLine();

            if (isHelp || String.IsNullOrEmpty(domains) || string.IsNullOrEmpty(password))
            {
                Console.WriteLine("  Generate a new free Let's Encrypt certificate for specifis domains.");
                Console.WriteLine("  You must install it manually in Azure (go to your \"App Service / TLS/SSL settings\" section.");
                Console.WriteLine("  The generated certificate will be store in a \"./store\" folder.");
                Console.WriteLine();
                Console.WriteLine("AzureLetsEncrypt --domains=[List_of_domains] --password=[Password]");
                Console.WriteLine();
                Console.WriteLine("   --domains     List of domains to include in the certificate (no wildcard),");
                Console.WriteLine("                 separated by semicolons.");
                Console.WriteLine("   --password    Password used to encrypt the pfx certificate.");
                Console.WriteLine();
                Console.WriteLine("Example:");
                Console.WriteLine("   AzureLetsEncrypt --domains=dvoituron.com;www.dvoituron.com --password=MyP@ssword");
                return;
            }

            this.Certificate.Domains = domains.Split(';');
            this.Certificate.Password = password;
        }

        private void SetDefaultConfiguration()
        {
            string assemblyPath = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;

            Certificate = new Certificate();
            Certificate.Folders = new Folders();
            Certificate.Keys = new Keys();
            Certificate.Commands = new Commands();

            Certificate.Domains = new string[] { };
            Certificate.Password = String.Empty;
            Certificate.Folders.WwwRoot = Environment.CurrentDirectory;  // "D:/home/site/wwwroot";
            Certificate.Folders.Store = "./store";
            Certificate.Keys.Private = "{folders.store}/{domains.0}-private.key";
            Certificate.Keys.Identifier = "{folders.store}/{domains.0}-account-le.key";
            Certificate.Keys.Signing = "{folders.store}/{domains.0}-signing-le.csr";
            Certificate.Keys.Certificate = "{folders.store}/{domains.0}.crt";
            Certificate.Keys.Pfx = "{folders.store}/{domains.0}.pfx";
            Certificate.Keys.Password = "{password}";
            Certificate.Commands.CreatePrivateKey = assemblyPath + "/openssl genrsa -out {keys.private} 2048";
            Certificate.Commands.CreateLetsEncryptKey = assemblyPath + "/openssl genrsa -out {keys.identifier} 4096";
            Certificate.Commands.CreateCertificateRequest = assemblyPath + "/le64 --key {keys.identifier} --csr {keys.signing} --csr-key {keys.private} --crt {keys.certificate} --domains \"{domains}\" --generate-missing --path {folders.wwwroot}\\.well-known\\acme-challenge\\ --unlink --live";
            Certificate.Commands.ConvertToPfx = assemblyPath + "/openssl pkcs12 -export -out {keys.pfx} -inkey {keys.private} -in {keys.certificate} -passout pass:{keys.password}";
        }
    }

    #region SUB-CLASSES

    public class Azure
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TenantId { get; set; }
        public string WebAppResourceId { get; set; }
        public string ResourceGroup { get; set; }
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
