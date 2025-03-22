using System.Collections.Generic;
using System.Text.Json;

namespace FRCGroove.Lib.Models.TBAv3
{
    public class TBAStatsCollection
    {
        public Dictionary<string, double> ccwms { get; set; }
        public Dictionary<string, double> dprs { get; set; }
        public Dictionary<string, double> oprs { get; set; }

        public TBAStatsCollection(JsonDocument doc)
        {
            ccwms = new Dictionary<string, double>();
            dprs = new Dictionary<string, double>();
            oprs = new Dictionary<string, double>();

            foreach (JsonProperty property in doc.RootElement.GetProperty("ccwms").EnumerateObject())
            {
                ccwms[property.Name] = property.Value.GetDouble();
            }

            foreach (JsonProperty property in doc.RootElement.GetProperty("dprs").EnumerateObject())
            {
                dprs[property.Name] = property.Value.GetDouble();
            }

            foreach (JsonProperty property in doc.RootElement.GetProperty("oprs").EnumerateObject())
            {
                oprs[property.Name] = property.Value.GetDouble();
            }
        }
    }
}
