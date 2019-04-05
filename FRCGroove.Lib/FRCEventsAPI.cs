#define xMOCK

using System;
using System.Collections.Generic;
using System.Linq;

using RestSharp;
using RestSharp.Authenticators;

using Newtonsoft.Json;

using FRCGroove.Lib.models;
using System.Configuration;

namespace FRCGroove.Lib
{
    public class FRCEventsAPI
    {
        private static RestClient _client = new RestClient("https://frc-api.firstinspires.org/v2.0/2019")
        {
            Authenticator = new HttpBasicAuthenticator(ConfigurationManager.AppSettings["clientid"], ConfigurationManager.AppSettings["clientsecret"])
        };

        private static Dictionary<string, DateTime> _knownStartTimes = new Dictionary<string, DateTime>()
        {   {"TXCHA~Qualification", new DateTime(2019, 3, 15, 11, 00, 00, DateTimeKind.Utc)},
            {"TXCHA~Playoff", new DateTime(2019, 3, 16, 14, 00, 00, DateTimeKind.Utc)},
            {"TXPAS~Qualification", new DateTime(2019, 3, 29, 11, 00, 00, DateTimeKind.Utc)},
            {"TXPAS~Playoff", new DateTime(2019, 3, 30, 14, 00, 00, DateTimeKind.Utc)},
            {"TXGRE~Qualification", new DateTime(2019, 3, 22, 11, 00, 00, DateTimeKind.Utc)},
            {"TXGRE~Playoff", new DateTime(2019, 3, 23, 14, 00, 00, DateTimeKind.Utc)},
            {"FTCMP~Qualification", new DateTime(2019,  4, 4, 13, 30, 00, DateTimeKind.Utc)},
            {"FTCMP~Playoff", new DateTime(2019, 4, 6, 13, 00, 00, DateTimeKind.Utc)},
        };

        public static List<District> GetDistrictListing()
        {
            string path = $"districts";

            var request = new RestRequest(path);
            var response = _client.Execute<DistrictListing>(request);

            Log($"GetDistrictListing", response.Content);

            List<District> districtListing = response.Data.districts;

            return districtListing;
        }

        public static List<Event> GetEventListing(int teamNumber = 0)
        {
            string path = $"events/";
            if (teamNumber > 0) path += $"?teamNumber={teamNumber}";

            var request = new RestRequest(path);
            var response = _client.Execute<EventListing>(request);

            Log($"GetEventListing-{teamNumber}", response.Content);

            List<Event> eventListing = response.Data.Events.OrderBy(t => t.dateStart).ToList();

            return eventListing;
        }

        public static List<Event> GetDistrictEventListing(string districtCode, int teamNumber = 0)
        {
            string path = $"events/?districtCode={districtCode}";
            if (teamNumber > 0) path += $"&teamNumber={teamNumber}";

#if MOCK
            string mockInput = System.IO.File.ReadAllText(@"C:\temp\GetEvent.mock.json");
            var response = new { Data = JsonConvert.DeserializeObject<EventListing>(mockInput) };
#else
            var request = new RestRequest(path);
            var response = _client.Execute<EventListing>(request);

            Log($"GetDistrictEventListing-{districtCode},{teamNumber}", response.Content);
#endif
            List<Event> eventListing = response.Data.Events.OrderBy(t => t.dateStart).ToList();

            return eventListing;
        }

        public static Event GetEvent(string eventCode)
        {
            string path = $"events/{eventCode}";
#if MOCK
            string mockInput = System.IO.File.ReadAllText(@"C:\temp\GetEvent.mock.json");
            var response = new { Data = JsonConvert.DeserializeObject<EventListing>(mockInput) };
#else
            var request = new RestRequest(path);
            var response = _client.Execute<EventListing>(request);

            Log($"GetEvent-{eventCode}", response.Content);
#endif
            if (response.Data != null && response.Data.Events.Count > 0)
            {
                return response.Data.Events[0];
            }
            else
            {
                return null;
            }
        }

