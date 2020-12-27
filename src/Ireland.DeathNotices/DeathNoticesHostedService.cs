using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ireland.DeathNotices.Models;
using Ireland.DeathNotices.Services;
using Microsoft.Extensions.Hosting;
using Volo.Abp;

namespace Ireland.DeathNotices
{
    public class DeathNoticesHostedService : IHostedService
    {
        private readonly IAbpApplicationWithExternalServiceProvider _application;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDeathNoticeService _deathNoticeService;
        private readonly GenerateCsvService _generator;

        public DeathNoticesHostedService(
            IAbpApplicationWithExternalServiceProvider application,
            IServiceProvider serviceProvider,
            IDeathNoticeService deathNoticeService,
            GenerateCsvService generator)
        {
            _application = application;
            _serviceProvider = serviceProvider;
            _deathNoticeService = deathNoticeService;
            _generator = generator;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _application.Initialize(_serviceProvider);
            var notices = new List<NoticePerCountyOutput>();
            var periodType = PeriodType.Month;
            
            for (var year = 2020; year <= 2020; year++)
            {
                var yearOutput = await _deathNoticeService.GetNoticesPerCountry(year, periodType);
                await _generator.WriteToFile($"irish-death-notices-by-{periodType.ToString().ToLower()}-{year}.csv", periodType, yearOutput);
                notices.AddRange(yearOutput);
            }

            await _generator.WriteToFile($"irish-death-notices-by-{periodType.ToString().ToLower()}.csv", periodType, notices);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _application.Shutdown();

            return Task.CompletedTask;
        }
    }
}
