namespace FRCGroove.Lib.Models
{
    public class Team
    {
        public int? teamNumber { get; set; }
        public string station { get; set; }
        public bool surrogate { get; set; }
        public bool? dq { get; set; }

        public int number
        {
            get
            {
                if (teamNumber.HasValue)
                    return teamNumber.Value;
                return 0;
            }
        }
    }
}
