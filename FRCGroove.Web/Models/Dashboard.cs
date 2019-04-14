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
        public List<RegisteredTeam> RegisteredTeams { get; set; }

        private FRCEventState _eventState = FRCEventState.Invalid;

        public FRCEventState EventState
        {
            get
            {
                if (_eventState == FRCEventState.Invalid)
                {
                    if (Matches != null)
                    {
                        if (Matches.Count() == 0)
                            _eventState = FRCEventState.Future;
                        else
                        {
                            List<Match> finals = Matches.Where(m => m.title.StartsWith("Final") && m.teams.Count(t => t.number == 0) == 0).ToList();
                            if (finals.Count > 0)
                            {
                                bool redWin = (finals.Count(t => t.scoreRedFinal > t.scoreBlueFinal) == 2);
                                bool blueWin = (finals.Count(t => t.scoreRedFinal < t.scoreBlueFinal) == 2);
                                if (redWin || blueWin)
                                    _eventState = FRCEventState.Past;
                                else
                                    _eventState = FRCEventState.Finals;
                            }
                            else
                            {
                                List<Match> semifinals = Matches.Where(m => m.title.StartsWith("Semifinal") && m.teams.Count(t => t.number == 0) == 0).ToList();
                                if (semifinals.Count > 0)
                                {
                                    _eventState = FRCEventState.Semifinals;
                                }
                                else
                                {
                                    List<Match> quarterfinals = Matches.Where(m => m.title.StartsWith("Quarterfinal") && m.teams.Count(t => t.number == 0) == 0).ToList();
                                    if (quarterfinals.Count > 0)
                                        _eventState = FRCEventState.Quarterfinals;
                                    else
                                        _eventState = FRCEventState.Qualifications;
                                }
                            }
                        }
                    }
                }

                return _eventState;
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
                        url = $"/FRCEvent/{eventCode}/{teamList},{teamNumber}";
                    }
                    else
                        url = $"/FRCEvent/{eventCode}/{teamNumber}";
                }
                else if (action == "remove")
                {
                    if (TeamsOfInterest.Count() > 0)
                    {
                        string teamList = string.Join(",", TeamsOfInterest.Where(t => t.number != teamNumber).Select(t => t.number));
                        url = $"/FRCEvent/{eventCode}/x{teamNumber},{teamList}";
                    }
                    else
                        url = $"/FRCEvent/{eventCode}";
                }
            }
            return url;
        }
    }
}