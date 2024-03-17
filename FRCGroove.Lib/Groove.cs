using FRCGroove.Lib.Models;
using FRCGroove.Lib.Models.FRCv2;
using FRCGroove.Lib.Models.Groove;
using FRCGroove.Lib.Models.TBAv3;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace FRCGroove.Lib
{
    public static class Groove
    {
        public static string CacheFolder { get; set; }
        public static Dictionary<int, GrooveTeam> TeamListingCache { get; set; }

        public static List<GrooveDistrict> GetDistricts()
        {
            var tbaDistricts = TBAAPIv3.GetDistrictListing();
            return tbaDistricts.Select(d => new GrooveDistrict(d)).ToList();
        }

        public static List<GrooveEvent> GetDistrictEvents(string districtKey)
        {
            List<TBAEvent> tbaEvents = TBAAPIv3.GetDistrictEventListing(districtKey);
            return tbaEvents.Select(e => new GrooveEvent(e)).ToList();
        }

        public static List<GrooveEvent> GetEvents(int year)
        {
            List<TBAEvent> tbaEvents = TBAAPIv3.GetEventListing(DateTime.Now.Year);
            return tbaEvents.Select(e => new GrooveEvent(e)).ToList();
        }

        public static GrooveEvent GetEvent(string eventKey)
        {
            TBAEvent e = TBAAPIv3.GetEvent(eventKey); //API CALL (6 hour cache)
            return new GrooveEvent(e);
        }

        public static List<GrooveMatch> GetMatches(string eventKey)
        {
            // TODO: fallback to FRC API (based on what?)

            List<TBAMatchData> tbaMatches = TBAAPIv3.GetMatches(eventKey); //API CALL (TBA-compliant cache)
            return tbaMatches.Select(m => new GrooveMatch(m)).ToList();
        }

        public static GrooveTeam GetTeam(int teamNumber)
        {
            // CACHED AT STARTUP
            if (Groove.TeamListingCache != null && Groove.TeamListingCache.ContainsKey(teamNumber))
            {
                return TeamListingCache[teamNumber];
            }
            else
            {
                TBATeam tbaTeam = TBAAPIv3.GetTeam(teamNumber);
                return new GrooveTeam(tbaTeam);
            }
        }

        public static bool DoesTeamListingCacheExist()
        {
            string cachePath = $@"{CacheFolder}\FullTeamListing.{DateTime.Now.Year}.json";
            return File.Exists(cachePath);
        }

        public static void CreateTeamListingCache()
        {
            List<TBATeam> tbaTeams = TBAAPIv3.GetFullTeamListing();

            List<GrooveTeam> teams = tbaTeams.Select(t => new GrooveTeam(t)).ToList();

            string json = JsonConvert.SerializeObject(teams);

            string cachePath = $@"{CacheFolder}\FullTeamListing.{DateTime.Now.Year}.json";
            if (DoesTeamListingCacheExist())
            {
                File.Move(cachePath, $@"{CacheFolder}\FullTeamListing.{DateTime.Now.ToString("yyyy-MM-dd.HH-mm-ss")}.json");
            }
            
            using (StreamWriter sw = new StreamWriter(cachePath))
            {
                sw.Write(json);
                sw.Close();
            }

            LoadTeamListingCache();
        }

        public static void LoadTeamListingCache()
        {
            if (CacheFolder.Length > 0)
            {
                if (DoesTeamListingCacheExist())
                {
                    string cachedData = File.ReadAllText($@"{CacheFolder}\FullTeamListing.{DateTime.Now.Year}.json");
                    List<GrooveTeam> teams = JsonConvert.DeserializeObject<List<GrooveTeam>>(cachedData);
                    TeamListingCache = teams.ToDictionary(t => t.number, t => t);
                }
            }
        }

        public static List<GrooveEventRanking> GetEventRankings(string eventKey)
        {
            TBAEventRankings rankings = TBAAPIv3.GetEventRankings(eventKey); //API CALL (TBA-compliant cache)
            return rankings.rankings.Select(r => new GrooveEventRanking(r)).ToList();
        }

        public static List<GrooveTeam> GetChampsTeams()
        {
            List<RegisteredTeam> frcTeams = FRCEventsAPIv2.GetChampsTeams();
            return frcTeams.Select(t => new GrooveTeam(t)).ToList();
        }

        public static Dictionary<int, string> GetChampsPitLocations()
        {
            RestResponse<List<PitLocation>> response = new RestResponse<List<PitLocation>>();

            string cachePath = $@"{CacheFolder}\pits.{DateTime.Now.Year}.json";
            if (File.Exists(cachePath))
            {
                string cachedData = File.ReadAllText(cachePath);
                response = new RestResponse<List<PitLocation>>() { Data = JsonConvert.DeserializeObject<List<PitLocation>>(cachedData) };
            }

            Dictionary<int, string> pitLocations = null;
            if (response.Data != null)
            {
                pitLocations = response.Data.ToDictionary(t => t.teamNumber, t => t.pitLocation);
            }

            return pitLocations;
        }
    }
}
