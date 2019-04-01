using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRCGroove.Lib.models
{
    public class PlayoffBracket
    {

        public PlayoffBracket(List<Match> matches)
        {
            if (matches != null)
            {
                List<Match> qf1 = matches.Where(m => m.title.StartsWith("Quarterfinal 1")).ToList();
                List<Match> qf2 = matches.Where(m => m.title.StartsWith("Quarterfinal 2")).ToList();
                List<Match> qf3 = matches.Where(m => m.title.StartsWith("Quarterfinal 3")).ToList();
                List<Match> qf4 = matches.Where(m => m.title.StartsWith("Quarterfinal 4")).ToList();
            }
        }
    }
}
