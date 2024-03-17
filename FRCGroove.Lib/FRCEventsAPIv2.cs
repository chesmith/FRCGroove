﻿using System;
using System.Collections.Generic;
using System.Linq;

using RestSharp;
using RestSharp.Authenticators;

using Newtonsoft.Json;

using FRCGroove.Lib.Models;
using FRCGroove.Lib.Models.FRCv2;
using System.Configuration;
using System.IO;

namespace FRCGroove.Lib
{
    public class FRCEventsAPIv2
    {
        private static readonly RestClient _client = new RestClient("https://frc-api.firstinspires.org/v2.0/" + DateTime.Now.Year)
        {
            Authenticator = new HttpBasicAuthenticator(ConfigurationManager.AppSettings["clientid"], ConfigurationManager.AppSettings["clientsecret"])
        };

        public static string CacheFolder { get; set; }

        public static Dictionary<int, RegisteredTeam> TeamListingCache { get; set; }
        private static List<RegisteredTeam> _champsTeamsCache;

        public static Dictionary<string, string> ChampsDivisions = new Dictionary<string, string>() {
                { "ARPKY", "ARCHIMEDES" },
                { "CPRA", "CURIE" },
                { "DCMP", "DALY" },
                { "GCMP", "GALILEO" },
                { "HCMP", "HOPPER" },
                { "JCMP", "JOHNSON" },
                { "MPCIA", "MILSTEIN" },
                { "NPFCMP", "NEWTON" }
            };

        public static List<RegisteredTeam> GetEventTeamListing(string eventCode)
        {
            string path = $"teams/?eventCode={eventCode}";

            var request = new RestRequest(path);
            var response = _client.Execute<RegisteredTeamListing>(request);

            Log($"GetEventTeamListing-{eventCode}", response.Content);
            if (response.Data != null)
            {
                List<RegisteredTeam> teams = response.Data.teams;
                if( response.Data.teamCountPage > 1)
                {
                    for(int page = 2; page <= response.Data.pageTotal; page++)
                    {
                        var subpageRequest = new RestRequest($"{path}&page={page}");
                        var subpageResponse = _client.Execute<RegisteredTeamListing>(subpageRequest);
                        Log($"GetEventTeamListing-{eventCode}-{page}", subpageResponse.Content);
                        if(subpageResponse.Data != null)
                        {
                            teams.AddRange(subpageResponse.Data.teams);
                        }
                    }
                }
                return teams.OrderBy(t => t.teamNumber).ToList();
            }
            else
                return null;
        }

        public static List<RegisteredTeam> GetChampsTeams()
        {
            if (_champsTeamsCache != null && _champsTeamsCache.Count > 0) return _champsTeamsCache;

            _champsTeamsCache = new List<RegisteredTeam>();
            if (CacheFolder.Length > 0)
            {
                string cachePath = $@"{CacheFolder}\ChampsTeams.{DateTime.Now.Year}.json";
                if (!File.Exists(cachePath))
                {
                    Dictionary<int, string> teamDivisions = new Dictionary<int, string>();
                    foreach (string eventCode in ChampsDivisions.Keys)
                    {
                        string initcap = eventCode.Substring(0, 1) + eventCode.Substring(1).ToLower();
                        List<RegisteredTeam> eventTeams = FRCEventsAPIv2.GetEventTeamListing(eventCode);
                        if (eventTeams != null && eventTeams.Count > 0)
                        {
                            eventTeams.Select(t => { t.champsDivision = ChampsDivisions[eventCode]; return t; }).ToList();
                            _champsTeamsCache.AddRange(eventTeams);
                        }
                    }
                    File.WriteAllText(cachePath, JsonConvert.SerializeObject(_champsTeamsCache));
                }
                else
                {
                    _champsTeamsCache = JsonConvert.DeserializeObject<List<RegisteredTeam>>(File.ReadAllText(cachePath));
                    return _champsTeamsCache;
                }
            }

            return _champsTeamsCache;
        }

