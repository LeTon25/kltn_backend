using KLTN.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KLTN.Domain.Entities;
using KLTN.Infrastructure.Data;

namespace KLTN.Infrastructure.Repositories
{
    public class SemesterRepository : Repository<Semester>, ISemesterRepository
    {
        public SemesterRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