        public static List<Match> GetEventSchedule(string eventCode, int teamNumber = 0)
        {
            string path = $"schedule/{eventCode}";
            if (teamNumber > 0) path += $"?teamNumber={teamNumber}";

            var request = new RestRequest(path);
            var response = _client.Execute<ScheduleListing>(request);

            Log($"GetEventSchedule-{eventCode},{teamNumber}", response.Content);

            List<Match> schedule = response.Data.Schedule.OrderByDescending(t => t.tournamentLevel).ThenBy(t => t.matchNumber).ToList();

            //TODO: AdjustForTimeZone(eventCode, schedule);

            return schedule;
        }

        public static List<Match> GetHybridSchedule(string eventCode, string tournamentLevel)
        {
            string path = $"schedule/{eventCode}/{tournamentLevel}/hybrid";

#if MOCK
            string mockInput;
            if(tournamentLevel == "Qualification")
                mockInput = System.IO.File.ReadAllText(@"C:\temp\GetHybridSchedule.Qualification.mock.json");
            else
                mockInput = System.IO.File.ReadAllText(@"C:\temp\GetHybridSchedule.Playoff.mock.json");

            var response = new { Data = JsonConvert.DeserializeObject<ScheduleListing>(mockInput) };
#else
            var request = new RestRequest(path);
            var response = _client.Execute<ScheduleListing>(request);

            Log($"GetHybridSchedule-{eventCode},{tournamentLevel}", response.Content);
#endif
            List<Match> schedule = response.Data.Schedule.OrderBy(t => t.matchNumber).ToList();

            AdjustForTimeZone(eventCode, tournamentLevel, schedule);

            return schedule;
        }

        public static List<Match> GetHybridSchedule_FromFile(string path)
        {
            string mockInput = System.IO.File.ReadAllText(path);

            var response = new { Data = JsonConvert.DeserializeObject<ScheduleListing>(mockInput) };
            List<Match> schedule = response.Data.Schedule.OrderBy(t => t.matchNumber).ToList();

            return schedule;
        }

        public static List<Match> GetFullHybridSchedule(string eventCode)
        {
            List<Match> qualifications = GetHybridSchedule(eventCode, "Qualification");
            List<Match> playoffs = GetHybridSchedule(eventCode, "Playoff");

            List<Match> schedule = new List<Match>();
            schedule.AddRange(qualifications);
            schedule.AddRange(playoffs);

            foreach(Match match in schedule)
            {
                match.eventCode = eventCode;
            }

            return schedule;
        }

        private static void AdjustForTimeZone(string eventCode, string tournamentLevel, List<Match> schedule)
        {
            //checks to see if the start times are listed inaccurately for the timezone and adjust
            if (schedule.Count > 0 && _knownStartTimes.ContainsKey($"{eventCode}~{tournamentLevel}"))
            {
                DateTime knownStartTime = _knownStartTimes[$"{eventCode}~{tournamentLevel}"];
                double delta = (knownStartTime - schedule[0].startTime).TotalMinutes;
                if (Math.Abs(delta) > 50)
                {
                    foreach (Match match in schedule)
                    {
                        match.startTime = match.startTime.AddMinutes(delta);
                    }
                }
            }

            //TODO: check the first match's actual time - if it's off by > 50 minutes, assume the timezone is messed up and adjust to match the scheduled time
            if (schedule.Exists(m => m.actualStartTime != null))
            {
                Match firstMatch = schedule[0];
                double delta = (schedule[0].startTime - schedule[0].actualStartTime.Value).TotalMinutes;
                if (Math.Abs(delta) > 50)
                {
                    foreach(Match match in schedule)
                    {
                        if (match.actualStartTime == null) break;

                        match.actualStartTime = match.actualStartTime.Value.AddMinutes(delta);
                    }
                }
            }
        }

