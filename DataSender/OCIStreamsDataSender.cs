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
        ConfigFileAuthenticationDetailsProvider _config;
        StreamClient _client;
        public OCIStreamsDataSender()
        {
            // Initialize OCI Streams client
            _config = new ConfigFileAuthenticationDetailsProvider("WORK");
            _client = new Oci.StreamingService.StreamClient(_config);
            _client.SetEndpoint("https://cell-1.streaming.us-ashburn-1.oci.oraclecloud.com");
        }
        public void SendData(string data)
        {
            try
            {
                List<PutMessagesDetailsEntry> messages = new List<PutMessagesDetailsEntry>();

                // We can send more than one message at a time here....
                PutMessagesDetailsEntry detailsEntry = new PutMessagesDetailsEntry
                {
                    Key = Encoding.UTF8.GetBytes($"data"),
                    Value = Encoding.UTF8.GetBytes(data)
                };
                messages.Add(detailsEntry);


                PutMessagesDetails messagesDetails = new PutMessagesDetails
                {
                    Messages = messages
                };
                PutMessagesRequest putRequest = new PutMessagesRequest
                {
                    StreamId = "ocid1.stream.oc1.iad.amaaaaaafvqkqmqaqe7pq72n7ob6qe7xwgfi6i5kcuzldm6hbr3ad5rpr7aa",
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
            }
        }

        // Dispose of the client when done
        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
