# AzureLetsEncrypt
Simple tool to add a Let's Encrypt SSL certificate to your websites.
This application is a .NET Core tool that generates an SSL certificate, validated it with [LetsEncrypt](https://letsencrypt.org/).

> 2 minutes of a live demo: [https://youtu.be/OCKpC1W4Wks](https://youtu.be/OCKpC1W4Wks)

## Steps to generate and to validate a certificate

**Prerequisite**: you must have a website accessible from your domain name (in http). 
Indeed, the validation of LetsEncrypt tries to check the presence of a file available in your website.

1. Go to **Azure portal**, navigate to your **Advanced Tools** (Kudu environment).
   And select **Debug console / CMD**. 

2. **Install this tool**, using this command:
	```Bash
	dotnet tool install dvoituron.tools.azureletsencrypt --tool-path tools
	```

3. **Run this command**, using your domains and password.
	```Bash
	.\tools\AzureLetsEncrypt --domains=mydomain.com,www.mydomain.com --password=MyPassword
	```

4. You can **download your validated Pfx certificate** from the subfolder `./store` of your website.

5. In the Azure portal, go to **SSL settings** panel, to upload your Pfx.
   - **Private certificate**: upload your Pfx file (using your password)
   - **Bindings**: Add a SSL Binding using a SNI based

## When to renew the certificate?
LetsEncrypt's certificates expire after 90 days. 
This is [an obligation imposed by LetsEncrypt](https://letsencrypt.org/2015/11/09/why-90-days.html).
So, you need to reproduce these steps, each 3 months.

## More about this project

The detailled steps to create a SSL certificate and validate it with the Let’s Encrypt Certificate Authority 
are described on [my blog](https://dvoituron.com/2018/01/29/ssl-certification-azure-letsencrypt). 
This tool automates these steps.

If you want generate and publish automatically your certificate, 
see [https://github.com/dvoituron/AzureLetsEncrypt](https://github.com/dvoituron/AzureLetsEncrypt)