        private static void Log(string v, string content)
        {
#if xDEBUG
            StreamWriter sw = new StreamWriter($@"C:\temp\FRCGroove.logs\{v}.{DateTime.Now:yyyy-dd-mm-HH-MM-ss}.json");
            sw.Write(content);
            sw.Close();
#endif
        }

        #region unused code
        //UNUSED
        //Used by home page
        //public static List<District> GetDistrictListing()
        //{
        //    string path = $"districts";

        //    var request = new RestRequest(path);
        //    var response = _client.Execute<DistrictListing>(request);

        //    Log($"GetDistrictListing", response.Content);

        //    List<District> districtListing = response.Data.districts;

        //    return districtListing;
        //}

        //UNUSED
        //Used by home page (teamNumber param not used)
        //public static List<Event> GetEventListing(int teamNumber = 0)
        //{
        //    string path = $"events/";
        //    if (teamNumber > 0) path += $"?teamNumber={teamNumber}";

        //    var request = new RestRequest(path);
        //    var response = _client.Execute<EventListing>(request);

        //    Log($"GetEventListing-{teamNumber}", response.Content);

        //    List<Event> eventListing = null;
        //    if (response.Data != null)
        //        eventListing = response.Data.Events.OrderBy(t => t.dateStart).ToList();

        //    return eventListing;
        //}

        //UNUSED
        //Used by home page (teamNumber param not used)
        //        public static List<Event> GetDistrictEventListing(string districtCode, int teamNumber = 0)
        //        {
        //            string path = $"events/?districtCode={districtCode}";
        //            if (teamNumber > 0) path += $"&teamNumber={teamNumber}";

        //            var request = new RestRequest(path);
        //            var response = _client.Execute<EventListing>(request);

        //            Log($"GetDistrictEventListing-{districtCode},{teamNumber}", response.Content);
        //            List<Event> eventListing = response.Data.Events.OrderBy(t => t.dateStart).ToList();

        //            return eventListing;
        //        }

        //private static Dictionary<string, DateTime> _knownStartTimes = new Dictionary<string, DateTime>()
        //{   {"TXCHA~Qualification", new DateTime(2019, 3, 15, 11, 00, 00, DateTimeKind.Utc)},
        //    {"TXCHA~Playoff", new DateTime(2019, 3, 16, 14, 00, 00, DateTimeKind.Utc)},
        //    {"TXPAS~Qualification", new DateTime(2019, 3, 29, 11, 00, 00, DateTimeKind.Utc)},
        //    {"TXPAS~Playoff", new DateTime(2019, 3, 30, 14, 00, 00, DateTimeKind.Utc)},
        //    {"TXGRE~Qualification", new DateTime(2019, 3, 22, 11, 00, 00, DateTimeKind.Utc)},
        //    {"TXGRE~Playoff", new DateTime(2019, 3, 23, 14, 00, 00, DateTimeKind.Utc)},
        //    {"FTCMP~Qualification", new DateTime(2019,  4, 4, 13, 30, 00, DateTimeKind.Utc)},
        //    {"FTCMP~Playoff", new DateTime(2019, 4, 6, 13, 00, 00, DateTimeKind.Utc)},
        //    {"CARVER~Qualification", new DateTime(2019, 4, 18, 8, 30, 00, DateTimeKind.Utc)},
        //    {"CARVER~Playoff", new DateTime(2019, 4, 20, 9, 30, 00, DateTimeKind.Utc)},
        //    {"GALILEO~Qualification", new DateTime(2019, 4, 18, 8, 30, 00, DateTimeKind.Utc)},
        //    {"GALILEO~Playoff", new DateTime(2019, 4, 20, 9, 30, 00, DateTimeKind.Utc)},
        //    {"HOPPER~Qualification", new DateTime(2019, 4, 18, 8, 30, 00, DateTimeKind.Utc)},
        //    {"HOPPER~Playoff", new DateTime(2019, 4, 20, 9, 30, 00, DateTimeKind.Utc)},
        //    {"NEWTON~Qualification", new DateTime(2019, 4, 18, 8, 30, 00, DateTimeKind.Utc)},
        //    {"NEWTON~Playoff", new DateTime(2019, 4, 20, 9, 30, 00, DateTimeKind.Utc)},
        //    {"ROEBLING~Qualification", new DateTime(2019, 4, 18, 8, 30, 00, DateTimeKind.Utc)},
        //    {"ROEBLING~Playoff", new DateTime(2019, 4, 20, 9, 30, 00, DateTimeKind.Utc)},
        //    {"TURING~Qualification", new DateTime(2019, 4, 18, 8, 30, 00, DateTimeKind.Utc)},
        //    {"TURING~Playoff", new DateTime(2019, 4, 20, 9, 30, 00, DateTimeKind.Utc)}
        //};

