using System.Text.Json.Serialization;

namespace Ireland.DeathNotices.Models
{
    public class NoticePeriodOutput
    {
        public int Year { get; set; }

        public PeriodType Period { get; set; }

        public int PeriodIndex { get; set; }

        public string County { get; set; }

        [JsonPropertyName("iTotalDisplayRecords")]
        public string TotalNotices { get; set; }
    }
}