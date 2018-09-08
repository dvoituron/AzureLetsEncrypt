using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Fluent;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace AzureLetsEncrypt
{
    public class AzureCertificate
    {
        private X509Certificate2 _inner;
        private IAzure _azure;
        private IWebApp _webApp;
        private IAppServiceCertificate _appServiceCertificate;

        public AzureCertificate(IAzure azure)
        {
            _azure = azure;
        }

        public string Path { get; private set; }
        public string Password { get; private set; }
        public string Thumbprint => this._inner.Thumbprint;
        public byte[] AllBytes => System.IO.File.ReadAllBytes(Path);
        private Action<string> Logger { get; set; }

        public AzureCertificate AddLogger(Action<string> logger)
        {
            Logger = logger;
            return this;
        }

        public AzureCertificate Load(string path, string password)
        {
            Logger.Invoke($"Loading certificate '{path}'.");
            Path = path;
            Password = password;
            _inner = new X509Certificate2(path, password);

            return this;
        }

        public AzureCertificate ForWebAppID(string webApppResourceID)
        {
            Logger.Invoke($"Setting WebApp '{webApppResourceID}'.");
            // App Services > [Web App] > properties > Resource ID
            _webApp = _azure.WebApps.GetById(webApppResourceID);
            return this;
        }

        public AzureCertificate UploadTo(string toResourceGroup)
        {
            if (_webApp == null)
                throw new ArgumentNullException("Please, call the ForWebAppID method before this method.");

            string group = String.IsNullOrEmpty(toResourceGroup) ? _webApp.ResourceGroupName : toResourceGroup;

            Logger.Invoke($"Uploading certificate '{Thumbprint}' in '{group}'.");
            _appServiceCertificate = _azure.AppServices
                                           .AppServiceCertificates
                                           .ListByResourceGroup(group)
                                           .FirstOrDefault(i => i.Thumbprint == Thumbprint);

            if (_appServiceCertificate == null)
            {
                Logger.Invoke($"Uploading certificate '{Thumbprint}' in '{group}'.");
                _appServiceCertificate = _azure.AppServices
                                               .AppServiceCertificates
                                               .Define(this.Thumbprint)
                                                   .WithRegion(_webApp.Region)
                                                   .WithExistingResourceGroup(group)
                                                   .WithPfxByteArray(this.AllBytes)
                                                   .WithPfxPassword(this.Password)
                                               .Create();
            }

            return this;
        }

        public AzureCertificate DefineSslBindingFor(string forHostname)
        {
            if (_appServiceCertificate == null)
                throw new ArgumentNullException("Please, call the UploadTo method before this method.");

            Logger.Invoke($"Setting SSL Binding for hostname '{forHostname}'.");
            _webApp.Update()
                   .DefineSslBinding()
                      .ForHostname(forHostname)
                      .WithExistingCertificate(this.Thumbprint)
                      .WithSniBasedSsl()
                      .Attach()
                   .Apply();

            return this;
        }

        public AzureCertificate DefineSslBindingFor(string[] forHostnames)
        {
            foreach (var item in forHostnames)
            {
                DefineSslBindingFor(item);
            }

            return this;
        }
    }
}
