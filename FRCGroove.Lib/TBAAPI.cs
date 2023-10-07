﻿using System;
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
using System.IO;
using System.Net.Http.Headers;
using System.Net;
using System.Diagnostics;

namespace FRCGroove.Lib
{
    public static class TBAAPI
    {
        private static readonly RestClient _client = new RestClient("https://www.thebluealliance.com/api/v3");
        private static readonly RestClient _epaClient = new RestClient("https://api.statbotics.io/v2");

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

        public static string CacheFolder { get; set; }
        public static Dictionary<int, EPA> EPACache { get; set; }

        private static Dictionary<string, TBAStatsCollection> _statsCache = new Dictionary<string, TBAStatsCollection>();
        private static Dictionary<string, string> _statsCacheETags = new Dictionary<string, string>();
        private static Dictionary<string, DateTime> _statsCacheAge = new Dictionary<string, DateTime>();

        private static Dictionary<string, TBAEvent> _eventCache = new Dictionary<string, TBAEvent>();
        private static Dictionary<string, DateTime> _eventCacheAge = new Dictionary<string, DateTime>();

        private static Dictionary<string, List<TBAMatchData>> _matchCache = new Dictionary<string, List<TBAMatchData>>();
        private static Dictionary<string, string> _matchCacheETags = new Dictionary<string, string>();
        private static Dictionary<string, DateTime> _matchCacheAge = new Dictionary<string, DateTime>();

        private static Dictionary<string, TBAEventRankings> _eventRankCache = new Dictionary<string,TBAEventRankings>();
        private static Dictionary<string, string> _eventRankCacheETags = new Dictionary<string, string>();
        private static Dictionary<string, DateTime> _eventRankCacheAge = new Dictionary<string, DateTime>();

        private static Dictionary<string, List<TBAPlayoffAlliance>> _playoffAllianceCache = new Dictionary<string, List<TBAPlayoffAlliance>>();
        private static Dictionary<string, DateTime> _playoffAllianceCacheAge = new Dictionary<string, DateTime>();

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

        public static void InitializeEPACache()
        {
            if (CacheFolder.Length > 0)
            {
                string cachePath = $@"{CacheFolder}\EPACache.{DateTime.Now.Year}.json";
                if (!File.Exists(cachePath))
                {
                    EPACache = new Dictionary<int, EPA>();
                    List<EPA> epas = new List<EPA>();
                    int offset = 0;
                    while (true)
                    {
                        var request = new RestRequest($"/team_years?year={DateTime.Now.Year}&limit=100&offset={offset}");
                        var resp = _epaClient.Execute(request);
                        List<EPA> results = JsonConvert.DeserializeObject<List<EPA>>(resp.Content);
                        if (results.Count == 0) break;
                        epas.AddRange(results);
                        offset += 100;
                    }
                    EPACache = epas.ToDictionary(v => v.team, v => v);
                    File.WriteAllText(cachePath, JsonConvert.SerializeObject(EPACache));
                }

                try
                {
                    string cachedData = File.ReadAllText(cachePath);
                    EPACache = JsonConvert.DeserializeObject<Dictionary<int, EPA>>(cachedData);
                }
                catch (Exception) { /* do nothing */ }
            }
        }

        public static void ResetEPACache()
        {
            string cachePath = $@"{CacheFolder}\EPACache.{DateTime.Now.Year}.json";
            if (File.Exists(cachePath))
            {
                File.Move(cachePath, $@"{CacheFolder}\EPACache.{DateTime.Now.ToString("yyyy-MM-dd.HH-mm-ss")}.json");
            }

            InitializeEPACache();
        }

