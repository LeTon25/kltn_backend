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
    public class AnnouncementRepository : Repository<Announcement>, IAnnnouncementRepository
    {
        public AnnouncementRepository(ApplicationDbContext db):base(db) 
        { }
        public IEnumerable<Announcement> GetByCourse(string courseId)
        {
            throw new NotImplementedException();
        }

        public Announcement GetById(string announceId)
        {
            throw new NotImplementedException();
        }
    }
}
