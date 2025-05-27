using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using DataSender;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DataSenderTestApp
{
    internal class Program
    {
        public static IDataSender _ds;

        static void Main(string[] args)
        {

            using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .Build();
            IConfiguration appConfig = host.Services.GetRequiredService<IConfiguration>();

            /*
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>()
                {
                    ["ProfileName"] = "ProfileName",
                    ["EndpointConfiguration"] = "EndpointConfiguration",
                    ["StreamId"] = "StreamId"
                })
                .Build();
            */

            StreamConfig config = new StreamConfig
            {
                ProfileName = appConfig.GetValue<String>("ProfileName"),
                EndpointConfiguration = appConfig.GetValue<String>("EndpointConfiguration"),
                StreamId = appConfig.GetValue<String>("StreamId")
            };

            /*
            StreamConfig config = new StreamConfig()
            {
                ProfileName = "WORK",
                EndpointConfiguration = "https://cell-1.streaming.us-ashburn-1.oci.oraclecloud.com",
                StreamId = "ocid1.stream.oc1.iad.amaaaaaafvqkqmqaqe7pq72n7ob6qe7xwgfi6i5kcuzldm6hbr3ad5rpr7aa"
            };
            */

            _ds = new OCIStreamsDataSender(config);
            string testData = "\"hello{0}\" : \"world{0}\"";

            List<string> dataArray = new List<string>();

            for (int i = 0; i < 10; i++)
            {
                dataArray.Add("{" + string.Format (testData, i) + "}");
            }
            _ds.SendData(dataArray.ToArray());


        }
    }
}