        public static TBAStatsCollection GetStats(string eventCode)
        {
            if (_statsCache.ContainsKey(eventCode) && _statsCacheAge[eventCode].AddMinutes(5) > DateTime.Now)
                return _statsCache[eventCode];

            string path = $"event/{eventCode}/oprs";

            var request = new RestRequest(path);
            request.AddHeader("X-TBA-Auth-Key", ConfigurationManager.AppSettings["TBAAuthKey"]);
            if (_statsCacheETags.ContainsKey(eventCode))
                request.AddHeader("If-None-Match", _matchCacheETags[eventCode]);
            var resp = _client.Execute(request);

            TBAStatsCollection stats = null;
            if (resp.StatusCode != HttpStatusCode.NotModified || (resp.Content != "null" && resp.Content.Length > 5))
            {
                if (resp.Headers.Any(t => t.Name == "ETag"))
                    _matchCacheETags[eventCode] = resp.Headers.FirstOrDefault(t => t.Name == "ETag").Value.ToString();

                JObject j = JObject.Parse(resp.Content);
                stats = new TBAStatsCollection(j);

                _statsCache[eventCode] = stats;
                _statsCacheAge[eventCode] = DateTime.Now;
            }
            else if (_statsCache.ContainsKey(eventCode))
            {
                _statsCacheAge[eventCode] = DateTime.Now;
                return _statsCache[eventCode];
            }

            return stats;
        }

        public static List<TBAMatchData> GetMatches(string eventCode)
        {
            if (_matchCacheAge.ContainsKey(eventCode) && _matchCacheAge[eventCode] > DateTime.Now && _matchCache.ContainsKey(eventCode))
            {
                Debug.WriteLine($"{DateTime.Now:s} GetMatches - pull from cache due to Cache-Control not yet expired: {_matchCacheAge[eventCode]:s}");
                return _matchCache[eventCode];
            }

            string path = $"event/{eventCode}/matches";

            var request = new RestRequest(path);
            request.AddHeader("X-TBA-Auth-Key", ConfigurationManager.AppSettings["TBAAuthKey"]);
            if(_matchCacheETags.ContainsKey(eventCode))
                request.AddHeader("If-None-Match", _matchCacheETags[eventCode]);
            var resp = _client.Execute<List<TBAMatchData>>(request);

            double cacheExpire = 0.0;
            if (resp.Headers.Any(t => t.Name == "Cache-Control"))
            {
                string val = resp.Headers.FirstOrDefault(t => t.Name == "Cache-Control").Value.ToString();
                CacheControlHeaderValue cacheControl;
                if (CacheControlHeaderValue.TryParse(val, out cacheControl))
                {
                    if(cacheControl.MaxAge != null)
                        cacheExpire = cacheControl.MaxAge.Value.TotalSeconds;
                }
            }

            if (resp.StatusCode != HttpStatusCode.NotModified || resp.Data != null)
            {
                Debug.WriteLine($"{DateTime.Now:s} GetMatches - Pull full data and store in cache; max-age: {cacheExpire}");

                List<TBAMatchData> schedule = resp.Data;

                if (resp.Headers.Any(t => t.Name == "ETag"))
                    _matchCacheETags[eventCode] = resp.Headers.FirstOrDefault(t => t.Name == "ETag").Value.ToString();

                if (schedule.Count > 1)
                {
                    schedule = resp.Data.OrderBy(t => t.match_number).ToList();
                    AdjustForTimeZone(eventCode, "Qualification", schedule);

                    // enrich with prediction data
                    Dictionary<int, EPA> epas = TBAAPI.EPACache;
                    foreach (TBAMatchData match in schedule)
                    {
                        match.alliances.red.predictedPoints = Convert.ToInt32(epas.Where(t => match.alliances.red.team_keys.Contains("frc" + t.Key.ToString())).Sum(e => e.Value.epa_end));
                        match.alliances.blue.predictedPoints = Convert.ToInt32(epas.Where(t => match.alliances.blue.team_keys.Contains("frc" + t.Key.ToString())).Sum(e => e.Value.epa_end));
                    }
                }

                _matchCache[eventCode] = schedule;
                _matchCacheAge[eventCode] = DateTime.Now.AddSeconds(cacheExpire);

                return schedule;
            }
            else if (_matchCache.ContainsKey(eventCode))
            {
                Debug.WriteLine($"{DateTime.Now:s} GetMatches - API had no update so retrieve data from cache and update Cache-Control expiration: {_matchCacheAge[eventCode]}");

                _matchCacheAge[eventCode] = DateTime.Now.AddSeconds(cacheExpire);
                return _matchCache[eventCode];
            }
            else
            {
                Debug.WriteLine($"{DateTime.Now:s} Error - no update or data from API and nothing in cache");
                return null;
            }
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
            //            //TODO: Figure out an exact number of hours it is off and adjust by that amount
            //            //      Use the previous match's actual time to sus out the state of this one? (for one, it shouldn't be < the last one)
            //            match.actual_time = match.actual_time + delta;
            //        }
            //    }
            //}
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
            if (_eventCache.ContainsKey(eventCode) && _eventCacheAge[eventCode].AddHours(6) > DateTime.Now)
                return _eventCache[eventCode];

            string path = $"event/{eventCode}/simple";

            var request = new RestRequest(path);
            request.AddHeader("X-TBA-Auth-Key", ConfigurationManager.AppSettings["TBAAuthKey"]);
            var resp = _client.Execute<TBAEvent>(request);

            _eventCache[eventCode] = resp.Data;
            _eventCacheAge[eventCode] = DateTime.Now;

            return resp.Data;
        }

