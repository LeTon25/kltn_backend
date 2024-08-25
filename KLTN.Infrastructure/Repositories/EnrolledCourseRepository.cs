using KLTN.Domain.Entities;
using KLTN.Domain.Repositories;
using KLTN.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Infrastructure.Repositories
{
    public class EnrolledCourseRepository : Repository<EnrolledCourse>, IEnrolledCourseRepository
    {
        public EnrolledCourseRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
