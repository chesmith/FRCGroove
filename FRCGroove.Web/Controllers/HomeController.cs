using FRCGroove.Lib;
using FRCGroove.Lib.models;

using FRCGroove.Web.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FRCGroove.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string districtCode = "")
        {
            FRCEventListing frcEventListing = new FRCEventListing();

            frcEventListing.Districts = FRCEventsAPI.GetDistrictListing();
            frcEventListing.Districts.Insert(0, new District() { code = "", name = "All Districts" });
            frcEventListing.Districts.Insert(1, new District() { code = "World", name = "World Championship" });

            List<Event> eventListing;
            if (districtCode.Length > 0 && districtCode != "World")
                eventListing = FRCEventsAPI.GetDistrictEventListing(districtCode);
            else
                eventListing = FRCEventsAPI.GetEventListing();

            if (districtCode == "World")
            {
                eventListing = eventListing.Where(e => e.name.StartsWith("FIRST Championship")).ToList();
            }

            frcEventListing.PastEvents = eventListing.Where(e => e.dateEnd < DateTime.Now.Date).ToList();
            frcEventListing.CurrentEvents = eventListing.Where(e => e.dateStart <= DateTime.Now.Date && e.dateEnd >= DateTime.Now.Date).ToList();
            frcEventListing.FutureEvents = eventListing.Where(e => e.dateStart > DateTime.Now.Date).ToList();

            return View(frcEventListing);
        }

        private Dashboard BuildEventDashboard(string eventCode, string teamList)
        {
            Dashboard dashboard = new Dashboard();

            if (eventCode.Length > 0)
            {
                dashboard.FrcEvent = FRCEventsAPI.GetEvent(eventCode);
                if (dashboard.FrcEvent != null)
                {
                    dashboard.Matches = FRCEventsAPI.GetFullHybridSchedule(eventCode);

                    TimeSpan[] rollingDelta = new TimeSpan[3];
                    DateTime curdate = DateTime.MinValue;
                    foreach (Match match in dashboard.Matches)
                    {
                        if (match.actualStartTime == null) break;

                        if (match.startTime.Date > curdate)
                        {
                            rollingDelta = new TimeSpan[3];
                        }

                        rollingDelta[0] = rollingDelta[1];
                        rollingDelta[1] = rollingDelta[2];
                        rollingDelta[2] = (match.actualStartTime.Value - match.startTime);

                        curdate = match.startTime.Date;
                    }
                    dashboard.ScheduleOffset = (rollingDelta[0].TotalMinutes + rollingDelta[1].TotalMinutes + rollingDelta[2].TotalMinutes) / 3;
                }
            }

            if (teamList.Length > 0)
            {
                string[] teamNumbers = teamList.Split(',');
                foreach (string teamNumber in teamNumbers)
                {
                    RegisteredTeam team = FRCEventsAPI.GetTeam(Int32.Parse(teamNumber));
                    if (eventCode.Length > 0)
                    {
                        EnrichTeamData(team, eventCode, dashboard.Matches);
                    }
                    dashboard.TeamsOfInterest.Add(team);
                }
            }

            FRCEventState eventState = dashboard.EventState;
            if (eventState == FRCEventState.Quarterfinals || eventState == FRCEventState.Semifinals || eventState == FRCEventState.Finals || eventState == FRCEventState.Past)
            {
                dashboard.Alliances = FRCEventsAPI.GetPlayoffAlliances(eventCode);
                foreach (Alliance alliance in dashboard.Alliances)
                {
                    alliance.LoadTeams();
                    //foreach (RegisteredTeam team in alliance.teams)
                    //{
                    //    EnrichTeamData(team, eventCode, dashboard.Matches);
                    //}
                }
            }

            dashboard.Bracket = new PlayoffBracket(dashboard.Alliances, dashboard.Matches);

            return dashboard;
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

                Match nextMatch = (Match)matches.Where(m => m.teams.Where(t => t.number == team.number).Count() > 0 && m.actualStartTime == null).FirstOrDefault();
                if(nextMatch != null)
                {
                    team.NextMatch = nextMatch;
                }
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult FRCEvent(string eventCode = "", string teamList = "")
        {
            Dashboard dashboard = BuildEventDashboard(eventCode, teamList);

            return View(dashboard);
        }
    }
}