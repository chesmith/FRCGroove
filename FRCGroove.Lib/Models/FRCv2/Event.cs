using System;
using System.Collections.Generic;

namespace FRCGroove.Lib.Models.FRCv2
{
    public class Event
    {
        public string code { get; set; }
        public object divisionCode { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string districtCode { get; set; }
        public string venue { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string stateprov { get; set; }
        public string country { get; set; }
        public string website { get; set; }
        public List<object> webcasts { get; set; }
        public string timezone { get; set; }
        public DateTime dateStart { get; set; }
        public DateTime dateEnd { get; set; }
    }
}
