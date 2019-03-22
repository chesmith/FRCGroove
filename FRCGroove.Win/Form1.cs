using FRCGroove.Win.models;

using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FRCGroove.Win
{
    public partial class Form1 : Form
    {
        RestClient client = new RestClient("https://frc-api.firstinspires.org/v2.0/2019");

        Dictionary<string, DateTime> _knownStartTimes = new Dictionary<string, DateTime>()
        {   {"TXCHA~Qualification", new DateTime(2019, 3, 15, 11, 00, 00, DateTimeKind.Utc)},
            {"TXCHA~Playoff", new DateTime(2019, 3, 16, 14, 00, 00, DateTimeKind.Utc)},
            {"TXPAS~Qualification", new DateTime(2019, 3, 29, 11, 00, 00, DateTimeKind.Utc)},
            {"TXPAS~Playoff", new DateTime(2019, 3, 30, 14, 00, 00, DateTimeKind.Utc)} };

        public Form1()
        {
            InitializeComponent();

            string clientid = ConfigurationManager.AppSettings["clientid"];
            string clientsecret = ConfigurationManager.AppSettings["clientsecret"];
            client.Authenticator = new HttpBasicAuthenticator(clientid, clientsecret);
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            List<Event> districtEvents = GetEventListing("TX", 3103);
            if (districtEvents != null && districtEvents.Count > 0)
            {
                foreach (Event districtEvent in districtEvents)
                {
                    txtResults.Text += $"{districtEvent.code}: {districtEvent.name} {districtEvent.dateStart:M/d}-{districtEvent.dateEnd:M/d}\r\n";

                    //List<Match> eventSchedule = GetEventSchedule(districtEvent.code, 3103);
                    //if (eventSchedule != null && eventSchedule.Count > 0)
                    //{
                    //    foreach (Match match in eventSchedule)
                    //    {
                    //        txtResults.Text += $"\t{match.matchNumber} | {match.tournamentLevel} | {match.startTime:M/d h:mm}\r\n";
                    //    }
                    //}
                    //else
                    //{
                    //    txtResults.Text += "\t<<no matches scheduled>>\r\n";
                    //}

                    List<EventRanking> eventRanks = GetEventRankings(districtEvent.code, 3103);
                    List<DistrictRank> districtRanks = GetDistrictRankings("TX", 3103);
                    txtResults.Text += $"Our current event rank: {(eventRanks.Count > 0 ? eventRanks[0].rank.ToString() : "n/a")} (District: {districtRanks[0].rank})";

                    //List<EventRanking> rankings = GetEventRankings(districtEvent.code);
                    //if (rankings != null && rankings.Count > 0)
                    //{
                    //    foreach (EventRanking ranking in rankings)
                    //    {
                    //        if (ranking.teamNumber == 3103)
                    //            txtResults.Text += $"\t* {ranking.rank} | {ranking.teamNumber}\r\n";
                    //        else
                    //            txtResults.Text += $"\t{ranking.rank} | {ranking.teamNumber}\r\n";
                    //    }
                    //}
                    //else
                    //{
                    //    txtResults.Text += "\t<<no rankings available>>\r\n";
                    //}

                    txtResults.Text += "\r\n";

                    //List<Match> hybridSchedule = GetFullHybridSchedule(districtEvent.code);
                    //TimeSpan[] rollingDelta = new TimeSpan[3];
                    //foreach(Match match in hybridSchedule)
                    //{
                    //    txtResults.Text += $"{match.matchNumber} | {match.tournamentLevel} | St:{match.startTime:M/d h:mm} | St(a):{match.actualStartTime: M/d h:mm} | Delta:{match.actualStartTime - match.startTime} | Sr:{match.scoreRedFinal} | Sb:{match.scoreBlueFinal}\r\n";

                    //    List<Team> redAlliance = match.teams.Where(t => t.station.StartsWith("Red")).ToList();
                    //    foreach (Team team in redAlliance)
                    //    {
                    //        txtResults.Text += $"\t{team.station}: {team.teamNumber}{(team.dq ? "(d)" : "")}{(team.surrogate ? "(s)" : "")}";
                    //    }
                    //    txtResults.Text += "\r\n";

                    //    List<Team> blueAlliance = match.teams.Where(t => t.station.StartsWith("Blue")).ToList();
                    //    foreach (Team team in blueAlliance)
                    //    {
                    //        txtResults.Text += $"\t{team.station}: {team.teamNumber}{(team.dq ? "(d)" : "")}{(team.surrogate ? "(s)" : "")}";
                    //    }
                    //    txtResults.Text += "\r\n";

                    //    //TODO: I think I'm going to have to math out from the "match number" which bracket it falls in (e.g. QF2-3, SF1-2, F1)

                    //    rollingDelta[0] = rollingDelta[1];
                    //    rollingDelta[1] = rollingDelta[2];
                    //    rollingDelta[2] = (match.actualStartTime - match.startTime);
                    //}

                    //double delta = (rollingDelta[0].TotalMinutes + rollingDelta[1].TotalMinutes + rollingDelta[2].TotalMinutes) / 3;
                    //txtResults.Text += $"Running about {Math.Abs(Math.Round(delta,0))} minutes {((delta < 0) ? "ahead" : "behind")}";

                    txtResults.Text += "\r\n";
                }
            }
            else
            {
                txtResults.Text = "no events";
            }
        }

        private List<Event> GetEventListing(string districtCode, int teamNumber = 0)
        {
            string path = $"events/?districtCode={districtCode}";
            if (teamNumber > 0) path += $"&teamNumber={teamNumber}";

            var request = new RestRequest(path);
            var response = client.Execute<EventListing>(request);

            List<Event> eventListing = response.Data.Events.OrderBy(t => t.dateStart).ToList();

            return eventListing;
        }

        private List<Match> GetEventSchedule(string eventCode, int teamNumber = 0)
        {
            string path = $"schedule/{eventCode}";
            if (teamNumber > 0) path += $"?teamNumber={teamNumber}";

            var request = new RestRequest(path);
            var response = client.Execute<ScheduleListing>(request);

            List<Match> schedule = response.Data.Schedule.OrderByDescending(t => t.tournamentLevel).ThenBy(t => t.matchNumber).ToList();

            //TODO: AdjustForTimeZone(eventCode, schedule);

            return schedule;
        }

        private List<Match> GetHybridSchedule(string eventCode, string tournamentLevel)
        {
            string path = $"schedule/{eventCode}/{tournamentLevel}/hybrid";

            var request = new RestRequest(path);
            var response = client.Execute<ScheduleListing>(request);

            List<Match> schedule = response.Data.Schedule.OrderBy(t => t.matchNumber).ToList();

            AdjustForTimeZone(eventCode, tournamentLevel, schedule);

            return schedule;
        }

        private List<Match> GetFullHybridSchedule(string eventCode)
        {
            List<Match> qualifications = GetHybridSchedule(eventCode, "Qualification");
            List<Match> playoffs = GetHybridSchedule(eventCode, "Playoff");

            List<Match> schedule = new List<Match>();
            schedule.AddRange(qualifications);
            schedule.AddRange(playoffs);

            return schedule;
        }

        private void AdjustForTimeZone(string eventCode, string tournamentLevel, List<Match> schedule)
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

        private List<EventRanking> GetEventRankings(string eventCode, int teamNumber = 0)
        {
            string path = $"rankings/{eventCode}";
            if (teamNumber > 0) path += $"?teamNumber={teamNumber}";

            var request = new RestRequest(path);
            var response = client.Execute<EventRankListing>(request);

            List<EventRanking> eventRankings = response.Data.Rankings.OrderBy(t => t.rank).ToList();

            return eventRankings;
        }

        private List<DistrictRank> GetDistrictRankings(string districtCode, int teamNumber = 0)
        {
            string path = $"rankings/district/";
            if (teamNumber > 0)
                path += $"?teamNumber={teamNumber}";
            else
                path += $"{districtCode}";

            var request = new RestRequest(path);
            var response = client.Execute<DistrictRankListing>(request);

            List<DistrictRank> districtRankings = response.Data.districtRanks.OrderBy(t => t.rank).ToList();

            return districtRankings;
        }
    }
}
