using System.Collections.Generic;
using System.Linq;

namespace Ireland.DeathNotices.Models
{
    public class NoticePerCountyOutput
    {
        public int Year { get; set; }

        public int PeriodIndex { get; set; }

        public PeriodType Period { get; set; }

        public IDictionary<string, int> County { get; set; } = new Dictionary<string, int>();

        public int CountyTotalNotices => County.Sum(x => x.Value);

        public bool ControlCheck => TotalNotices == CountyTotalNotices;

        public int TotalNotices { get; set; }
    }
}