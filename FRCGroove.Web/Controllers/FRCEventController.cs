using FRCGroove.Web.Models;
using FRCGroove.Lib;
using FRCGroove.Lib.models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FRCGroove.Web.Controllers
{
    public class FRCEventController : Controller
    {
        // GET: FRCEvent
        public ActionResult Index(string eventCode = "", string teamList = "")
        {
            //eventCode = "NYLI";
            //teamList = "";
            Dashboard dashboard = BuildEventDashboard(eventCode, teamList);

            return View(dashboard);
        }


        private Dashboard BuildEventDashboard(string eventCode, string teamList)
        {
            Dashboard dashboard = new Dashboard();

            if (eventCode.Length == 0) eventCode = "TXGRE";

            if (eventCode.Length > 0)
            {
                dashboard.FrcEvent = FRCEventsAPI.GetEvent(eventCode);
                if (dashboard.FrcEvent != null)
                {
                    dashboard.Matches = FRCEventsAPI.GetFullHybridSchedule(eventCode);
                    //dashboard.Matches = FRCEventsAPI.GetHybridSchedule(eventCode, "Qualification");

                    TimeSpan[] rollingDelta = new TimeSpan[3];
                    foreach (Match match in dashboard.Matches)
                    {
                        if (match.actualStartTime == null) break;

                        rollingDelta[0] = rollingDelta[1];
                        rollingDelta[1] = rollingDelta[2];
                        rollingDelta[2] = (match.actualStartTime.Value - match.startTime);
                    }
                    dashboard.ScheduleOffset = (rollingDelta[0].TotalMinutes + rollingDelta[1].TotalMinutes + rollingDelta[2].TotalMinutes) / 3;
                }
            }

            //if (teamList.Length == 0) teamList = "5414";

            if (teamList.Length > 0)
            {
                string[] teamNumbers = teamList.Split(',');
                foreach (string teamNumber in teamNumbers)
                {
                    RegisteredTeam team = FRCEventsAPI.GetTeam(Int32.Parse(teamNumber));
                    if (eventCode.Length > 0)
                    {
                        EnrichTeamData(team, eventCode);
                    }
                    dashboard.TeamsOfInterest.Add(team);
                }
            }

            dashboard.Bracket = new PlayoffBracket(dashboard.Matches);

            return dashboard;
        }

        private void EnrichTeamData(RegisteredTeam team, string eventCode)
        {
            List<DistrictRank> districtRankings = FRCEventsAPI.GetDistrictRankings(team.districtCode, team.number);
            if (districtRankings.Count > 0)
                team.districtRank = districtRankings[0].rank;

            List<EventRanking> eventRankings = FRCEventsAPI.GetEventRankings(eventCode, team.number);
            if (eventRankings.Count > 0)
                team.eventRank = eventRankings[0].rank;

            team.Stats = TBAAPI.GetStats("2019" + eventCode.ToLower());

        }
    }
}