        public static List<EventRanking> GetEventRankings(string eventCode, int teamNumber = 0)
        {
            string path = $"rankings/{eventCode}";
            if (teamNumber > 0) path += $"?teamNumber={teamNumber}";

#if MOCK
            string mockInput = System.IO.File.ReadAllText(@"C:\temp\GetEventRanking.mock.json");
            var response = new { Data = JsonConvert.DeserializeObject<EventRankListing>(mockInput) };
#else
            var request = new RestRequest(path);
            var response = _client.Execute<EventRankListing>(request);

            Log($"GetEventRanking-{eventCode},{teamNumber}", response.Content);
#endif
            List<EventRanking> eventRankings = response.Data.Rankings.OrderBy(t => t.rank).ToList();

            return eventRankings;
        }

        public static List<DistrictRank> GetDistrictRankings(string districtCode, int teamNumber = 0)
        {
            string path = $"rankings/district/";
            if (teamNumber > 0)
                path += $"?teamNumber={teamNumber}";
            else
                path += $"{districtCode}";

#if MOCK
            string mockInput = System.IO.File.ReadAllText(@"C:\temp\GetDistrictRankings.mock.json");
            var response = new { Data = JsonConvert.DeserializeObject<DistrictRankListing>(mockInput) };
#else
            var request = new RestRequest(path);
            var response = _client.Execute<DistrictRankListing>(request);

            Log($"GetDistrictRankings-{districtCode},{teamNumber}", response.Content);
#endif
            List<DistrictRank> districtRankings = response.Data.districtRanks.OrderBy(t => t.rank).ToList();

            return districtRankings;
        }

        public static List<RegisteredTeam> GetTeamListing(string districtCode)
        {
            string path = $"teams/?districtCode={districtCode}";

#if MOCK
            string mockInput = System.IO.File.ReadAllText(@"C:\temp\GetDistrictRankings.mock.json");
            var response = new { Data = JsonConvert.DeserializeObject<RegisteredTeamListing>(mockInput) };
#else
            var request = new RestRequest(path);
            var response = _client.Execute<RegisteredTeamListing>(request);

            Log($"GetTeamListing-{districtCode}", response.Content);
#endif
            List<RegisteredTeam> registeredTeams = response.Data.teams.OrderBy(t => t.number).ToList();

            return registeredTeams;
        }

        public static RegisteredTeam GetTeam(int teamNumber)
        {
            string path = $"teams/?teamNumber={teamNumber}";

#if MOCK
            string mockInput = System.IO.File.ReadAllText(@"C:\temp\GetTeam.mock.json");
            var response = new { Data = JsonConvert.DeserializeObject<RegisteredTeamListing>(mockInput) };
#else
            var request = new RestRequest(path);
            var response = _client.Execute<RegisteredTeamListing>(request);

            Log($"GetTeam-{teamNumber}", response.Content);
#endif
            if (response.Data !=null && response.Data.teams.Count > 0)
                return response.Data.teams[0];
            else
                return null;
        }

        public static List<Alliance> GetPlayoffAlliances(string eventCode)
        {
            string path = $"alliances/{eventCode}";

#if MOCK
            string mockInput = System.IO.File.ReadAllText(@"C:\temp\GetPlayoffAlliance.mock.json");
            var response = new { Data = JsonConvert.DeserializeObject<AllianceListing>(mockInput) };
#else
            var request = new RestRequest(path);
            var response = _client.Execute<AllianceListing>(request);

            Log($"GetPlayoffAlliance-{eventCode}", response.Content);
#endif
            if (response.Data != null && response.Data.Alliances.Count > 0)
                return response.Data.Alliances;
            else
                return null;
        }

        private static void Log(string v, string content)
        {
#if xDEBUG
            System.IO.StreamWriter sw = new System.IO.StreamWriter($@"C:\temp\FRCGroove.logs\{v}.{DateTime.Now:yyyy-dd-mm-HH-MM-ss}.json");
            sw.Write(content);
            sw.Close();
#endif
        }
    }
}
