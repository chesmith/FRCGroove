using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRCGroove.Lib.Models
{
    public class TBAEvent
    {
        public string key { get; set; }
        public string name { get; set; }
        public string event_code { get; set; }
        public int event_type { get; set; }
        public TBADistrict district { get; set; }
        public string city { get; set; }
        public string state_prov { get; set; }
        public string country { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public int year { get; set; }

        public DateTime dateStart
        {
            get
            {
                if (start_date != null)
                    return DateTime.Parse(start_date);
                else
                    return DateTime.MinValue;
            }
        }

        public DateTime dateEnd
        {
            get
            {
                if (end_date != null)
                    return DateTime.Parse(end_date);
                else
                    return DateTime.MinValue;
            }
        }
    }
}
