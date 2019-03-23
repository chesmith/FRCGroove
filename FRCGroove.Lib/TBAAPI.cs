using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestSharp;
using RestSharp.Authenticators;

using FRCGroove.Lib.models;
using System.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace FRCGroove.Lib
{
    public static class TBAAPI
    {
        private static RestClient _client = new RestClient("https://www.thebluealliance.com/api/v3");

        public static TBAStats GetStats(string eventCode)
        {
            string path = $"event/{eventCode}/oprs";

            var request = new RestRequest(path);
            request.AddHeader("X-TBA-Auth-Key", ConfigurationManager.AppSettings["TBAAuthKey"]);
            var resp = _client.Execute(request);

            JObject j = JObject.Parse(resp.Content);
            TBAStats stats = new TBAStats(j);
            
            return stats;
        }
    }
}
