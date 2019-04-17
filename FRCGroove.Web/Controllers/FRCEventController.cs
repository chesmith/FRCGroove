using FRCGroove.Web.Models;
using FRCGroove.Lib;
using FRCGroove.Lib.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FRCGroove.Web.Controllers
{
    public class FRCEventController : Controller
    {
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

            return teamsToKeep.Distinct().OrderBy(t => Int32.Parse(t)).ToList();
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

                dashboard.RegisteredTeams = FRCEventsAPI.TeamListingCache;
                List<EventRanking> eventRankings = FRCEventsAPI.GetEventRankings(eventCode);
                if(eventRankings != null)
                    dashboard.EventRankings = eventRankings.ToDictionary(e => e.teamNumber, e => e);
            }

            dashboard.TeamsOfInterest.AddRange(GatherTeamsOfInterest(eventCode, teamsOfInterest, dashboard.EventRankings));

            return dashboard;
        }

        private List<RegisteredTeam> GatherTeamsOfInterest(string eventCode, List<string> teamList, Dictionary<int, EventRanking> eventRankings = null)
        {
            List<RegisteredTeam> teamsOfInterest = new List<RegisteredTeam>();
            if (teamList != null && teamList.Count > 0)
            {
                TBAStatsCollection stats = TBAAPI.GetStats("2019" + eventCode.ToLower());
                if (eventRankings == null)
                    eventRankings = FRCEventsAPI.GetEventRankings(eventCode).ToDictionary(e => e.teamNumber, e => e);

                foreach (string teamNumber in teamList)
                {
                    RegisteredTeam team = FRCEventsAPI.GetTeam(Int32.Parse(teamNumber));
                    if (stats != null && stats.oprs != null && stats.oprs.ContainsKey("frc" + team.teamNumber))
                        team.Stats = new TBAStats(stats, team.teamNumber);
                    else
                        team.Stats = null;

                    if (eventRankings.Count > 0 && eventRankings.ContainsKey(team.teamNumber))
                        team.eventRank = eventRankings[team.teamNumber].rank;
                    else
                        team.eventRank = -1;

                    teamsOfInterest.Add(team);
                }
            }
            return teamsOfInterest;
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

        [HttpPost]
        public JsonResult TeamsOfInterestAjax(string eventCode, string teamList, string sortName, string sortDirection)
        {
            if (eventCode.Length > 0)
            {
                List<string> teams = BuildTeamsOfInterest(teamList);

                this.ControllerContext.HttpContext.Response.Cookies.Add(new HttpCookie("teamList") { Value = string.Join(",", teams) });

                List<RegisteredTeam> teamsOfInterest = GatherTeamsOfInterest(eventCode, teams);
                teamsOfInterest = teamsOfInterest.Where(t => t.Stats != null).ToList();

                if (sortName.Length == 0) sortName = "Rank";

                if (sortName == "Number")
                    return (sortDirection == "ASC" ? Json(teamsOfInterest.OrderBy(t => t.teamNumber).ToList()) : Json(teamsOfInterest.OrderByDescending(t => t.teamNumber).ToList()));
                else if (sortName == "Name")
                    return (sortDirection == "ASC" ? Json(teamsOfInterest.OrderBy(t => t.nameShort).ToList()) : Json(teamsOfInterest.OrderByDescending(t => t.nameShort).ToList()));
                else if (sortName == "Rank")
                    return (sortDirection == "ASC" ? Json(teamsOfInterest.OrderBy(t => t.eventRank).ToList()) : Json(teamsOfInterest.OrderByDescending(t => t.eventRank).ToList()));
                else if (sortName == "OPR")
                    return (sortDirection == "ASC" ? Json(teamsOfInterest.OrderBy(t => t.Stats.OPR).ToList()) : Json(teamsOfInterest.OrderByDescending(t => t.Stats.OPR).ToList()));
                else if (sortName == "DPR")
                    return (sortDirection == "ASC" ? Json(teamsOfInterest.OrderBy(t => t.Stats.DPR).ToList()) : Json(teamsOfInterest.OrderByDescending(t => t.Stats.DPR).ToList()));
                else if (sortName == "CCWM")
                    return (sortDirection == "ASC" ? Json(teamsOfInterest.OrderBy(t => t.Stats.CCWM).ToList()) : Json(teamsOfInterest.OrderByDescending(t => t.Stats.CCWM).ToList()));
                else
                    return null;
            }
            else
                return null;
        }
    }
}