using FRCGroove.Lib;
using FRCGroove.Lib.models;

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FRCGroove.Win
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            List<Event> districtEvents = FRCEventsAPI.GetEventListing("TX", 3103);
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

                    List<EventRanking> eventRanks = FRCEventsAPI.GetEventRankings(districtEvent.code, 3103);
                    List<DistrictRank> districtRanks = FRCEventsAPI.GetDistrictRankings("TX", 3103);
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
    }
}
