using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRCGroove.Lib.Models.TBAv3
{
    public class TBAStats
    {
        public double OPR { get; set; }
        public double DPR { get; set; }
        public double CCWM { get; set; }

        public TBAStats(TBAStatsCollection statsCollection, int teamNumber)
        {
            if (statsCollection.oprs.ContainsKey("frc" + teamNumber))
            {
                OPR = @Math.Round(statsCollection.oprs["frc" + teamNumber.ToString()], 2);
                DPR = @Math.Round(statsCollection.dprs["frc" + teamNumber.ToString()], 2);
                CCWM = @Math.Round(statsCollection.ccwms["frc" + teamNumber.ToString()], 2);
            }
        }
    }
}
