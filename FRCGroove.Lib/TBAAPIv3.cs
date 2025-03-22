using System;
using System.Collections.Generic;
using System.Linq;

using RestSharp;

using FRCGroove.Lib.Models.TBAv3;
using FRCGroove.Lib.Models.Statbotics;

using System.Configuration;
using System.Net.Http.Headers;
using System.Net;
using System.Diagnostics;
using System.Text.Json;

namespace FRCGroove.Lib
{
    public static class TBAAPIv3
    {
        private static readonly RestClient _client = new RestClient("https://www.thebluealliance.com/api/v3");

        private static readonly Cache<TBAStatsCollection> _statsCache = new Cache<TBAStatsCollection>();
        private static readonly Cache<TBAEvent> _eventCache = new Cache<TBAEvent>();
        private static readonly Cache<List<TBAMatchData>> _matchCache = new Cache<List<TBAMatchData>>();
        private static readonly Cache<TBAEventRankings> _eventRankCache = new Cache<TBAEventRankings>();
        private static readonly Cache<List<TBATeam>> _eventTeamsCache = new Cache<List<TBATeam>>();
        private static readonly Cache<List<TBAEvent>> _teamEventsCache = new Cache<List<TBAEvent>>();
        private static readonly Cache<TBATeamEventStatus> _teamEventStatusCache = new Cache<TBATeamEventStatus>();
        private static readonly Cache<List<TBAPlayoffAlliance>> _playoffAllianceCache = new Cache<List<TBAPlayoffAlliance>>();

        private static readonly Dictionary<string, string> _champsEvents = new Dictionary<string, string>()
        {
            { "ARPKY", "2023arc" },
            { "ARCHIMEDES", "2023arc" },
            { "CPRA", "2023cur" },
            { "CURIE", "2023cur" },
            { "DCMP", "2023dal" },
            { "DALY", "2023dal" },
            { "GCMP", "2023gal" },
            { "GALILEO", "2023gal" },
            { "HCMP", "2023hop" },
            { "HOPPER", "2023hop" },
            { "JCMP", "2023joh" },
            { "JOHNSON", "2023joh" },
            { "MPCIA", "2023mil" },
            { "MILSTEIN", "2023mil" },
            { "NPFCMP", "2023new" },
            { "NEWTON", "2023new" }
        };

        public static string TranslateFRCEventCode(string eventCode)
        {
            if (_champsEvents.Keys.Contains(eventCode))
                return _champsEvents[eventCode];
            else
                return "2023" + eventCode.ToLower();
        }

