# AzureLetsEncrypt
Simple tool to add a Let's Encrypt SSL certificate to your websites.

this application is a .NET Core tool that generates an SSL certificate, validates it with [LetsEncrypt](https://letsencrypt.org/).

The detailled steps to create a SSL certificate and validate it with the Let’s Encrypt Certificate Authority are described in my blog : https://dvoituron.com/2018/01/29/ssl-certification-azure-letsencrypt.
This tool automates these steps.

> If you want generate and publish automatically your certificate, see https://github.com/dvoituron/AzureLetsEncrypt

## Steps to generate and validate a certificate

1. Go to Azure portal, navigate to your Web App Service, and select the **Console** 

2. Install this tool, using this command:
	```Bash
	dotnet tool install dvoituron.tools.azureletsencrypt --tool-path tools
	```

3. From you root folder (D:\home\site\wwwroot), run this command:
	```Bash
	.\tools\AzureLetsEncrypt --domains=mydomain.com,www.mydomain.com --password=My@Password
	```

4. You can download your validated Pfx certificate from a subfolder `./store`.

5. Go to **SSL settings** to upload your Pfx.
   - **Private certificate**: upload your Pfx file (using your password)
   - **Bindings**: Add a SSL Binding using a SNI based