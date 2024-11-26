using KLTN.Domain.Entities;
using KLTN.Domain.Repositories;
using KLTN.Domain.Shared;
using KLTN.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Infrastructure.Repositories
{
    public class SubjectRepository : Repository<Subject>, ISubjectRepository
    {
        public SubjectRepository(ApplicationDbContext db) : base(db)
        {
        }

        public async Task<List<MonthlyStatistic>> GetMonthlySubjectStatistics(int year)
        {
            var statistics = await _db.Subjects
                        .Where(u => u.CreatedAt.Year <= year)
                        .GroupBy(u => new
                        {
                            Year = u.CreatedAt.Year,
                            Month = u.CreatedAt.Month,
                        })
                        .Select(g => new
                        {
                            g.Key.Year,
                            g.Key.Month,
                            Count = g.Count()
                        })
                        .OrderBy(stat => stat.Year)
                        .OrderBy(stat => stat.Month)
                        .ToListAsync();

            var result = new List<MonthlyStatistic>();
            var fullYearMonths = Enumerable.Range(1, 12)
           .Select(month => new
           {
               Year = year,
               Month = month,
               CumulativeUserCount = 0
           }).ToList();
            foreach (var monthStat in fullYearMonths)
            {
                var matchingStat = statistics
                    .Where(stat => stat.Year < year || (stat.Year == year && stat.Month <= monthStat.Month))
                    .Sum(stat => stat.Count);

                result.Add(new MonthlyStatistic
                {
                    Month = monthStat.Month.ToString(),
                    Count = matchingStat
                });
            }
            return result;
        }
    }
}
