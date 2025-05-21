using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSender
{
    /// <summary>
    /// Interface for the data sender.
    /// An IDataSender may send streaming data to one of several back ends.
    /// Amazon Kinesis
    /// OCI Streaming
    /// ...
    /// </summary>
    public interface IDataSender
    {
        void SendData(string data);
    }
}
