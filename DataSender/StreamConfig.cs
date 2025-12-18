using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSender
{
    public class StreamConfig
    {
        public StreamConfig()
        {
            ProfileName = "DEFAULT";
        }
        /// <summary>
        /// The name of the cloud profile to use for authentication.
        /// </summary>
        public string ProfileName { get; set; }
        /// <summary>
        /// For OCI this is the endpointURL to referance your stream.
        /// </summary>
        public string EndpointConfiguration { get; set; }
        /// <summary>
        /// For OCI this is the OCID of the stream.
        /// </summary>
        public string StreamId { get; set; }
    }
}
