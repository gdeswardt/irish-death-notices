using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ireland.DeathNotices.Models;

namespace Ireland.DeathNotices.Services
{
    public interface IDeathNoticeService
    {
        Task<IList<NoticePeriodOutput>> GetNoticesAsync(int year, PeriodType periodType);
        Task<IList<NoticePerCountyOutput>> GetNoticesPerCountry(int year, PeriodType periodType);
    }
}