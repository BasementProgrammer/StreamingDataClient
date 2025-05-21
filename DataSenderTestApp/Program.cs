using System;
using DataSender;

namespace DataSenderTestApp
{
    internal class Program
    {
        public static IDataSender _ds = new OCIStreamsDataSender();

        static void Main(string[] args)
        {
            string testData = "\"hello{0}\" : \"world{0}\"";

            for (int i = 0; i < 10; i++)
            {
                _ds.SendData("{" + string.Format (testData, i) + "}");
            }
            

        }
    }
}
