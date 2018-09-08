﻿using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System;
using System.Diagnostics;
using System.IO;

namespace AzureLetsEncrypt
{
    public class Program
    {
        static void Main(string[] args)
        {
            var watcher = new Stopwatch();

            var createSsl = new Commands.CreateSsl();
            createSsl.Interactive();

            //Console.WriteLine("Reading AppSettings.json. Environment: " + Environment.GetEnvironmentVariable("Environment"));
            //var setting = new Configuration.AppSettings();

            //var credentials = SdkContext.AzureCredentialsFactory
            //                            .FromServicePrincipal(setting.Azure.ClientID,
            //                                                  setting.Azure.ClientSecret,
            //                                                  setting.Azure.TenantID, 
            //                                                  AzureEnvironment.AzureGlobalCloud);

            //Console.WriteLine("Connecting to Azure.");
            //var azure = Azure.Configure()
            //                 .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
            //                 .Authenticate(credentials)
            //                 .WithDefaultSubscription();

            //var certificate = new AzureCertificate(azure);

            //Console.WriteLine("Uploading and binding certificates.");
            //certificate.AddLogger((message) =>
            //                      {
            //                          Console.WriteLine("  . " + message);
            //                      })
            //           .Load(setting.Certificate.Pfx.Filename, setting.Certificate.Pfx.Password)
            //           .ForWebAppID(setting.Certificate.Azure.WebAppResourceId)
            //           .UploadTo(setting.Certificate.Azure.ResourceGroup)
            //           .DefineSslBindingFor(setting.Certificate.Azure.Hostnames);

            Console.WriteLine($"Finished in {watcher.ElapsedMilliseconds} ms.");

            if (System.Diagnostics.Debugger.IsAttached)
                Console.ReadKey();
        }
    }
}
