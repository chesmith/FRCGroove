using FRCGroove.Lib.Models.FRCv2;
using FRCGroove.Lib.Models.TBAv3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRCGroove.Lib.Models.Groove
{
    public class GrooveDistrict
    {
        public string key { get; set; }
        public string name { get; set; }
        public int year { get; set; }

        public GrooveDistrict() { }
        
        public GrooveDistrict(TBADistrict district)
        {
            key = district.key;
            name = district.display_name;
            year = district.year;
        }

        public GrooveDistrict(FRCDistrict district)
        {
            key = district.code;
            name = district.name;
            year = DateTime.Now.Year;
        }
    }
}
