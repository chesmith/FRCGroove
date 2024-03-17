using System.Collections.Generic;

using FRCGroove.Lib.Models.FRCv2;
using FRCGroove.Lib.Models.Groove;

namespace FRCGroove.Web.Models
{
    public enum EventState
    {
        Invalid,
        Past,
        Qualifications,
        Playoffs,
        Finals,
        Future
    }

    public class Dashboard
    {
        public GrooveEvent Event { get; set; }
        public List<GrooveTeam> TeamsOfInterest { get; set; }
        public List<GrooveMatch> Matches { get; set; }
        public double ScheduleOffset { get; set; }

        //public List<TBAPlayoffAlliance> TBAPlayoffAlliances { get; set; }   // TODO
        public PlayoffBracket Bracket { get; set; }

        public Dictionary<int, GrooveEventRanking> EventRankings { get; set; }

        private EventState _eventState = EventState.Invalid;

        //public FRCEventState FrcEventState
        //{
        //    get
        //    {
        //        if (_eventState == FRCEventState.Invalid)
        //        {
        //            if (Matches != null)
        //            {
        //                if (Matches.Count() == 0)
        //                    _eventState = FRCEventState.Future;
        //                else
        //                {
        //                    List<Match> finals = Matches.Where(m => m.title.StartsWith("Final") && m.teams.Count(t => t.number == 0) == 0).ToList();
        //                    if(finals.Exists(t => t.scoreRedFinal > 0 || t.scoreBlueFinal > 0))
        //                    {
        //                        bool redWin = (finals.Count(t => t.scoreRedFinal > t.scoreBlueFinal) == 2);
        //                        bool blueWin = (finals.Count(t => t.scoreRedFinal < t.scoreBlueFinal) == 2);
        //                        if (redWin || blueWin)
        //                            _eventState = FRCEventState.Past;
        //                        else
        //                            _eventState = FRCEventState.Finals;
        //                    }
        //                    else
        //                    {
        //                        List<Match> semifinals = Matches.Where(m => m.title.StartsWith("Semifinal") && m.teams.Count(t => t.number == 0) == 0).ToList();
        //                        if (semifinals.Exists(t => t.scoreRedFinal > 0 || t.scoreBlueFinal > 0))
        //                        {
        //                            _eventState = FRCEventState.Semifinals;
        //                        }
        //                        else
        //                        {
        //                            List<Match> quarterfinals = Matches.Where(m => m.title.StartsWith("Quarterfinal") && m.teams.Count(t => t.number == 0) == 0).ToList();
        //                            if (quarterfinals.Exists(t => t.scoreRedFinal > 0 || t.scoreBlueFinal > 0))
        //                                _eventState = FRCEventState.Quarterfinals;
        //                            else
        //                                _eventState = FRCEventState.Qualifications;
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        return _eventState;
        //    }
        //}

        public EventState EventState { get; set; }
        //{
        //    get
        //    {
        //        if (_eventState == FRCEventState.Invalid)
        //        {
        //            if (TBAMatches != null)
        //            {
        //                if (TBAMatches.Count() <= 1)
        //                    _eventState = FRCEventState.Future;
        //                else
        //                {
        //                    List<TBAMatchData> finals = TBAMatches.Where(m => 
        //                        m.comp_level == "f"
        //                        && m.alliances.red.team_keys.Count() > 0
        //                        && m.alliances.blue.team_keys.Count() > 0).ToList();
        //                    if (finals.Exists(t => t.alliances.red.score > 0 || t.alliances.blue.score > 0))
        //                    {
        //                        bool redWin = (finals.Count(t => t.alliances.red.score > t.alliances.blue.score) == 2);
        //                        bool blueWin = (finals.Count(t => t.alliances.red.score < t.alliances.blue.score) == 2);
        //                        if (redWin || blueWin)
        //                            _eventState = FRCEventState.Past;
        //                        else
        //                            _eventState = FRCEventState.Finals;
        //                    }
        //                    else
        //                    {
        //                        List<TBAMatchData> semifinals = TBAMatches.Where(m => 
        //                            m.comp_level == "sf"
        //                            && m.alliances.red.team_keys.Count() > 0
        //                            && m.alliances.blue.team_keys.Count() > 0).ToList();
        //                        if (semifinals.Exists(t => t.alliances.red.score > 0 || t.alliances.blue.score > 0))
        //                        {
        //                            _eventState = FRCEventState.Semifinals;
        //                        }
        //                        else
        //                        {
        //                            _eventState = FRCEventState.Qualifications;
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        return _eventState;
        //    }
        //}

        public Dashboard()
        {
            TeamsOfInterest = new List<GrooveTeam>();
        }
    }
}