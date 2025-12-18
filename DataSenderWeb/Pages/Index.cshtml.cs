using DataSender;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Server.Kestrel;
using Oci.Common;
using Oci.Common.Auth;
using Oci.SecretsService.Models;
using Oci.SecretsService.Requests;
using Oci.VaultService.Models;
using Oci.VaultService.Requests;
using Oci.VaultService.Responses;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace DataSenderWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _appConfig;
        private readonly IBasicAuthenticationDetailsProvider _config;

        public IndexModel(ILogger<IndexModel> logger) : base()
        {
            _logger = logger;
            // Load configuration from appsettings.json and environment variables
            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .Build();
            _appConfig = host.Services.GetRequiredService<IConfiguration>();

            
            try
            {
                _config = ResourcePrincipalAuthenticationDetailsProvider.GetProvider();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing Instance Principals authentication: {ex.Message}");
                // Fallback to config file authentication
                _config = new ConfigFileAuthenticationDetailsProvider("DEFAULT");
            }

            string vaultId;
            vaultId = Environment.GetEnvironmentVariable("VaultID");

            StreamId = populateConfigFromVault("StreamId", vaultId);
            EndpointConfiguration = populateConfigFromVault("EndpointConfiguration", vaultId);
        }

        private string populateConfigFromVault(string secretName, string vaultId)
        {
            var client = new Oci.SecretsService.SecretsClient(_config);
            var vaultClient = new Oci.VaultService.VaultsClient(_config);


            var getSecretBundleByNameRequest = new GetSecretBundleByNameRequest
            {
                SecretName = secretName,
                VaultId = vaultId,
            };

            var secretResponce = client.GetSecretBundleByName(getSecretBundleByNameRequest).Result;
            var secretBundle = secretResponce.SecretBundle;
            Base64SecretBundleContentDetails secretBundleContent = (Base64SecretBundleContentDetails)secretBundle.SecretBundleContent;
            var content = secretBundleContent.Content;
            var decodedBytes = Convert.FromBase64String(content);

            return Encoding.UTF8.GetString(decodedBytes);
        }

        private string populateConfigFromVault(string value)
        {
            var client = new Oci.SecretsService.SecretsClient(_config);
            GetSecretBundleRequest getSecretBundleRequest = new GetSecretBundleRequest
            {
                SecretId = value
            };
            var secretResponce = client.GetSecretBundle(getSecretBundleRequest).Result;
            var secretBundle = secretResponce.SecretBundle;
            Base64SecretBundleContentDetails secretBundleContent = (Base64SecretBundleContentDetails)secretBundle.SecretBundleContent;
            var content = secretBundleContent.Content;
            var decodedBytes = Convert.FromBase64String(content);

            return Encoding.UTF8.GetString(decodedBytes);
        }

        public string StreamId { get; set; }
        public string ProfileName { get; set; }
        public string EndpointConfiguration { get; set; }

        public Dictionary<string, string> ConvertFormCollectionToDictionary(IFormCollection formCollection)
        {
            return formCollection.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());
        }


        public IActionResult OnPostSendDataToStream (IFormCollection formCollection)
         {
            // Create a StreamConfig object from the configuration
            StreamConfig config = new StreamConfig
            {
                ProfileName = formCollection["ProfileName"],
                EndpointConfiguration = formCollection["EndpointConfiguration"],
            };

            Dictionary<string, string> formParameters = ConvertFormCollectionToDictionary(formCollection);

            // get the values from the form
            int numberOfClients = int.Parse(formParameters["numberOfClients"]);
            int numberOfMessages = int.Parse(formParameters["numberOfMessages"]);
            int messageDelay = int.Parse(formParameters["messageDelay"]);
            string dataToSend = formParameters["dataToSend"];
            dataToSend = dataToSend.Trim();

            double minimumNormalTemp = double.Parse(formParameters["minimumNormalTemp"]);
            double maximumNormalTemp = double.Parse(formParameters["maximumNormalTemp"]);
            double minimumErrorTemp = double.Parse(formParameters["minimumErrorTemp"]);
            double maximumErrorTemp = double.Parse(formParameters["maximumErrorTemp"]);
            double tempErrorRate = double.Parse(formParameters["tempErrorRate"]);
            string dataConfiguration = formParameters["dataConfiguration"];

            // Variables to replace
            // %%timeStamp%% - DateTime.Now.ToString("o")
            // %%messageId%% - The number of the message for the particular client
            // %%clientId%% - The number for the particular client

            int totalNumberOfMessages = numberOfClients * numberOfMessages;

            // Create a collection of data sender objects
            IList<IDataSender> dataSenders = new List<IDataSender>();
            try
            {
                // Create a list of data senders based on the number of clients specified
                for (int i = 0; i < numberOfClients; i++)
                {
                    // Create a new data sender for each client
                    dataSenders.Add(new OCIStreamsDataSender(config));
                }

                Random random = new Random();
                double checkRate = random.NextDouble(); // This is just to seed the random number generator
                // Loop through each data sender and send the specified number of messages
                for (int i = 0; i < numberOfMessages; i++)
                {
                    for (int j = 0; j < numberOfClients; j++)
                    {
                        decimal reportingTemp = 0;
                        if (dataConfiguration == "Temperature")
                        {
                            // Figure out if this shoul dbe a normal or error temperature
                            if (random.NextDouble() < (tempErrorRate / 100))
                            {
                                // Generate an error temperature
                                reportingTemp = (decimal)(random.NextDouble() * (maximumErrorTemp - minimumErrorTemp) + minimumErrorTemp);
                            }
                            else
                            {
                                // Generate a normal temperature
                                reportingTemp = (decimal)(random.NextDouble() * (maximumNormalTemp - minimumNormalTemp) + minimumNormalTemp);
                            }
                        }
                        // Replace the placeholders in the data string
                        string message = dataToSend
                            .Replace("%%timeStamp%%", DateTime.Now.ToString("o"))
                            .Replace("%%messageId%%", (i + 1).ToString())
                            .Replace("%%temp%%", reportingTemp.ToString())
                            .Replace("%%clientId%%", (j + 1).ToString());
                        // Send the message using the current data sender
                        dataSenders[j].SendData(new[] { message });
                    }
                    // Delay if specified
                    if (messageDelay > 0)
                    {
                        System.Threading.Thread.Sleep(messageDelay);
                    }
                }

            }
            finally
            {
                // Dispose of the data senders if necessary
                foreach (var sender in dataSenders)
                {
                    if (sender is IDisposable disposableSender)
                    {
                        disposableSender.Dispose();
                    }
                }
            }

            // Handle form submission or other post actions here
            _logger.LogInformation("Form submitted successfully.");
            return RedirectToPage("Index");
        }

    }
}
