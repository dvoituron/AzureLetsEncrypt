# AzureLetsEncrypt
Simple tool to add a Let's Encrypt SSL certificate to your websites.

this application is a .NET Core tool that generates an SSL certificate, validates it with [LetsEncrypt](https://letsencrypt.org/) and publishes it on an [Azure Web App](https://azure.microsoft.com/en-us/services/app-service/web/).

The detailled steps to create a SSL certificate and validate it with the Let’s Encrypt Certificate Authority are described in my blog : https://dvoituron.com/2018/01/29/ssl-certification-azure-letsencrypt.
This tool automates these steps.

# How to use **AzureLetsEncrypt** ?

In this tool, the first step is to generate a new PFX... and the second step is to publish this file in a Azure Web App. You can configure the tool to execute these two steps or to execute only the first step (and you'll publish the pfx in Azure manually).

## Publish automatically to Azure

1. Download the last release of [AzureLetsEncrypt](https://github.com/dvoituron/AzureLetsEncrypt/releases)
2. Edit the **AppSettings.json** file with your personnal information:
    ```json
    {
        "azure": {
            "clientId": "[clientId]",
            "clientSecret": "[clientSecret]",
            "tenantId": "[tenantId]",
            "webAppResourceId": "[ResourceID]",
            "resourceGroup": "[ResourceGroup]"
        },
        "certificate": {
            "domains": [
                "mydomain.com",
                "www.mydomain.com"
            ],
            "password": "MyP@ssword",
        }
    }
    ```

    - **clientId, clientSecret, tenantId**: 
      To get these Ids, open a `Cloud shell` in Azure portal and run this command 
      ```
      az ad sp create-for-rbac --sdk-auth
      ```
      You can also install Azure CLI on your PC, and execute `az login` and  `az ad sp create-for-rbac --sdk-auth`.  

    - **resourceId, resourceGroup**: in Azure portal, go to App Services > [Web App] > Properties > Resource ID and Resource Group.

    - **domains**: write all domains to include in the Pfx certificate. Your domains must be accessible (ex. http://mydomain.com must return a web content).

    - **password**: define a secret password to protect the generated Pfx. Keep in mind this password to install the pfx file later.

3. Save and include the new **AppSettings.json** file in the release ZIP package.
4. Go to Azure portal, navigate to your Web App Service, and select the **WebJobs** section.
   - Add a **new WebJob**
   - Define a **job name** (ex. RenewSsl)
   - Select your **local ZIP file** name, adapted with your AppSettings.json
   - Define the job type as **Triggered**
     - Trigger type : **Scheduled**
     - **CRON** Expression: `0 0 3 1 * *` Each first day of month, at 3AM
   - Save this new Job. You can Run this job for the first time to validate it. Go to the SSL settings section to check you new SSL certificate.

## Publish manually to Azure
If you prefer to create a validated certificate, but publish this pfx file manually, remove the azure section in the previous AppSettings.json file (keep only the certificate section).

1. Download the last release of [AzureLetsEncrypt](https://github.com/dvoituron/AzureLetsEncrypt/releases)
2. Extract this ZIP package and edit the **AppSettings.json** file with your personnal information:
   Remove the section **azure**, to avoid an automatic certificate upload in Azure (see previous chapter).
    ```json
    {
        "certificate": {
            "domains": [
                "mydomain.com",
                "www.mydomain.com"
            ],
            "password": "MyP@ssword",
        }
    }
    ```
3. Go to Azure portal, navigate to your Web App Service, and select the **Advanced Tools** section (Kudu plateform). In Debug console, create a folder and copy it all files of extracted package (including your AppSeetings.json file).
4. From this folder, execute this command.

   ```
   dotnet AzureLetsEncrypt.dll
   ```
5. You can download your validated Pfx certificate from a subfolder `./store`.
6. Go to **SSL settings** to upload your Pfx.
   - **Private certificate**: upload your Pfx file (using your password)
   - **Bindings**: Add a SSL Binding using a SNI based