        private static Dictionary<string, DateTime> _knownStartTimes = new Dictionary<string, DateTime>()
        {
            {"2023txbel~Qualification", new DateTime(2023, 3, 10, 11, 00, 00, DateTimeKind.Utc)},
            {"2023txbel~Playoff", new DateTime(2023, 3, 11, 13, 00, 00, DateTimeKind.Utc)},
            {"2023txcha~Qualification", new DateTime(2023, 3, 11, 11, 00, 00, DateTimeKind.Utc)},
            {"2023txcha~Playoff", new DateTime(2023, 3, 12, 13, 00, 00, DateTimeKind.Utc)},
            {"2023txcmp1~Qualification", new DateTime(2023, 4, 6, 15, 00, 00, DateTimeKind.Utc)},
            {"2023txcmp1~Playoff", new DateTime(2023, 4, 6, 12, 30, 00, DateTimeKind.Utc)},
            {"2023txcmp2~Qualification", new DateTime(2023, 4, 6, 15, 00, 00, DateTimeKind.Utc)},
            {"2023txcmp2~Playoff", new DateTime(2023, 4, 6, 12, 30, 00, DateTimeKind.Utc)},
            {"2023arc~Qualification", new DateTime(2022, 4, 21, 8, 30, 00, DateTimeKind.Utc)},
            {"2023arc~Playoff", new DateTime(2022, 4, 23, 8, 30, 00, DateTimeKind.Utc)},
            {"2023cur~Qualification", new DateTime(2022, 4, 21, 8, 30, 00, DateTimeKind.Utc)},
            {"2023cur~Playoff", new DateTime(2022, 4, 23, 8, 30, 00, DateTimeKind.Utc)},
            {"2023dal~Qualification", new DateTime(2022, 4, 21, 8, 30, 00, DateTimeKind.Utc)},
            {"2023dal~Playoff", new DateTime(2022, 4, 23, 8, 30, 00, DateTimeKind.Utc)},
            {"2023gal~Qualification", new DateTime(2022, 4, 21, 8, 30, 00, DateTimeKind.Utc)},
            {"2023gal~Playoff", new DateTime(2022, 4, 23, 8, 30, 00, DateTimeKind.Utc)},
            {"2023joh~Qualification", new DateTime(2022, 4, 21, 8, 30, 00, DateTimeKind.Utc)},
            {"2023joh~Playoff", new DateTime(2022, 4, 23, 8, 30, 00, DateTimeKind.Utc)},
            {"2023mil~Qualification", new DateTime(2022, 4, 21, 8, 30, 00, DateTimeKind.Utc)},
            {"2023mil~Playoff", new DateTime(2022, 4, 23, 8, 30, 00, DateTimeKind.Utc)},
            {"2023new~Qualification", new DateTime(2022, 4, 21, 8, 30, 00, DateTimeKind.Utc)},
            {"2023new~Playoff", new DateTime(2022, 4, 23, 8, 30, 00, DateTimeKind.Utc)},
        };

        public static TBAStatsCollection GetStats(string eventCode)
        {
            return GetFromCacheOrApi(_statsCache, eventCode, $"event/{eventCode}/oprs", resp =>
            {
                if (resp.StatusCode != HttpStatusCode.NotModified && !resp.Content.StartsWith("null") && resp.Content.Length > 5)
                {
                    using (JsonDocument doc = JsonDocument.Parse(resp.Content))
                    {
                        return new TBAStatsCollection(doc);
                    }
                }
                return null;
            });
        }

        public static List<TBAMatchData> GetMatches(string eventCode)
        {
            return GetFromCacheOrApi(_matchCache, eventCode, $"event/{eventCode}/matches", resp =>
            {
                var schedule = resp.Data;
                if (schedule != null && schedule.Count > 1)
                {
                    schedule = schedule.OrderBy(t => t.match_number).ToList();
                    AdjustForTimeZone(eventCode, "Qualification", schedule);

                    // enrich with prediction data
                    Dictionary<int, Statbotics_v3> epas = StatboticsAPI.EPACache;
                    foreach (TBAMatchData match in schedule)
                    {
                        match.alliances.red.predictedPoints = Convert.ToInt32(epas.Where(t => match.alliances.red.team_keys.Contains("frc" + t.Key.ToString())).Sum(e => e.Value.epa.total_points.mean));
                        match.alliances.blue.predictedPoints = Convert.ToInt32(epas.Where(t => match.alliances.blue.team_keys.Contains("frc" + t.Key.ToString())).Sum(e => e.Value.epa.total_points.mean));
                    }
                }
                return schedule;
            });
        }

