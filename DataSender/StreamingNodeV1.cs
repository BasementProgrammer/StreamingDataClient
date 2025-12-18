using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSender
{
    public class StreamingNodeV1 : IStreamingNode
    {
        /// <summary>
        /// Collection of streaming senders that make up the node.
        /// </summary>
        public List<IStreamingSender> SensorCollection { get; } = new List<IStreamingSender>(); 
        public string GetTemplateData()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine("\t\"clientId\": %%clientId%%,");
            stringBuilder.AppendLine("\t\"messageId\": %%messageId%%,");
            stringBuilder.AppendLine("\t\"timestamp\": \"%%timeStamp%%\",");
            stringBuilder.AppendLine("\t\"source\": \"DataSenderWeb - Client %%clientId%%\"");
            foreach (var sensor in SensorCollection)
            {
                stringBuilder.AppendLine(sensor.GetTemplateData());
            }
            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }

        public string ReportData(string templateMessage, IDictionary<string, string> parameters)
        {
            throw new NotImplementedException();
        }
    }
}
