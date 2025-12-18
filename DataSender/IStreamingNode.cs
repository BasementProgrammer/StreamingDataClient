using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSender
{
    public interface IStreamingNode
    {        
        /// <summary>
        /// Creates a single data report for the node that includes data from all configured
        /// reporting nodes. 
        /// </summary>
        /// <param name="templateMessage">The templatized message</param>
        /// /// <param name="parameters">The configuration parameters that tell the node how to replce the placeholders in the message</param>
        /// <returns>The message with the relavent placeholders updated</returns>
        string ReportData(string templateMessage, IDictionary<string, string> parameters);

        /// <summary>
        /// Retrieve the template data for the sender to be added to the
        /// template message.
        /// </summary>
        /// <returns></returns>
        string GetTemplateData();
    }
}
