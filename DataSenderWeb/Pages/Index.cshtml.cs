using DataSender;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;

namespace DataSenderWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _appConfig;

        public IndexModel(ILogger<IndexModel> logger)
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
        }

        public IActionResult OnPostSendDataToStream (IFormCollection formCollection)
         {


            // Create a StreamConfig object from the configuration
            StreamConfig config = new StreamConfig
            {
                ProfileName = _appConfig.GetValue<String>("ProfileName"),
                EndpointConfiguration = _appConfig.GetValue<String>("EndpointConfiguration"),
                StreamId = _appConfig.GetValue<String>("StreamId")
            };

            // get the values from the form
            int numberOfClients = int.Parse(formCollection["numberOfClients"]);
            int numberOfMessages = int.Parse(formCollection["numberOfMessages"]);
            int messageDelay = int.Parse(formCollection["messageDelay"]);
            string dataToSend = formCollection["dataToSend"];
            dataToSend = dataToSend.Trim();

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

                // Loop through each data sender and send the specified number of messages
                for (int i = 0; i < numberOfMessages; i++)
                {
                    for (int j = 0; j < numberOfClients; j++)
                    {
                        // Replace the placeholders in the data string
                        string message = dataToSend
                            .Replace("%%timeStamp%%", DateTime.Now.ToString("o"))
                            .Replace("%%messageId%%", (i + 1).ToString())
                            .Replace("%%clientId%%", (j + 1).ToString());
                        // Send the message using the current data sender
                        dataSenders[i].SendData(new[] { message });
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
