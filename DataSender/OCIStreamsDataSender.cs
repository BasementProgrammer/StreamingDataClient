using Oci.Common.Auth;
using Oci.Common.Waiters;
using Oci.StreamingService;
using Oci.StreamingService.Models;
using Oci.StreamingService.Requests;
using Oci.StreamingService.Responses;
using System.Text;

namespace DataSender
{
    public class OCIStreamsDataSender : IDataSender, IDisposable
    {

        // OCI Streams client and configuration
        IBasicAuthenticationDetailsProvider _config;
        StreamClient _client;
        StreamConfig _streamConfig;
        public OCIStreamsDataSender(StreamConfig streamConfig)
        {
            // Initialize OCI Streams client
            _streamConfig = streamConfig;
            try
            {
                _config = ResourcePrincipalAuthenticationDetailsProvider.GetProvider();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing Instance Principals authentication: {ex.Message}");
                // Fallback to config file authentication
                _config = new ConfigFileAuthenticationDetailsProvider(_streamConfig.ProfileName);
            }
            _client = new StreamClient(_config);
            _client.SetEndpoint(_streamConfig.EndpointConfiguration);
        }
        public void SendData(string[] data)
        {
            try
            {
                List<PutMessagesDetailsEntry> messages = new List<PutMessagesDetailsEntry>();
                foreach (string message in data)
                {
                    PutMessagesDetailsEntry detailsEntry = new PutMessagesDetailsEntry
                    {
                        Key = Encoding.UTF8.GetBytes($"data"),
                        Value = Encoding.UTF8.GetBytes(message)
                    };
                    messages.Add(detailsEntry);
                }

                PutMessagesDetails messagesDetails = new PutMessagesDetails
                {
                    Messages = messages
                };
                PutMessagesRequest putRequest = new PutMessagesRequest
                {
                    StreamId = _streamConfig.StreamId,
                    PutMessagesDetails = messagesDetails
                };
                var response = _client.PutMessages(putRequest);
                PutMessagesResponse putResponse = response.Result;

                // the putResponse can contain some useful metadata for handling failures
                foreach (PutMessagesResultEntry entry in putResponse.PutMessagesResult.Entries)
                {
                    if (entry.Error != null)
                    {
                        Console.WriteLine($"Error({entry.Error}): {entry.ErrorMessage}");
                    }
                    else
                    {
                        Console.WriteLine($"Published message to partition {entry.Partition}, offset {entry.Offset}");
                    }
                }

                // Implementation for sending data to OCI Streams
                Console.WriteLine($"Sending data to OCI Streams: {data}");
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error sending data to OCI Streams: {ex.Message}");
                throw;
            }
        }
        

        // Dispose of the client when done
        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
