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
        private IAzure _azure;

        public AzureCertificate(Configuration.AppSettings appSettings)
        {
            AppSettings = appSettings;
        }

        public Configuration.AppSettings AppSettings { get; private set; }

        public AzureCertificate Connect()
        {
            string clientId = AppSettings.Azure.ClientId;
            string shortClientId = clientId.Substring(0, clientId.Length > 8 ? 8 : clientId.Length);
            Shell.WriteCommandLog($"Connection to Azure for ClientId '{shortClientId}...'.");

            var credentials = SdkContext.AzureCredentialsFactory
                                        .FromServicePrincipal(AppSettings.Azure.ClientId,
                                                              AppSettings.Azure.ClientSecret,
                                                              AppSettings.Azure.TenantId,
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

            // App Services > [Web App] > properties > Resource ID
            var webApp = _azure.WebApps.GetById(AppSettings.Azure.WebAppResourceId);

            if (webApp == null)
                throw new ArgumentException("ERR: WebAppResourceId not found.");

            // Upload to Azure
            Shell.WriteCommandLog($"Upload the certificate to Azure.");
            var thumbprint = UploadTo(webApp, AppSettings.Azure.ResourceGroup);
            Shell.WriteCommandLog($"Certificate '{thumbprint}' uploaded.");

            // Binding SSL certificate for hostnames
            foreach (var host in AppSettings.Certificate.Domains)
            {
                Shell.WriteCommandLog($"Setting SSL Binding for hostname '{host}'.");
                DefineSslBindingFor(webApp, thumbprint, host);
            }
            
            return this;        
        }

        private string UploadTo(IWebApp webApp, string toResourceGroup)
        {
            string filename = AppSettings.Certificate.Keys.Pfx;
            string password = AppSettings.Certificate.Keys.Password;

            var x509 = new X509Certificate2(filename, password);
            var thumbprint = x509.Thumbprint;
            var allBytes = System.IO.File.ReadAllBytes(filename);
            var group = String.IsNullOrEmpty(toResourceGroup) ? webApp.ResourceGroupName : toResourceGroup;

            var appServiceCertificate = _azure.AppServices
                                              .AppServiceCertificates
                                              .ListByResourceGroup(group)
                                              .FirstOrDefault(i => i.Thumbprint == thumbprint);

            // If not existing
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

            return thumbprint;
        }

        private void DefineSslBindingFor(IWebApp webApp, string thumbprint, string forHostname)
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
