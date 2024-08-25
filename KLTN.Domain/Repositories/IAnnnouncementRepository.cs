using KLTN.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Repositories
{
    public interface IAnnnouncementRepository : IRepository<Announcement>
    {
        IEnumerable<Announcement> GetByCourse(string courseId);
        Announcement GetById(string announceId);
    }
}