        private static void AdjustForTimeZone(string eventCode, string tournamentLevel, List<TBAMatchData> schedule)
        {
            //2023-04-15 tbh this is causing more problems than it's worth - I think we're just going to assume the API times are kosher

            //checks to see if the scheduled start times are listed inaccurately for the timezone and adjust
            //if (schedule.Count > 0 && _knownStartTimes.ContainsKey($"{eventCode}~{tournamentLevel}"))
            //{
            //    DateTime knownStartTime = _knownStartTimes[$"{eventCode}~{tournamentLevel}"];
            //    if (knownStartTime.Year == DateTime.Now.Year)
            //    {
            //        double delta = (knownStartTime - schedule[0].timeDT).TotalSeconds;
            //        if (Math.Abs(delta) > 3000)
            //        {
            //            foreach (TBAMatchData match in schedule)
            //            {
            //                match.time = match.time + Convert.ToInt32(delta);
            //            }
            //        }
            //    }
            //}

            ////check each match's actual time - if it's off by > 59 minutes, assume the API is misreporting and adjust as best we can
            //if (schedule.Exists(m => m.actual_time > 0))
            //{
            //    foreach (TBAMatchData match in schedule)
            //    {
            //        if (match.actual_time == 0) break;

            //        int delta = match.time - match.actual_time;
            //        if (Math.Abs(delta) > 3599)
            //        {
            //            //LEGACY TODO: Figure out an exact number of hours it is off and adjust by that amount
            //            //      Use the previous match's actual time to sus out the state of this one? (for one, it shouldn't be < the last one)
            //            match.actual_time = match.actual_time + delta;
            //        }
            //    }
            //}
        }

        public static List<TBAEvent> GetDistrictEventListing(string districtCode)
        {
            string path = $"district/{districtCode}/events/simple";

            var request = new RestRequest(path);
            request.AddHeader("X-TBA-Auth-Key", ConfigurationManager.AppSettings["TBAAuthKey"]);
            var resp = _client.Execute<List<TBAEvent>>(request);

            return resp.Data;
        }

        public static List<TBAEvent> GetEventListing(int year)
        {
            string path = $"events/{year}/simple";

            var request = new RestRequest(path);
            request.AddHeader("X-TBA-Auth-Key", ConfigurationManager.AppSettings["TBAAuthKey"]);
            var resp = _client.Execute<List<TBAEvent>>(request);

            return resp.Data;
        }

        public static TBAEvent GetEvent(string eventCode)
        {
            return GetFromCacheOrApi(_eventCache, eventCode, $"event/{eventCode}/simple");
        }

        public static TBAEventRankings GetEventRankings(string eventCode)
        {
            return GetFromCacheOrApi(_eventRankCache, eventCode, $"event/{eventCode}/rankings");
        }

        public static List<TBAPlayoffAlliance> GetPlayoffAlliances(string eventCode)
        {
            return GetFromCacheOrApi(_playoffAllianceCache, eventCode, $"event/{eventCode}/alliances");
        }

        public static List<TBADistrict> GetDistrictListing()
        {
            string path = $"districts/{DateTime.Now.Year}";

            var request = new RestRequest(path);
            request.AddHeader("X-TBA-Auth-Key", ConfigurationManager.AppSettings["TBAAuthKey"]);
            var resp = _client.Execute<List<TBADistrict>>(request);

            return resp.Data;
        }

        public static List<TBATeam> GetFullTeamListing()
        {
            string path = $"teams/{DateTime.Now.Year}/0/simple";

            var request = new RestRequest(path);
            request.AddHeader("X-TBA-Auth-Key", ConfigurationManager.AppSettings["TBAAuthKey"]);
            var response = _client.Execute<List<TBATeam>>(request);

            if (response.Data != null)
            {
                List<TBATeam> teams = response.Data;
                List<TBATeam> allTeams = new List<TBATeam>();
                allTeams.AddRange(teams);
                int page = 0;
                while (teams.Count > 0)
                {
                    page++;
                    var subpageRequest = new RestRequest($"teams/{DateTime.Now.Year}/{page}/simple");
                    subpageRequest.AddHeader("X-TBA-Auth-Key", ConfigurationManager.AppSettings["TBAAuthKey"]);
                    var subpageResponse = _client.Execute<List<TBATeam>>(subpageRequest);
                    if (subpageResponse.Data != null)
                    {
                        teams = subpageResponse.Data;
                        allTeams.AddRange(teams);
                    }
                }
                return allTeams;
            }
            else
                return null;
        }