        //UNUSED
        //public static List<RegisteredTeam> GetFullTeamListing(/*TODO: int year*/)
        //{
        //    string path = $"teams";

        //    var request = new RestRequest(path);
        //    var response = _client.Execute<RegisteredTeamListing>(request);

        //    //var response = new RestResponse<RegisteredTeamListing>();
        //    //string cachePath = $@"{CacheFolder}\GetFullTeamListing.2019.json";
        //    //if (File.Exists(cachePath))
        //    //{
        //    //    string cachedData = File.ReadAllText(cachePath);
        //    //    response = new RestResponse<RegisteredTeamListing>() { Data = JsonConvert.DeserializeObject<RegisteredTeamListing>(cachedData) };
        //    //}

        //    if (response.Data != null)
        //    {
        //        List<RegisteredTeam> teams = response.Data.teams;
        //        if (response.Data.teamCountPage > 1)
        //        {
        //            for (int page = 2; page <= response.Data.pageTotal; page++)
        //            {
        //                var subpageRequest = new RestRequest($"{path}?page={page}");
        //                var subpageResponse = _client.Execute<RegisteredTeamListing>(subpageRequest);
        //                Log($"GetFullTeamListing-{page}", subpageResponse.Content);
        //                if (subpageResponse.Data != null)
        //                {
        //                    teams.AddRange(subpageResponse.Data.teams);
        //                }
        //            }
        //        }
        //        return teams;
        //    }
        //    else
        //        return null;
        //}

        //UNUSED
        //        public static List<DistrictRank> GetDistrictRankings(string districtCode, int teamNumber = 0)
        //        {
        //            string path = $"rankings/district/";
        //            if (teamNumber > 0)
        //                path += $"?teamNumber={teamNumber}";
        //            else
        //                path += $"{districtCode}";

        //            var request = new RestRequest(path);
        //            var response = _client.Execute<DistrictRankListing>(request);

        //            Log($"GetDistrictRankings-{districtCode},{teamNumber}", response.Content);
        //            List<DistrictRank> districtRankings = response.Data.districtRanks.OrderBy(t => t.rank).ToList();

        //            return districtRankings;
        //        }

