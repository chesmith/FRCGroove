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
        private Dictionary<string, string> _champsDivisions = new Dictionary<string, string>() {
                { "ARPKY", "ARCHIMEDES" },
                { "CPRA", "CURIE" },
                { "DCMP", "DALY" },
                { "GCMP", "GALILEO" },
                { "HCMP", "HOPPER" },
                { "JCMP", "JOHNSON" },
                { "MPCIA", "MILSTEIN" },
                { "NPFCMP", "NEWTON" }
            };

        // GET: Teams
        public ActionResult Index(string sort, string search)
        {
            TeamListing teamListingModel = BuildTeamListing(sort, search);

            return View(teamListingModel);
        }

        private TeamListing BuildTeamListing(string sort, string search)
        {
            TeamListing teamListing = new TeamListing();

            List<RegisteredTeam> champsTeams = FRCEventsAPI.GetChampsTeams();

            Dictionary<int, string> pitLocations = FRCEventsAPI.GetChampsPitLocations();
            if (pitLocations != null)
            {
                foreach (RegisteredTeam team in champsTeams)
                {
                    if (pitLocations.ContainsKey(team.teamNumber))
                        team.pitLocation = pitLocations[team.teamNumber];
                }
            }

            foreach(RegisteredTeam team in champsTeams)
            {
                team.epa = TBAAPI.EPACache[team.teamNumber];
            }

            if (sort == null || sort.Length == 0) sort = "#";

            if (search != null && search.Length > 0)
            {
                search = search.ToLower();
                champsTeams = champsTeams.Where(t => t.teamNumber.ToString().StartsWith(search) || t.nameShort.ToLower().Contains(search) || t.champsDivision.ToLower().Contains(search)).ToList();
            }

            if (sort == "#" || sort.ToLower() == "number")
                teamListing.Teams = champsTeams.OrderBy(t => t.teamNumber).ToList();
            else if (sort.ToLower() == "name")
                teamListing.Teams = champsTeams.OrderBy(t => t.nameShort).ToList();
            else if (sort.ToLower() == "epa")
                teamListing.Teams = champsTeams.OrderByDescending(t => t.epa.epa_end).ToList();
            else if (sort.ToLower() == "division")
                teamListing.Teams = champsTeams.OrderBy(t => t.champsDivision).ToList();
            else if (sort.ToLower() == "pit")
                teamListing.Teams = champsTeams.OrderBy(t => t.pitLocation).ToList();

            teamListing.Watchlist = BuildTeamsOfInterest(string.Empty).Select(t => Int32.Parse(t)).ToList();

            return teamListing;
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

        public ActionResult WatchList()
        {
            return View();
        }

        [HttpPost]
        public JsonResult UpdateWatchList(string teamList)
        {
            List<string> teams = BuildTeamsOfInterest(teamList);

            this.ControllerContext.HttpContext.Response.Cookies.Add(new HttpCookie("teamList") { Value = string.Join(",", teams), Expires = DateTime.Now.AddYears(1) });

            return null;
        }

        public JsonResult GetChampsTeams(string sort, string search)
        {
            return Json(BuildTeamListing(sort, search), JsonRequestBehavior.AllowGet);
        }
    }
}