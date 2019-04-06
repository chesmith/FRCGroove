using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using FRCGroove.Lib.models;

namespace FRCGroove.Web.Models
{
    public enum FRCEventState
    {
        Invalid,
        Past,
        Qualifications,
        Quarterfinals,
        Semifinals,
        Finals,
        Future
    }

    public class Dashboard
    {
        public string districtCode { get; set; }
        public Event FrcEvent { get; set; }
        public List<RegisteredTeam> TeamsOfInterest { get; set; }
        public double ScheduleOffset { get; set; }
        public List<Match> Matches { get; set; }
        public List<Alliance> Alliances { get; set; }
        public PlayoffBracket Bracket { get; set; }

        public FRCEventState EventState
        {
            get
            {
                if (Matches == null)
                    return FRCEventState.Invalid;
                if (Matches.Count() == 0 || Matches.Where(t => t.actualStartTime != null).Count() == 0)
                    return FRCEventState.Future;

                List<Match> finals = Matches.Where(m => m.title.StartsWith("Final") && m.teams.Where(t => t.number == 0).Count() == 0).ToList();
                if (finals.Count > 0)
                {
                    bool redWin = (finals.Where(t => t.scoreRedFinal > t.scoreBlueFinal).Count() == 2);
                    bool blueWin = (finals.Where(t => t.scoreRedFinal < t.scoreBlueFinal).Count() == 2);
                    if (redWin || blueWin)
                        return FRCEventState.Past;
                    else
                        return FRCEventState.Finals;
                }
                else
                {
                    List<Match> semifinals = Matches.Where(m => m.title.StartsWith("Semifinal") && m.teams.Where(t => t.number == 0).Count() == 0).ToList();
                    if (semifinals.Count > 0)
                    {
                        return FRCEventState.Semifinals;
                    }
                    else
                    {
                        List<Match> quarterfinals = Matches.Where(m => m.title.StartsWith("Quarterfinal") && m.teams.Where(t => t.number == 0).Count() == 0).ToList();
                        if (quarterfinals.Count > 0)
                            return FRCEventState.Quarterfinals;
                        else
                            return FRCEventState.Qualifications;
                    }
                }
            }
        }

        public Dashboard()
        {
            TeamsOfInterest = new List<RegisteredTeam>();
        }

        public string TeamOfInterestUrl(string eventCode, int teamNumber, string action)
        {
            string url = string.Empty;
            if (eventCode.Length > 0)
            {
                if (action == "add")
                {
                    if (TeamsOfInterest.Count() > 0)
                    {
                        string teamList = string.Join(",", TeamsOfInterest.Select(t => t.number));
                        url = $"/{eventCode}/{teamList},{teamNumber}";
                    }
                    else
                        url = $"/{eventCode}/{teamNumber}";
                }
                else if (action == "remove")
                {
                    if (TeamsOfInterest.Count() > 0)
                    {
                        string teamList = string.Join(",", TeamsOfInterest.Where(t => t.number != teamNumber).Select(t => t.number));
                        url = $"/{eventCode}/x{teamNumber},{teamList}";
                    }
                    else
                        url = $"/{eventCode}";
                }
            }
            return url;
        }
    }
}