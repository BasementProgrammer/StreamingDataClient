# StreamingDataClient
A .NET Web app designed to stream data to a cloud based streaming data service.
This application is intended to be used as a testbed for things like IoT sameples
and other applications where you need to send a set of events from one (or many) devices
to a cloud based service.

The intention is for this project to support multiple cloud services as it evolves.

DataSender - A library with the functiona.ity to send messages to a streaming service.
DataSender\IDataSender              - Interface for the DataSender applications.
DataSender\OCIStreamsDataSender     - Data asender implementation for the OCI Streaming service.
DataSender\StreamConfig             - Configuration class designed to hold the stream config details

DataSenderTestApp                   - CLI to test the library.

DataSenderWeb                       - ASP.NET Core Test application, includes docker file.

# CLI appsettings.json
Currently the CLI uses the following appsettings.json format
{
  "ProfileName": "WORK",
  "EndpointConfiguration": "https://cell-1.streaming.us-ashburn-1.oci.oraclecloud.com",
  "StreamId": "ocid1.stream.oc1.iad.amaaaaaafvqkqmqaqe7pq72n7ob6qe7xwgfi6i5kcuzldm6hbr3ad5rpr7aa"
}

## OCI 

ProfileName                         - This is the name of the profile configured in the CLI that provides connectivity to your account.
EndpointConfiguration               - The name of the streaming endpoint, you can find this as part of your service definition
StreamId                            - This is the ID for the stream you are sending to. Note that multiple streams use the same endpoint.