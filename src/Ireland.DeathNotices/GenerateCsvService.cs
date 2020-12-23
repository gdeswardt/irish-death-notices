using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ireland.DeathNotices.Models;
using Ireland.DeathNotices.Services;
using Volo.Abp.DependencyInjection;

namespace Ireland.DeathNotices
{
    public class GenerateCsvService : ITransientDependency
    {
        public async Task WriteToFile(string fileSpec, PeriodType periodType, IList<NoticePerCountyOutput> notices)
        {
            await using StreamWriter writer = File.CreateText(fileSpec);
            var countyNames = string.Join(',', DeathNoticeService.Counties);
            await writer.WriteLineAsync($"YEAR,{periodType.ToString().ToUpper()},{countyNames},TOTAL");
            foreach (var item in notices)
            {
                var counties = string.Join(',', item.County.Select(x => x.Value.ToString()));
                await writer.WriteLineAsync($"{item.Year},{item.PeriodIndex},{counties},{item.TotalNotices}");
            }
        }
    }
}
