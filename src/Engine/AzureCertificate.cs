using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace AzureLetsEncrypt.Engine
{
    public class AzureCertificate
    {
        private Configuration.AppSettings _appSettings;
        private IAzure _azure;
        private string _certificate

        public AzureCertificate(Configuration.AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public AzureCertificate Connect()
        {
            string shortClientId = _appSettings.Azure.ClientId.Substring(0, _appSettings.Azure.ClientId.Length > 8 ? 8 : _appSettings.Azure.ClientId.Length);
            Shell.WriteCommandLog($"Connection to Azure for ClientId '{shortClientId}...'.");

            var credentials = SdkContext.AzureCredentialsFactory
                                        .FromServicePrincipal(_appSettings.Azure.ClientId,
                                                              _appSettings.Azure.ClientSecret,
                                                              _appSettings.Azure.TenantId,
                                                              AzureEnvironment.AzureGlobalCloud);

            _azure = Azure.Configure()
                          .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                          .Authenticate(credentials)
                          .WithDefaultSubscription();

            return this;
        }

        public AzureCertificate Upload()
        {
            if (_azure == null)
                throw new ArgumentException("Call the Connect method before this one.");

            // Load

            // App Services > [Web App] > properties > Resource ID
            var webApp = _azure.WebApps.GetById(webApppResourceID);

            // Upload to Azure
            Shell.WriteCommandLog($"Upload the certificate to Azure.");
            UploadTo(webApp, resourceGroup);

            // Binding
            foreach (var item in forHostnames)
            {
                Shell.WriteCommandLog($"Setting SSL Binding for hostname '{item}'.");
                DefineSslBindingFor(item);
            }


            return this;

        }

        //public string Path { get; private set; }
        //public string Password { get; private set; }
        //public string Thumbprint => this._inner.Thumbprint;
        //public byte[] AllBytes => System.IO.File.ReadAllBytes(Path);

        private void UploadTo(IWebApp webApp, string toResourceGroup)
        {
            string filename = setting.Certificate.Pfx.Filename;
            string password = setting.Certificate.Pfx.Password;

            var x509 = new X509Certificate2(filename, password);
            var thumbprint = x509.Thumbprint;
            var allBytes = System.IO.File.ReadAllBytes(filename);
            var group = String.IsNullOrEmpty(toResourceGroup) ? webApp.ResourceGroupName : toResourceGroup;

            var appServiceCertificate = _azure.AppServices
                                              .AppServiceCertificates
                                              .ListByResourceGroup(group)
                                              .FirstOrDefault(i => i.Thumbprint == thumbprint);

            if (appServiceCertificate == null)
            {
                appServiceCertificate = _azure.AppServices
                                               .AppServiceCertificates
                                               .Define(thumbprint)
                                                   .WithRegion(webApp.Region)
                                                   .WithExistingResourceGroup(group)
                                                   .WithPfxByteArray(allBytes)
                                                   .WithPfxPassword(password)
                                               .Create();
            }
        }

        private void DefineSslBindingFor(IWebApp webApp, string forHostname)
        {
            webApp.Update()
                   .DefineSslBinding()
                      .ForHostname(forHostname)
                      .WithExistingCertificate(thumbprint)
                      .WithSniBasedSsl()
                      .Attach()
                   .Apply();
        }
    }
}