        //UNUSED
        //private static Dictionary<string, DateTime> _knownStartTimes = new Dictionary<string, DateTime>()
        //{
        //    {"TXBEL~Qualification", new DateTime(2023, 3, 10, 11, 00, 00, DateTimeKind.Utc)},
        //    {"TXBEL~Playoff", new DateTime(2023, 3, 11, 13, 00, 00, DateTimeKind.Utc)},
        //    {"TXCHA~Qualification", new DateTime(2022, 3, 12, 11, 00, 00, DateTimeKind.Utc)},
        //    {"TXCHA~Playoff", new DateTime(2022, 3, 13, 13, 00, 00, DateTimeKind.Utc)},
        //    {"TXPAS~Qualification", new DateTime(2022, 3, 25, 11, 00, 00, DateTimeKind.Utc)},
        //    {"TXPAS~Playoff", new DateTime(2022, 3, 26, 13, 00, 00, DateTimeKind.Utc)},
        //    {"TXPA2~Qualification", new DateTime(2022, 4, 1, 11, 00, 00, DateTimeKind.Utc)},
        //    {"TXPA2~Playoff", new DateTime(2022, 4, 2, 13, 00, 00, DateTimeKind.Utc)},
        //    {"TXCMP~Qualification", new DateTime(2022, 4, 7, 15, 00, 00, DateTimeKind.Utc)},
        //    {"TXCMP~Playoffs", new DateTime(2022, 4, 9, 12, 30, 00, DateTimeKind.Utc)},
        //    {"CARVER~Qualification", new DateTime(2022, 4, 21, 8, 30, 00, DateTimeKind.Utc)},
        //    {"CARVER~Playoff", new DateTime(2022, 4, 23, 8, 30, 00, DateTimeKind.Utc)},
        //    {"GALILEO~Qualification", new DateTime(2022, 4, 21, 8, 30, 00, DateTimeKind.Utc)},
        //    {"GALILEO~Playoff", new DateTime(2022, 4, 23, 8, 30, 00, DateTimeKind.Utc)},
        //    {"HOPPER~Qualification", new DateTime(2022, 4, 21, 8, 30, 00, DateTimeKind.Utc)},
        //    {"HOPPER~Playoff", new DateTime(2022, 4, 23, 8, 30, 00, DateTimeKind.Utc)},
        //    {"NEWTON~Qualification", new DateTime(2022, 4, 21, 8, 30, 00, DateTimeKind.Utc)},
        //    {"NEWTON~Playoff", new DateTime(2022, 4, 23, 8, 30, 00, DateTimeKind.Utc)},
        //    {"ROEBLING~Qualification", new DateTime(2022, 4, 21, 8, 30, 00, DateTimeKind.Utc)},
        //    {"ROEBLING~Playoff", new DateTime(2022, 4, 23, 8, 30, 00, DateTimeKind.Utc)},
        //    {"TURING~Qualification", new DateTime(2022, 4, 21, 8, 30, 00, DateTimeKind.Utc)},
        //    {"TURING~Playoff", new DateTime(2022, 4, 23, 8, 30, 00, DateTimeKind.Utc)}
        //};

        //UNUSED
        //private static void AdjustForTimeZone(string eventCode, string tournamentLevel, List<Match> schedule)
        //{
        //    //checks to see if the scheduled start times are listed inaccurately for the timezone and adjust
        //    if (schedule.Count > 0 && _knownStartTimes.ContainsKey($"{eventCode}~{tournamentLevel}"))
        //    {
        //        DateTime knownStartTime = _knownStartTimes[$"{eventCode}~{tournamentLevel}"];
        //        if (knownStartTime.Year == DateTime.Now.Year)
        //        {
        //            double delta = (knownStartTime - schedule[0].startTime).TotalMinutes;
        //            if (Math.Abs(delta) > 50)
        //            {
        //                foreach (Match match in schedule)
        //                {
        //                    match.startTime = match.startTime.AddMinutes(delta);
        //                }
        //            }
        //        }
        //    }

        //    //check each match's actual time - if it's off by > 59 minutes, assume the API is misrepoting and adjust to match the scheduled time
        //    if (schedule.Exists(m => m.actualStartTime != null))
        //    {
        //        foreach (Match match in schedule)
        //        {
        //            if (match.actualStartTime == null) break;

        //            double delta = (match.startTime - match.actualStartTime.Value).TotalMinutes;
        //            if (Math.Abs(delta) > 59)
        //            {
        //                    match.actualStartTime = match.actualStartTime.Value.AddMinutes(delta);
        //            }
        //        }
        //    }
        //}

        //UNUSED
        //        public static Event GetEvent(string eventCode)
        //        {
        //            string path = $"events/{eventCode}";
        //            var request = new RestRequest(path);
        //            var response = _client.Execute<EventListing>(request);

        //            Log($"GetEvent-{eventCode}", response.Content);
        //            if (response.Data != null && response.Data.Events.Count > 0)
        //            {
        //                return response.Data.Events[0];
        //            }
        //            else
        //            {
        //                return null;
        //            }
        //        }

