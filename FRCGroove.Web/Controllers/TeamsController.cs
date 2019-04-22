using FRCGroove.Lib;
using FRCGroove.Lib.Models;
using FRCGroove.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FRCGroove.Web.Controllers
{
    public class TeamsController : Controller
    {
        // GET: Teams
        public ActionResult Index(int? page, string sort)
        {
            TeamListing teamListingModel = new TeamListing();

            //List<RegisteredTeam> teams = FRCEventsAPI.TeamListingCache.Select(t => t.Value).ToList();

            List<string> _champs = new List<string>() { { "CARVER" }, { "GALILEO" }, { "HOPPER" }, { "NEWTON" }, { "ROEBLING" }, { "TURING" } };
            //List<Event> champsEvents = FRCEventsAPI.GetEventListing();
            //if (champsEvents != null)
            {
                //champsEvents = champsEvents.Where(e => e.name.StartsWith("FIRST Championship - Houston") && e.code != "CMPTX").ToList();

                List<RegisteredTeam> champsTeams = new List<RegisteredTeam>();
                Dictionary<int, string> teamDivisions = new Dictionary<int, string>();
                //foreach (string eventCode in champsEvents.Select(e => e.code))
                foreach (string eventCode in _champs)
                {
                    string initcap = eventCode.Substring(0, 1) + eventCode.Substring(1).ToLower();
                    List<RegisteredTeam> eventTeams = FRCEventsAPI.GetEventTeamListing(eventCode);
                    if (eventTeams != null && eventTeams.Count > 0)
                    {
                        eventTeams.Select(t => { t.champsDivision = initcap; return t; }).ToList();
                        champsTeams.AddRange(eventTeams);
                    }
                }

                Dictionary<int, string> pitLocationsGalileo = FRCEventsAPI.GetPitLocations("GALILEO");
                foreach (RegisteredTeam team in champsTeams)
                {
                    if (pitLocationsGalileo.ContainsKey(team.teamNumber))
                        team.pitLocation = pitLocationsGalileo[team.teamNumber];
                }

                if (sort == null || sort.Length == 0) sort = "#";

                if (sort == "#" || sort == "Number")
                    teamListingModel.Teams = champsTeams.OrderBy(t => t.teamNumber).ToList();
                else if (sort == "Name")
                    teamListingModel.Teams = champsTeams.OrderBy(t => t.nameShort).ToList();
                else if (sort == "Division")
                    teamListingModel.Teams = champsTeams.OrderBy(t => t.champsDivision).ToList();
                else if (sort == "Pit")
                    teamListingModel.Teams = champsTeams.OrderBy(t => t.pitLocation).ToList();

                teamListingModel.Watchlist = BuildTeamsOfInterest(string.Empty).Select(t => Int32.Parse(t)).ToList();
            }
            //List<RegisteredTeam> champsTeams = teams.Where(t => teamNumbers.Contains(t.teamNumber)).Distinct().ToList();

            //List<RegisteredTeam> champsTeams = FRCEventsAPI.GetEventTeamListing("CMPTX");

            //int pageNumber = (page ?? 1) - 1;
            //int pageSize = 500;
            //int start = pageNumber * pageSize;
            //List<RegisteredTeam> model = champsTeams.Skip(start).Take(pageSize).ToList();
            //return View(model);

            //return View(champsTeams.OrderBy(t => t.teamNumber).ToList());

            return View(teamListingModel);
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

        [HttpPost]
        public JsonResult UpdateWatchList(string teamList)
        {
            List<string> teams = BuildTeamsOfInterest(teamList);

            this.ControllerContext.HttpContext.Response.Cookies.Add(new HttpCookie("teamList") { Value = string.Join(",", teams), Expires = DateTime.Now.AddYears(1) });

            return null;
        }

    }
}