        public static TBAEventRankings GetEventRankings(string eventCode)
        {
            if (_eventRankCacheAge.ContainsKey(eventCode) && _eventRankCacheAge[eventCode] > DateTime.Now && _eventRankCache.ContainsKey(eventCode))
            {
                Debug.WriteLine($"{DateTime.Now:s} GetStats - pull from cache due to Cache-Control not yet expired: {_matchCacheAge[eventCode]:s}");
                return _eventRankCache[eventCode];
            }

            string path = $"event/{eventCode}/rankings";

            var request = new RestRequest(path);
            request.AddHeader("X-TBA-Auth-Key", ConfigurationManager.AppSettings["TBAAuthKey"]);
            if (_eventRankCacheETags.ContainsKey(eventCode))
                request.AddHeader("If-None-Match", _eventRankCacheETags[eventCode]);
            var resp = _client.Execute<TBAEventRankings>(request);

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
                Debug.WriteLine($"{DateTime.Now:s} GetStats - Pull full data and store in cache; max-age: {cacheExpire}");

                if (resp.Headers.Any(t => t.Name == "ETag"))
                    _eventRankCacheETags[eventCode] = resp.Headers.FirstOrDefault(t => t.Name == "ETag").Value.ToString();

                _eventRankCache[eventCode] = resp.Data;
                _eventRankCacheAge[eventCode] = DateTime.Now.AddSeconds(cacheExpire);

                return resp.Data;
            }
            else if (_eventRankCache.ContainsKey(eventCode))
            {
                Debug.WriteLine($"{DateTime.Now:s} GetStats - API had no update so retrieve data from cache and update Cache-Control expiration: {_matchCacheAge[eventCode]}");

                _eventRankCacheAge[eventCode] = DateTime.Now.AddSeconds(cacheExpire);
                return _eventRankCache[eventCode];
            }
            else
                return null;
        }

        public static List<TBAPlayoffAlliance> GetPlayoffAlliances(string eventCode)
        {
            if (_playoffAllianceCache.ContainsKey(eventCode) && _playoffAllianceCacheAge[eventCode].AddMinutes(5) > DateTime.Now)
                return _playoffAllianceCache[eventCode];

            string path = $"event/{eventCode}/alliances";

            var request = new RestRequest(path);
            request.AddHeader("X-TBA-Auth-Key", ConfigurationManager.AppSettings["TBAAuthKey"]);
            var resp = _client.Execute<List<TBAPlayoffAlliance>>(request);

            if (resp != null && resp.Data != null && resp.Data.Count > 0)
            {
                _playoffAllianceCache[eventCode] = resp.Data;
                _playoffAllianceCacheAge[eventCode] = DateTime.Now;
            }

            return resp.Data;
        }

        public static List<TBADistrict> GetDistrictListing()
        {
            string path = $"districts/{DateTime.Now.Year}";

            var request = new RestRequest(path);
            request.AddHeader("X-TBA-Auth-Key", ConfigurationManager.AppSettings["TBAAuthKey"]);
            var resp = _client.Execute<List<TBADistrict>>(request);

            return resp.Data;
        }
    }
}
