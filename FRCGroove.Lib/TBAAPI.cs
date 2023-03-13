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

        private static Dictionary<string, DateTime> _knownStartTimes = new Dictionary<string, DateTime>()
        {
            {"TXBEL~Qualification", new DateTime(2023, 3, 10, 11, 00, 00, DateTimeKind.Utc)},
            {"TXBEL~Playoff", new DateTime(2023, 3, 11, 13, 00, 00, DateTimeKind.Utc)},
            {"TXCHA~Qualification", new DateTime(2023, 3, 11, 11, 00, 00, DateTimeKind.Utc)},
            {"TXCHA~Playoff", new DateTime(2023, 3, 12, 13, 00, 00, DateTimeKind.Utc)},
            {"TXPAS~Qualification", new DateTime(2022, 3, 25, 11, 00, 00, DateTimeKind.Utc)},
            {"TXPAS~Playoff", new DateTime(2022, 3, 26, 13, 00, 00, DateTimeKind.Utc)},
            {"TXPA2~Qualification", new DateTime(2022, 4, 1, 11, 00, 00, DateTimeKind.Utc)},
            {"TXPA2~Playoff", new DateTime(2022, 4, 2, 13, 00, 00, DateTimeKind.Utc)},
            {"TXCMP~Qualification", new DateTime(2022, 4, 7, 15, 00, 00, DateTimeKind.Utc)},
            {"TXCMP~Playoffs", new DateTime(2022, 4, 9, 12, 30, 00, DateTimeKind.Utc)},
            {"CARVER~Qualification", new DateTime(2022, 4, 21, 8, 30, 00, DateTimeKind.Utc)},
            {"CARVER~Playoff", new DateTime(2022, 4, 23, 8, 30, 00, DateTimeKind.Utc)},
            {"GALILEO~Qualification", new DateTime(2022, 4, 21, 8, 30, 00, DateTimeKind.Utc)},
            {"GALILEO~Playoff", new DateTime(2022, 4, 23, 8, 30, 00, DateTimeKind.Utc)},
            {"HOPPER~Qualification", new DateTime(2022, 4, 21, 8, 30, 00, DateTimeKind.Utc)},
            {"HOPPER~Playoff", new DateTime(2022, 4, 23, 8, 30, 00, DateTimeKind.Utc)},
            {"NEWTON~Qualification", new DateTime(2022, 4, 21, 8, 30, 00, DateTimeKind.Utc)},
            {"NEWTON~Playoff", new DateTime(2022, 4, 23, 8, 30, 00, DateTimeKind.Utc)},
            {"ROEBLING~Qualification", new DateTime(2022, 4, 21, 8, 30, 00, DateTimeKind.Utc)},
            {"ROEBLING~Playoff", new DateTime(2022, 4, 23, 8, 30, 00, DateTimeKind.Utc)},
            {"TURING~Qualification", new DateTime(2022, 4, 21, 8, 30, 00, DateTimeKind.Utc)},
            {"TURING~Playoff", new DateTime(2022, 4, 23, 8, 30, 00, DateTimeKind.Utc)}
        };

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

            List<TBAMatchData> schedule = null;
            if (resp.Data != null)
            {
                schedule = resp.Data.OrderBy(t => t.match_number).ToList();
                AdjustForTimeZone(eventCode, "Qualification", schedule);    
            }

            return resp.Data;
        }

        private static void AdjustForTimeZone(string eventCode, string tournamentLevel, List<TBAMatchData> schedule)
        {
            //checks to see if the scheduled start times are listed inaccurately for the timezone and adjust
            if (schedule.Count > 0 && _knownStartTimes.ContainsKey($"{eventCode}~{tournamentLevel}"))
            {
                DateTime knownStartTime = _knownStartTimes[$"{eventCode}~{tournamentLevel}"];
                if (knownStartTime.Year == DateTime.Now.Year)
                {
                    double delta = (knownStartTime - schedule[0].timeDT).TotalSeconds;
                    if (Math.Abs(delta) > 3000)
                    {
                        foreach (TBAMatchData match in schedule)
                        {
                            match.time = match.time + Convert.ToInt32(delta);
                        }
                    }
                }
            }

            //check each match's actual time - if it's off by > 59 minutes, assume the API is misreporting and adjust as best we can
            if (schedule.Exists(m => m.actual_time > 0))
            {
                foreach (TBAMatchData match in schedule)
                {
                    if (match.actual_time == 0) break;

                    int delta = match.time - match.actual_time;
                    if (Math.Abs(delta) > 3599)
                    {
                        //TODO: Figure out an exact number of hours it is off and adjust by that amount
                        //      Use the previous match's actual time to sus out the state of this one? (for one, it shouldn't be < the last one)
                        match.actual_time = match.actual_time + delta;
                    }
                }
            }
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
            string path = $"team/{teamKey}/events/2023/keys";

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

        public static List<TBAEvent> GetEventListing(int year, string districtCode = "")
        {
            string path = $"events/{year}/simple";

            var request = new RestRequest(path);
            request.AddHeader("X-TBA-Auth-Key", ConfigurationManager.AppSettings["TBAAuthKey"]);
            var resp = _client.Execute<List<TBAEvent>>(request);

            return resp.Data;
        }

        public static TBAEvent GetEvent(string eventCode)
        {
            string path = $"event/{eventCode}/simple";

            var request = new RestRequest(path);
            request.AddHeader("X-TBA-Auth-Key", ConfigurationManager.AppSettings["TBAAuthKey"]);
            var resp = _client.Execute<TBAEvent>(request);

            return resp.Data;
        }

        public static TBAEventRankings GetEventRankings(string eventCode)
        {
            string path = $"event/{eventCode}/rankings";

            var request = new RestRequest(path);
            request.AddHeader("X-TBA-Auth-Key", ConfigurationManager.AppSettings["TBAAuthKey"]);
            var resp = _client.Execute<TBAEventRankings>(request);

            return resp.Data;
        }

        public static List<TBAPlayoffAlliance> GetPlayoffAlliances(string eventCode)
        {
            string path = $"event/{eventCode}/alliances";

            var request = new RestRequest(path);
            request.AddHeader("X-TBA-Auth-Key", ConfigurationManager.AppSettings["TBAAuthKey"]);
            var resp = _client.Execute<List<TBAPlayoffAlliance>>(request);

            return resp.Data;
        }
    }
}