        public static TBATeam GetTeam(int teamNumber)
        {
            TBATeam team = null;
            string path = $"team/frc{teamNumber}";
            var request = new RestRequest(path);
            var response = _client.Execute<TBATeam>(request);

            if (response.StatusCode == HttpStatusCode.OK && response.Data != null)
                team = response.Data;

            return team;
        }

        public static List<TBATeam> GetEventTeams(string eventCode)
        {
            return GetFromCacheOrApi(_eventTeamsCache, eventCode, $"event/{eventCode}/teams/simple");
        }

        public static List<TBAEvent> GetTeamEvents(int teamNumber, int year)
        {
            return GetFromCacheOrApi(_teamEventsCache, $"{teamNumber}~{year}", $"team/frc{teamNumber}/events/{year}");
        }

        public static TBATeamEventStatus GetTeamEventStatus(int teamNumber, string eventKey)
        {
            return GetFromCacheOrApi(_teamEventStatusCache, $"{teamNumber}~{eventKey}", $"team/frc{teamNumber}/event/{eventKey}/status");
        }

        private static T GetFromCacheOrApi<T>(Cache<T> cache, string cacheKey, string requestPath, Func<RestResponse<T>, T> customProcessor = null) where T : class
        {
            var callingMethod = new StackFrame(1, true).GetMethod().Name;
            var cacheItem = cache.Get(cacheKey);
            if (cacheItem != null && cacheItem.Expiration > DateTime.Now)
            {
                Debug.WriteLine($"{DateTime.Now:s} {callingMethod} ({cacheKey}) - pull from cache due to Cache-Control not yet expired: {cacheItem.Expiration}");
                return cacheItem.Data;
            }

            var request = new RestRequest(requestPath);
            request.AddHeader("X-TBA-Auth-Key", ConfigurationManager.AppSettings["TBAAuthKey"]);
            if (cacheItem != null && !string.IsNullOrEmpty(cacheItem.ETag))
                request.AddHeader("If-None-Match", cacheItem.ETag);
            var resp = _client.Execute<T>(request);

            double cacheExpire = 0.0;
            if (resp.Headers.Any(t => t.Name == "Cache-Control"))
            {
                string val = resp.Headers.FirstOrDefault(t => t.Name == "Cache-Control").Value.ToString();
                CacheControlHeaderValue cacheControl;
                if (CacheControlHeaderValue.TryParse(val, out cacheControl))
                {
                    if (cacheControl.MaxAge != null)
                        cacheExpire = cacheControl.MaxAge.Value.TotalSeconds;
                }
            }

            if (resp.StatusCode != HttpStatusCode.NotModified || resp.Data != null)
            {
                Debug.WriteLine($"{DateTime.Now:s} {callingMethod} ({cacheKey}) - Pull full data and store in cache; max-age: {cacheExpire}");

                string eTag = resp.Headers.FirstOrDefault(t => t.Name == "ETag")?.Value.ToString();
                T data = customProcessor != null ? customProcessor(resp) : resp.Data;
                cache.Set(cacheKey, data, eTag, DateTime.Now.AddSeconds(cacheExpire));

                return data;
            }
            else if (resp.StatusCode == HttpStatusCode.NotModified && cacheItem != null)
            {
                DateTime cacheExpirationTime = DateTime.Now.AddSeconds(cacheExpire);

                Debug.WriteLine($"{DateTime.Now:s} {callingMethod} ({cacheKey}) - API had no update so retrieve data from cache and update expiration: {cacheExpirationTime}");

                cache.Set(cacheKey, cacheItem.Data, cacheItem.ETag, cacheExpirationTime);
                return cacheItem.Data;
            }
            else
            {
                Debug.WriteLine($"{DateTime.Now:s} {callingMethod} ({cacheKey}) - no update or data from API and nothing in cache");
                return null;
            }
        }
    }
}
