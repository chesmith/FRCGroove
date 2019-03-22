using System;
using System.Collections.Generic;
using System.Linq;

using RestSharp;
using RestSharp.Authenticators;

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
            {"TXGRE~Qualification", new DateTime(2019, 3, 22, 14, 00, 00, DateTimeKind.Utc)},
            {"TXGRE~Playoff", new DateTime(2019, 3, 23, 14, 00, 00, DateTimeKind.Utc)}};

        public static List<Event> GetEventListing(string districtCode, int teamNumber = 0)
        {
            string path = $"events/?districtCode={districtCode}";
            if (teamNumber > 0) path += $"&teamNumber={teamNumber}";

            var request = new RestRequest(path);
            var response = _client.Execute<EventListing>(request);

            List<Event> eventListing = response.Data.Events.OrderBy(t => t.dateStart).ToList();

            return eventListing;
        }

        public static List<Match> GetEventSchedule(string eventCode, int teamNumber = 0)
        {
            string path = $"schedule/{eventCode}";
            if (teamNumber > 0) path += $"?teamNumber={teamNumber}";

            var request = new RestRequest(path);
            var response = _client.Execute<ScheduleListing>(request);

            List<Match> schedule = response.Data.Schedule.OrderByDescending(t => t.tournamentLevel).ThenBy(t => t.matchNumber).ToList();

            //TODO: AdjustForTimeZone(eventCode, schedule);

            return schedule;
        }

        public static List<Match> GetHybridSchedule(string eventCode, string tournamentLevel)
        {
            string path = $"schedule/{eventCode}/{tournamentLevel}/hybrid";

            var request = new RestRequest(path);
            var response = _client.Execute<ScheduleListing>(request);

            List<Match> schedule = response.Data.Schedule.OrderBy(t => t.matchNumber).ToList();

            AdjustForTimeZone(eventCode, tournamentLevel, schedule);

            return schedule;
        }

        public static List<Match> GetFullHybridSchedule(string eventCode)
        {
            List<Match> qualifications = GetHybridSchedule(eventCode, "Qualification");
            List<Match> playoffs = GetHybridSchedule(eventCode, "Playoff");

            List<Match> schedule = new List<Match>();
            schedule.AddRange(qualifications);
            schedule.AddRange(playoffs);

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
        }

        public static List<EventRanking> GetEventRankings(string eventCode, int teamNumber = 0)
        {
            string path = $"rankings/{eventCode}";
            if (teamNumber > 0) path += $"?teamNumber={teamNumber}";

            var request = new RestRequest(path);
            var response = _client.Execute<EventRankListing>(request);

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

            var request = new RestRequest(path);
            var response = _client.Execute<DistrictRankListing>(request);

            List<DistrictRank> districtRankings = response.Data.districtRanks.OrderBy(t => t.rank).ToList();

            return districtRankings;
        }
    }
}
