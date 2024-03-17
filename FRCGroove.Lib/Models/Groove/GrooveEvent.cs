using FRCGroove.Lib.Models.FRCv2;
using FRCGroove.Lib.Models.TBAv3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRCGroove.Lib.Models.Groove
{
    public class GrooveEvent
    {
        public string key { get; set; } // follows TBA format
        public string name { get; set; }
        public string type { get; set; }
        public DateTime dateStart { get; set; }
        public DateTime dateEnd { get; set; }

        public GrooveEvent(TBAEvent e)
        {
            key = e.key;
            name = e.name;
            switch (e.event_type)
            {
                case 0: type = "Regional"; break;
                case 1: type = "District"; break;
                case 2: type = "District Championship"; break;
                case 3: type = "Championship Division"; break;
                case 4: type = "Championship Finals"; break;
                case 5: type = "District Championship Division"; break;
                case 99: type = "Offseason"; break;
                case 100: type = "Preseason"; break;
            }
            dateStart = e.dateStart;
            dateEnd = e.dateEnd;
        }

        public GrooveEvent(Event e)
        {
            key = $"{e.dateStart.Year}{e.code.ToLower()}";
            name = e.name;
            switch (e.type)
            {
                case "Regional": type = "Regional"; break;
                case "DistrictEvent": type = "District"; break;
                case "DistrictChampionship":
                case "DistrictChampionshipWithLevels": type = "District Championship"; break;
                case "ChampionshipDivision": type = "Championship Division"; break;
                case "Championship": type = "Championship Finals"; break;
                case "DistrictChampionshipDivision": type = "District Championship Division"; break;
                case "OffSeason": type = "Offseason"; break;
                case "OffSeasonWithAzureSync": type = "Preseason"; break;
            }
            dateStart = e.dateStart;
            dateEnd = e.dateEnd;
        }
    }
}
