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

            //Dashboard dashboard1 = BuildEventDashboard(districtCode, eventCode, teams);
            Dashboard dashboard = BuildEventDashboardTBA("2023" + districtCode.ToLower(), "2023" + eventCode.ToLower(), teams);

            this.ControllerContext.HttpContext.Response.Cookies.Add(new HttpCookie("teamList") { Value = string.Join(",", teams), Expires = DateTime.Now.AddYears(100) });
            this.ControllerContext.HttpContext.Response.Cookies.Add(new HttpCookie("eventCode") { Value = eventCode, Expires = DateTime.Now.AddYears(1) });

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

        //private Dashboard BuildEventDashboard(string districtCode, string eventCode, List<string> teamsOfInterest)
        //{
        //    Dashboard dashboard = new Dashboard();

        //    dashboard.districtCode = districtCode;

        //    if (eventCode.Length > 0)
        //    {
        //        dashboard.FrcEvent = FRCEventsAPI.GetEvent(eventCode);
        //        if (dashboard.FrcEvent != null)
        //        {
        //            dashboard.Matches = FRCEventsAPI.GetFullHybridSchedule(eventCode);

        //            if (dashboard.Matches != null && (dashboard.EventState == FRCEventState.Past || dashboard.EventState == FRCEventState.Qualifications || dashboard.EventState == FRCEventState.Quarterfinals || dashboard.EventState == FRCEventState.Semifinals || dashboard.EventState == FRCEventState.Finals))
        //            {
        //                if (dashboard.EventState != FRCEventState.Qualifications)
        //                {
        //                    dashboard.Alliances = FRCEventsAPI.GetPlayoffAlliances(eventCode);
        //                    dashboard.Bracket = new PlayoffBracket(dashboard.Alliances, dashboard.Matches);
        //                }

        //                if (dashboard.EventState != FRCEventState.Past)
        //                {
        //                    List<Match> matchesForOffsetCalc = dashboard.Matches;
        //                    if (dashboard.EventState != FRCEventState.Qualifications)
        //                        matchesForOffsetCalc = dashboard.Matches.Where(m => m.tournamentLevel == "Playoff").ToList();
        //                    dashboard.ScheduleOffset = CalculateScheduleOffset(matchesForOffsetCalc);
        //                }
        //            }
        //        }

        //        dashboard.RegisteredTeams = FRCEventsAPI.TeamListingCache;

        //        //TODO: Should this be up in the != null section above?
        //        if (dashboard.EventState != FRCEventState.Invalid)
        //        {
        //            List<EventRanking> eventRankings = FRCEventsAPI.GetEventRankings(eventCode);
        //            if (eventRankings != null)
        //                dashboard.EventRankings = eventRankings.ToDictionary(e => e.teamNumber, e => e);
        //        }
        //    }

        //    dashboard.TeamsOfInterest.AddRange(GatherTeamsOfInterest(eventCode, teamsOfInterest, dashboard.EventRankings));

        //    return dashboard;
        //}

        private Dashboard BuildEventDashboardTBA(string districtCode, string eventCode, List<string> teamsOfInterest)
        {
            Dashboard dashboard = new Dashboard();

            dashboard.districtCode = districtCode;

            if (eventCode.Length > 0)
            {
                dashboard.TBAEvent = TBAAPI.GetEvent(eventCode);
                if (dashboard.TBAEvent != null)
                {
                    dashboard.TBAMatches = TBAAPI.GetMatches(eventCode);
                    dashboard.TBAMatches = dashboard.TBAMatches.OrderBy(m => m.sortTitle).ToList();

                    //TODO: replace with TBA
                    if (dashboard.TBAMatches != null && (dashboard.EventState == FRCEventState.Past || dashboard.EventState == FRCEventState.Qualifications || dashboard.EventState == FRCEventState.Quarterfinals || dashboard.EventState == FRCEventState.Semifinals || dashboard.EventState == FRCEventState.Finals))
                    {
                        if (dashboard.EventState != FRCEventState.Qualifications)
                        {
                            dashboard.TBAPlayoffAlliances = TBAAPI.GetPlayoffAlliances(eventCode);
                            //dashboard.TBABracket = new PlayoffBracket(dashboard.Alliances, dashboard.Matches);
                        }

                        if (dashboard.EventState != FRCEventState.Past)
                        {
                            List<TBAMatchData> matchesForOffsetCalc = dashboard.TBAMatches;
                            if (dashboard.EventState != FRCEventState.Qualifications)
                                matchesForOffsetCalc = dashboard.TBAMatches.Where(m => m.comp_level != "qm").ToList();
                            dashboard.ScheduleOffset = CalculateScheduleOffsetTBA(matchesForOffsetCalc);
                        }
                    }

                    if (dashboard.EventState != FRCEventState.Invalid)
                    {
                        TBAEventRankings rankings = TBAAPI.GetEventRankings(eventCode);
                        if (rankings != null)
                        {
                            dashboard.EventRankings = rankings.rankings.ToDictionary(e => Int32.Parse(e.team_key.Substring(3)), e => e);
                        }
                    }
                }

                dashboard.RegisteredTeams = FRCEventsAPI.TeamListingCache;  //TODO: replace with TBA
            }

            dashboard.TeamsOfInterest.AddRange(GatherTeamsOfInterest(eventCode, teamsOfInterest, dashboard.EventRankings));

            return dashboard;
        }

        private List<RegisteredTeam> GatherTeamsOfInterest(string eventCode, List<string> teamList, Dictionary<int, TBARanking> eventRankings = null)
        {
            List<RegisteredTeam> teamsOfInterest = new List<RegisteredTeam>();
            if (teamList != null && teamList.Count > 0)
            {
                TBAStatsCollection stats = TBAAPI.GetStats(eventCode);
                if (eventRankings == null)
                    eventRankings = TBAAPI.GetEventRankings(eventCode).rankings.ToDictionary(e => Int32.Parse(e.team_key.Substring(3)), e => e);

                foreach (string teamNumber in teamList)
                {
                    RegisteredTeam team = FRCEventsAPI.GetTeam(Int32.Parse(teamNumber));  //TODO: replace with TBA
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

        private string ConvertToTBACode(string eventCode)
        {
            Dictionary<string, string> champsMap = new Dictionary<string, string>() { { "CARVER", "carv" }, { "GALILEO", "gal" }, { "HOPPER", "hop" }, { "NEWTON", "new" }, { "ROEBLING", "roe" }, { "TURING", "tur" }, { "ARCHIMEDES", "arc" }, { "CARSON", "cars" }, { "CURIE", "cur" }, { "DALY", "dal" }, { "DARWIN", "dar" }, { "TESLA", "tes" } };
            if (champsMap.ContainsKey(eventCode))
            {
                return champsMap[eventCode];
            }
            return eventCode.ToLower();

        }

        private double CalculateScheduleOffset(List<Match> matches)
        {
            DateTime today = DateTime.Now.Date;
            List<Match> todaysMatches = matches.Where(m => m.startTime.Date == today && m.actualStartTime != null).Reverse().Take(3).ToList();
            double sum = todaysMatches.Select(m => (m.actualStartTime.Value - m.startTime).TotalMinutes).Sum();
            double average = 0.0;
            if (Math.Abs(sum) > 0)
                average = sum / todaysMatches.Count();

            //if(average == 0.0)
            //{
            //    //TODO: if the next match scores have not posted and the time for the match after that has passed, consider matches running late
            //    //      and report as schedule offset by that delta
            //    List<Match> tmatches = matches.Where(m => m.actualStartTime == null).Take(3).ToList();
            //    if(tmatches[1].startTime < DateTime.Now)
            //    {
            //        average = (DateTime.Now - tmatches[1].startTime).TotalMinutes;
            //    }
            //}

            return average;
        }

        private double CalculateScheduleOffsetTBA(List<TBAMatchData> matches)
        {
            DateTime today = DateTime.Now.Date;
            List<TBAMatchData> todaysMatches = matches.Where(m => m.timeDT.Date == today && m.actual_time != 0).Reverse().Take(3).ToList();
            double sum = todaysMatches.Select(m => (m.actual_timeDT - m.timeDT).TotalMinutes).Sum();
            double average = 0.0;
            if (Math.Abs(sum) > 0)
                average = sum / todaysMatches.Count();

            //if(average == 0.0)
            //{
            //    //TODO: if the next match scores have not posted and the time for the match after that has passed, consider matches running late
            //    //      and report as schedule offset by that delta
            //    List<Match> tmatches = matches.Where(m => m.actualStartTime == null).Take(3).ToList();
            //    if(tmatches[1].startTime < DateTime.Now)
            //    {
            //        average = (DateTime.Now - tmatches[1].startTime).TotalMinutes;
            //    }
            //}

            return average;
        }

        [HttpPost]
        public JsonResult TeamsOfInterestAjax(string eventCode, string teamList, string sortName, string sortDirection)
        {
            if (eventCode.Length > 0)
            {
                List<string> teams = BuildTeamsOfInterest(teamList);

                this.ControllerContext.HttpContext.Response.Cookies.Add(new HttpCookie("teamList") { Value = string.Join(",", teams), Expires = DateTime.Now.AddYears(1) });

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