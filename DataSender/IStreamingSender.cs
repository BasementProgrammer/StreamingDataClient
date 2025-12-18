using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSender
{
    /// <summary>
    /// Data senders represent a single sensor sending data from a node.
    ///     * sender could be a GPS 
    ///     * temperature sensor
    ///     * humidity sensor
    /// Each node is responsible for enriching the template message
    /// before it is sent. Each node operates independantly.
    /// </summary>
    public interface IStreamingSender
    {
        /// <summary>
        /// Replaces placeholders in the template message with specific data elements
        /// for the message. Note, there is no validation to ensure that 
        /// duplicate placeholders are not used.
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
