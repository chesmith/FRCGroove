using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestSharp;
using RestSharp.Authenticators;

using FRCGroove.Lib.Models;
using System.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace FRCGroove.Lib
{
    public static class TBAAPI
    {
        private static readonly RestClient _client = new RestClient("https://www.thebluealliance.com/api/v3");

        public static TBAStatsCollection GetStats(string eventCode)
        {
            string path = $"event/{eventCode}/oprs";

            var request = new RestRequest(path);
            request.AddHeader("X-TBA-Auth-Key", ConfigurationManager.AppSettings["TBAAuthKey"]);
            var resp = _client.Execute(request);

            TBAStatsCollection stats = null;
            if (resp.Content != "null" && resp.Content.Length > 5)
            {
                JObject j = JObject.Parse(resp.Content);
                stats = new TBAStatsCollection(j);
            }

            return stats;
        }

        public static List<TBAMatchData> GetMatches(string eventCode)
        {
            string path = $"event/{eventCode}/matches";

            var request = new RestRequest(path);
            request.AddHeader("X-TBA-Auth-Key", ConfigurationManager.AppSettings["TBAAuthKey"]);
            var resp = _client.Execute<List<TBAMatchData>>(request);

            return resp.Data;
        }

        public static List<TBATeam> GetEventTeams(string eventCode)
        {
            string path = $"event/{eventCode}/teams";

            var request = new RestRequest(path);
            request.AddHeader("X-TBA-Auth-Key", ConfigurationManager.AppSettings["TBAAuthKey"]);
            var resp = _client.Execute<List<TBATeam>>(request);

            return resp.Data;
        }

        public static List<string> GetTeamEvents(string teamKey)
        {
            string path = $"team/{teamKey}/events/2022/keys";

            var request = new RestRequest(path);
            request.AddHeader("X-TBA-Auth-Key", ConfigurationManager.AppSettings["TBAAuthKey"]);
            var resp = _client.Execute<List<string>>(request);

            return resp.Data;
        }

        public static List<TBAMatchData> GetTeamEventMatches(string teamKey, string eventCode)
        {
            string path = $"team/{teamKey}/event/{eventCode}/matches";

            var request = new RestRequest(path);
            request.AddHeader("X-TBA-Auth-Key", ConfigurationManager.AppSettings["TBAAuthKey"]);
            var resp = _client.Execute<List<TBAMatchData>>(request);

            return resp.Data;
        }
    }
}
