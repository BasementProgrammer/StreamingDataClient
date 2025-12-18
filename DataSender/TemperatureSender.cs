using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSender
{
    public class TemperatureSender : IStreamingSender
    {
        public string GetTemplateData()
        {
            return "\t\"temperature\": %%temp%%,";
        }

        public string ReportData(string templateMessage, IDictionary<string, string> parameters)
        {
            double minimumNormalTemp = double.Parse(parameters["minimumNormalTemp"]);
            double maximumNormalTemp = double.Parse(parameters["maximumNormalTemp"]);
            double minimumErrorTemp = double.Parse(parameters["minimumErrorTemp"]);
            double maximumErrorTemp = double.Parse(parameters["maximumErrorTemp"]);
            double tempErrorRate = double.Parse(parameters["tempErrorRate"]);

            Random random = new Random();
            double checkRate = random.NextDouble(); // This is just to seed the random number generator
            decimal reportingTemp;

            if (random.NextDouble() < (tempErrorRate / 100))
            {
                // Generate an error temperature
                reportingTemp = (decimal)(random.NextDouble() * (maximumErrorTemp - minimumErrorTemp) + minimumErrorTemp);
            }
            else
            {
                // Generate a normal temperature
                reportingTemp = (decimal)(random.NextDouble() * (maximumNormalTemp - minimumNormalTemp) + minimumNormalTemp);
            }

            return templateMessage
                .Replace("%%temp%%", reportingTemp.ToString());
        }
    }
}
