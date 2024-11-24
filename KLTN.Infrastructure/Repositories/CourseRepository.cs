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
    public class CourseRepository : Repository<Course>, ICourseRepository
    {
        public CourseRepository(ApplicationDbContext db) : base(db)
        {
        }

        public async Task<List<MonthlyStatistic>> GetMonthlyCourseStatistics(int year)
        {
            var statistics = await _db.Courses
            .Where(u => u.CreatedAt.Year == year)
            .GroupBy(u => u.CreatedAt.Month)
            .Select(g => new MonthlyStatistic
            {
                Month = g.Key.ToString(),
                Count = g.Count()
            })
            .OrderBy(stat => stat.Month) 
            .ToListAsync();

            var result = Enumerable.Range(1, 12)
             .Select(month => new MonthlyStatistic
             {
                 Month = month.ToString(),
                 Count = statistics.FirstOrDefault(s => s.Month == month.ToString())?.Count ?? 0
             })
             .ToList();
            return result;
        }
    }
}
