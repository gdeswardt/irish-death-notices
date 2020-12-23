using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Ireland.DeathNotices.Models;
using Volo.Abp.DependencyInjection;

namespace Ireland.DeathNotices.Services
{
    public class DeathNoticeService : IDeathNoticeService, ITransientDependency
    {
        private readonly HttpClient _httpClient;

        public static IList<string> Counties = new List<string>
        {
            "Antrim",
            "Armagh",
            "Carlow",
            "Cavan",
            "Clare",
            "Cork",
            "Derry",
            "Donegal",
            "Down",
            "Dublin",
            "Fermanagh",
            "Galway",
            "Kerry",
            "Kildare",
            "Kilkenny",
            "Laois",
            "Leitrim",
            "Limerick",
            "Longford",
            "Louth",
            "Mayo",
            "Meath",
            "Monaghan",
            "Offaly",
            "Roscommon",
            "Sligo",
            "Tipperary",
            "Tyrone",
            "Waterford",
            "Westmeath",
            "Wexford",
            "Wicklow",
        };

        public DeathNoticeService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<IList<NoticePeriodOutput>> GetNoticesAsync(int year, PeriodType periodType)
        {
            var periods = GetSearchPeriods(year, periodType);
            
            IList<NoticePeriodOutput> output = new List<NoticePeriodOutput>(); 
            foreach (var item in periods)
            {
                var periodOutput = await GetAsync(item.Value, item.Key, periodType);
                if (periodOutput != null)
                {
                    output.Add(periodOutput);
                }
            }
            return output;
        }

        public async Task<IList<NoticePerCountyOutput>> GetNoticesPerCountry(int year, PeriodType periodType)
        {
            var periods = GetSearchPeriods(year, periodType);
            var asyncTasks = new List<Task<NoticePeriodOutput>>();
            foreach (var item in periods)
            {
                asyncTasks.Add(GetAsync(item.Value, item.Key, periodType,null));
                asyncTasks.AddRange(Counties.Select(county => GetAsync(item.Value, item.Key, periodType, county)));
            }
            await Task.WhenAll(asyncTasks);

            var result = asyncTasks
                .Where(x => string.IsNullOrEmpty(x.Result.County))
                .Select(x => new NoticePerCountyOutput
                {
                    Year = x.Result.Year,
                    PeriodIndex = x.Result.PeriodIndex,
                    Period = x.Result.Period,
                    TotalNotices = int.Parse(x.Result.TotalNotices),
                    County = asyncTasks
                        .Where(c => c.Result.Year == x.Result.Year && c.Result.PeriodIndex == x.Result.PeriodIndex && !string.IsNullOrEmpty(c.Result.County))
                        .Select(i => new KeyValuePair<string,int>(i.Result.County, int.Parse(i.Result.TotalNotices)))
                        .OrderBy(o => o.Key)
                        .ToDictionary(k => k.Key, v => v.Value)
                })
                .ToList();

            return result;
        }

        private async Task<NoticePeriodOutput> GetAsync(Period period, int weekOfYear, PeriodType periodType, string county = null)
        {
            var url = BuildDeathNoticeUrl(period, county);
            var response = await _httpClient.GetAsync(url);
            var responseBody = await response.Content.ReadAsStringAsync();
            var output = JsonSerializer.Deserialize<NoticePeriodOutput>(responseBody);

            if (output == null)
            {
                return null;
            }

            output.Period = periodType;
            output.Year = period.StartDate.Year;
            output.PeriodIndex = weekOfYear;
            output.County = county;
            return output;
        }

        private IDictionary<int, Period> GetSearchPeriods(int year, PeriodType periodType)
        {
            return periodType == PeriodType.Week 
                ? GetYearWeekStartDates(year) 
                : GetYearMonthDates(year);
        }

        private string BuildDeathNoticeUrl(Period period, string county)
        {
            var baseUrl = "https://www.rip.ie/deathnotices.php?do=get_deathnotices_pages&iDisplayLength=0";
            var dateFrom = $"DateFrom={period.StartDate.Year}-{period.StartDate.Month}-{period.StartDate.Day}+00%3A00%3A00";
            var dateTo = $"DateTo={period.EndDate.Year}-{period.EndDate.Month}-{period.EndDate.Day}+23%3A59%3A59";
            var countyFilter = !string.IsNullOrEmpty(county)
                ? $"CountyID={Counties.IndexOf(county) + 1}"
                : "";
            
            return $"{baseUrl}&{dateFrom}&{dateTo}&{countyFilter}";
            
        }

        private IDictionary<int, Period> GetYearWeekStartDates(int year)
        {
            IDictionary<int, Period> dates = new Dictionary<int, Period>();
            for (var i = 1; i <= 52; i++)
            {
                var startDate = FirstDateOfWeek(year, i);
                var endDate = i != 52 
                    ? startDate.AddDays(6) 
                    : new DateTime(year, 12, 31);
                
                dates.Add(i, new Period { StartDate = startDate, EndDate = endDate });
            }
            return dates;
        }

        private IDictionary<int, Period> GetYearMonthDates(int year)
        {
            IDictionary<int, Period> dates = new Dictionary<int, Period>();
            for (var i = 1; i <= 12; i++)
            {
                var startDate = new DateTime(year, i, 1);
                var period = new Period
                {
                    StartDate = startDate,
                    EndDate = startDate.AddMonths(1).AddDays(-1)
                };
                dates.Add(i, period);
            }
            return dates;
        }
        
        private DateTime FirstDateOfWeek(int year, int weekOfYear)
        {
            var newYear = new DateTime(year, 1, 1);
            return newYear.AddDays((weekOfYear - 1) * 7);
        }
    }
}