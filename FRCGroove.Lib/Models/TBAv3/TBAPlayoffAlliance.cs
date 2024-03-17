using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRCGroove.Lib.Models.TBAv3
{
    public class TBAPlayoffAlliance
    {
        public List<object> declines { get; set; }
        public string name { get; set; }
        public List<string> picks { get; set; }
        public Status status { get; set; }

        public class Status
        {
            public string level { get; set; }
            public string status { get; set; }

            //public CurrentLevelRecord current_level_record { get; set; }
            //public object playoff_average { get; set; }
            //public Record record { get; set; }

            //public class CurrentLevelRecord
            //{
            //    public int losses { get; set; }
            //    public int ties { get; set; }
            //    public int wins { get; set; }
            //}

            //public class Record
            //{
            //    public int losses { get; set; }
            //    public int ties { get; set; }
            //    public int wins { get; set; }
            //}
        }
    }
}
