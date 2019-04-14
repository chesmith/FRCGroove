using FRCGroove.Web.Models;
using FRCGroove.Lib;
using FRCGroove.Lib.models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace FRCGroove.Web.Controllers
{
    public class FRCEventController : Controller
    {
        public FRCEventController()
        {
            FRCEventsAPI.CacheFolder = HostingEnvironment.MapPath("~/App_Data/cache/");
        }

        public ActionResult Index(string districtCode = "", string eventCode = "", string teamList = "")
        {
            List<string> teams = BuildTeamsOfInterest(teamList);

            Dashboard dashboard = BuildEventDashboard(districtCode, eventCode, teams);

            this.ControllerContext.HttpContext.Response.Cookies.Add(new HttpCookie("teamList") { Value = string.Join(",", teams) });
            this.ControllerContext.HttpContext.Response.Cookies.Add(new HttpCookie("eventCode") { Value = eventCode });

            //TODO: if we added teams from the cookie that weren't there in the input list, do a redirect with the full URL instead of just showing the dashboard (update the cookie first?)

            return View(dashboard);
        }

        private List<string> BuildTeamsOfInterest(string teamList)
        {
            string[] teamsFromQuerystring = teamList.Split(',');
            List<string> teams = new List<string>(teamsFromQuerystring);
            if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("teamList"))
            {
                string teamsFromCookie = this.ControllerContext.HttpContext.Request.Cookies["teamList"].Value;
                if (teamsFromCookie.Length > 0)
                {
                    string[] additionalTeams = teamsFromCookie.Split(',');
                    teams.AddRange(additionalTeams);
                }
            }

            List<string> teamsToRemove = teams.Where(t => t.IndexOf("x") == 0).Select(t => t.Substring(1)).ToList();
            List<string> teamsToKeep = teams.Where(t => t.Length > 0 && t.IndexOf("x") < 0 && !teamsToRemove.Contains(t)).ToList();

            teams = teamsToKeep.Distinct().ToList();
            return teams;
        }

        private Dashboard BuildEventDashboard(string districtCode, string eventCode, List<string> teamsOfInterest)
        {
            Dashboard dashboard = new Dashboard();

            dashboard.districtCode = districtCode;

            if (eventCode.Length > 0)
            {
                dashboard.FrcEvent = FRCEventsAPI.GetEvent(eventCode);
                if (dashboard.FrcEvent != null)
                {
                    dashboard.Matches = FRCEventsAPI.GetFullHybridSchedule(eventCode);

                    if (dashboard.Matches != null && (dashboard.EventState == FRCEventState.Past || dashboard.EventState == FRCEventState.Qualifications || dashboard.EventState == FRCEventState.Quarterfinals || dashboard.EventState == FRCEventState.Semifinals || dashboard.EventState == FRCEventState.Finals))
                    {
                        if (dashboard.EventState != FRCEventState.Qualifications)
                        {
                            dashboard.Alliances = FRCEventsAPI.GetPlayoffAlliances(eventCode);
                            dashboard.Bracket = new PlayoffBracket(dashboard.Alliances, dashboard.Matches);
                        }

                        if (dashboard.EventState != FRCEventState.Past)
                        {
                            List<Match> matchesForOffsetCalc = dashboard.Matches;
                            if (dashboard.EventState != FRCEventState.Qualifications)
                                matchesForOffsetCalc = dashboard.Matches.Where(m => m.tournamentLevel == "Playoff").ToList();
                            dashboard.ScheduleOffset = CalculateScheduleOffset(matchesForOffsetCalc);
                        }
                    }
                }

                dashboard.RegisteredTeams = FRCEventsAPI.GetEventTeamListing(eventCode);
            }

            AddTeamsOfInterest(eventCode, teamsOfInterest, dashboard);

            return dashboard;
        }

        private void AddTeamsOfInterest(string eventCode, List<string> teamList, Dashboard dashboard)
        {
            if (teamList != null && teamList.Count > 0)
            {
                foreach (string teamNumber in teamList)
                {
                    RegisteredTeam team = FRCEventsAPI.GetTeam(Int32.Parse(teamNumber));
                    if (eventCode.Length > 0)
                    {
                        EnrichTeamData(team, eventCode, dashboard.Matches);
                    }
                    dashboard.TeamsOfInterest.Add(team);
                }
            }
        }

        private void EnrichTeamData(RegisteredTeam team, string eventCode, List<Match> matches)
        {
            if (team != null)
            {
                List<DistrictRank> districtRankings = FRCEventsAPI.GetDistrictRankings(team.districtCode, team.number);
                if (districtRankings.Count > 0)
                    team.districtRank = districtRankings[0].rank;

                List<EventRanking> eventRankings = FRCEventsAPI.GetEventRankings(eventCode, team.number);
                if (eventRankings.Count > 0)
                    team.eventRank = eventRankings[0].rank;

                team.Stats = TBAAPI.GetStats("2019" + eventCode.ToLower());

                if (matches != null)
                {
                    Match nextMatch = (Match)matches.Where(m => m.teams.Count(t => t.number == team.number) > 0 && m.actualStartTime == null).FirstOrDefault();
                    if (nextMatch != null)
                    {
                        team.NextMatch = nextMatch;
                    }
                }
            }
        }

        private double CalculateScheduleOffset(List<Match> matches)
        {
            DateTime today = DateTime.Now.Date;
            List<Match> todaysMatches = matches.Where(m => m.startTime.Date == today && m.actualStartTime != null).Reverse().Take(3).ToList();
            double sum = todaysMatches.Select(m => (m.actualStartTime.Value - m.startTime).TotalMinutes).Sum();
            double average = 0.0;
            if (Math.Abs(sum) > 0)
                average = sum / todaysMatches.Count();

            return average;
        }
    }
}