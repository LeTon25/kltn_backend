using KLTN.Domain.Entities;
using KLTN.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Repositories
{
    public interface ICourseRepository : IRepository<Course>
    {
        Task<List<MonthlyStatistic>> GetMonthlyCourseStatistics(int year);
    }
}
