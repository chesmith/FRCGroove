using FRCGroove.Lib.Models.TBAv3;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FRCGroove.Lib
{
    public static class NexusAPI
    {
        private static readonly RestClient _client = new RestClient("https://frc.nexus/api/v1");

        public static Dictionary<string, string> GetPits(string eventCode)
        {
            string path = $"event/{eventCode}/pits";

            var request = new RestRequest(path);
            request.AddHeader("Nexus-Api-Key", ConfigurationManager.AppSettings["NexusApiKey"]);
            var resp = _client.Execute(request);

            if (resp.IsSuccessful && resp.Content != null)
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(resp.Content);
            }
            else
            {
                return null;
            }
        }
    }
}