        //UNUSED
        //public static List<FRCMatch> GetHybridSchedule(string eventCode, string tournamentLevel)
        //{
        //    string path = $"schedule/{eventCode}/{tournamentLevel}/hybrid";

        //    List<string> _champs = new List<string>() { { "CARVER" }, { "GALILEO" }, { "HOPPER" }, { "NEWTON" }, { "ROEBLING" }, { "TURING" } };
        //    RestRequest request = new RestRequest(path);
        //    RestResponse<ScheduleListing> response = (RestResponse<ScheduleListing>)_client.Execute<ScheduleListing>(request);

        //    if (response.Data != null && response.Data.Schedule.Count == 0 && tournamentLevel == "Qualification" && _champs.Contains(eventCode))
        //    {
        //        string cachePath = $@"{CacheFolder}\GetHybridSchedule.Qualification.{eventCode}.json";
        //        if (File.Exists(cachePath))
        //        {
        //            string cachedData = File.ReadAllText(cachePath);
        //            response = new RestResponse<ScheduleListing>() { Data = JsonConvert.DeserializeObject<ScheduleListing>(cachedData) };
        //        }
        //    }

        //    Log($"GetHybridSchedule-{eventCode},{tournamentLevel}", response.Content);

        //    List<FRCMatch> schedule = null;
        //    if (response.Data != null)
        //    {
        //        schedule = response.Data.Schedule.OrderBy(t => t.matchNumber).ToList();
        //        //AdjustForTimeZone(eventCode, tournamentLevel, schedule);
        //    }

        //    return schedule;
        //}

        //UNUSED
        //public static List<FRCMatch> GetFullHybridSchedule(string eventCode)
        //{
        //    List<FRCMatch> qualifications = GetHybridSchedule(eventCode, "Qualification");
        //    List<FRCMatch> playoffs = GetHybridSchedule(eventCode, "Playoff");

        //    List<FRCMatch> schedule = new List<FRCMatch>();
        //    schedule.AddRange(qualifications);
        //    schedule.AddRange(playoffs);

        //    foreach (FRCMatch match in schedule)
        //    {
        //        match.eventCode = eventCode;
        //    }

        //    return schedule;
        //}

        //        public static List<RegisteredTeam> GetDistrictTeamListing(string districtCode)
        //        {
        //            string path = $"teams/?districtCode={districtCode}";

        //            var request = new RestRequest(path);
        //            var response = _client.Execute<RegisteredTeamListing>(request);

        //            Log($"GetDistrictTeamListing-{districtCode}", response.Content);
        //            List<RegisteredTeam> registeredTeams = response.Data.teams.OrderBy(t => t.teamNumber).ToList();

        //            return registeredTeams;
        //        }

        //UNUSED
        //        public static List<Alliance> GetPlayoffAlliances(string eventCode)
        //        {
        //            string path = $"alliances/{eventCode}";

        //            var request = new RestRequest(path);
        //            var response = _client.Execute<AllianceListing>(request);

        //            Log($"GetPlayoffAlliance-{eventCode}", response.Content);
        //            if (response.Data != null && response.Data.Alliances.Count > 0)
        //                return response.Data.Alliances;
        //            else
        //                return null;
        //        }

        //public static RegisteredTeam GetTeam(int teamNumber)
        //{
        //    string path = $"teams/?teamNumber={teamNumber}";

        //    RegisteredTeam team = null;
        //    if (TeamListingCache != null && TeamListingCache.ContainsKey(teamNumber))
        //    {
        //        team = TeamListingCache[teamNumber];
        //    }

        //    if(team == null)
        //    {
        //        var request = new RestRequest(path);
        //        var response = _client.Execute<RegisteredTeamListing>(request);

        //        Log($"GetTeam-{teamNumber}", response.Content);
        //        if (response.Data != null && response.Data.teams.Count > 0)
        //            team = response.Data.teams[0];
        //    }

        //    return team;
        //}
        #endregion
    }
}