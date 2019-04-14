using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRCGroove.Lib.Models
{
    public class TBAStatsCollection
    {
        public Dictionary<string, double> ccwms { get; set; }
        public Dictionary<string, double> dprs { get; set; }
        public Dictionary<string, double> oprs { get; set; }

        public TBAStatsCollection(JObject j)
        {
            if(j.ContainsKey("oprs"))
                oprs = j["oprs"].ToObject<Dictionary<string, double>>();
            if(j.ContainsKey("dprs"))
                dprs = j["dprs"].ToObject<Dictionary<string, double>>();
            if(j.ContainsKey("ccwms"))
                ccwms = j["ccwms"].ToObject<Dictionary<string, double>>();
        }
    }
}
