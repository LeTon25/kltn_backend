using KLTN.Domain.Entities;
using KLTN.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Repositories
{
    public interface ISubjectRepository : IRepository<Subject>
    {
        Task<List<MonthlyStatistic>> GetMonthlySubjectStatistics(int year);

    }
}
