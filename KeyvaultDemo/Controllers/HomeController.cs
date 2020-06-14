using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using KeyvaultDemo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace KeyvaultDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            SecretClientOptions options = new SecretClientOptions()
            {
                Retry =
                {
                    Delay= TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(16),
                    MaxRetries = 5,
                    Mode = RetryMode.Exponential
                }
            };
            var client = new SecretClient(new Uri("https://kvmanagedidenty.vault.azure.net/"), new DefaultAzureCredential()
            {

            }, options);

            KeyVaultSecret secret = client.GetSecret("SampleSecret");

            ViewData["Secret"] = secret.Value;
            return View();
        }

        /// <summary>
        /// 1.Create an app service plan
        /// 2. Create a web app and deploy it under that plan.
        /// 3. Register the web app with azure AD, we get back a principleID
        /// 4. Create a keyvault
        /// 5. add the web app to the keyvault as an access policy and grant it the permission (cmd in blog)
        /// 6. Use this method to use the secret value inside the code.
        /// 
        /// In this method , we get the secret values from KV and display it in our view
        /// This way we dont have to maintain any secrets in our appsettings 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Privacy()
        {
            // Use this code to get the values during development
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyvaultclient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            var secret = await keyvaultclient.GetSecretAsync("https://kvmanagedidenty.vault.azure.net/", "SampleSecret");
            ViewData["Secret"] = secret.Value;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
