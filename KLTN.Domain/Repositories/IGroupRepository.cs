using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KLTN.Domain.Entities;
namespace KLTN.Domain.Repositories
{
    public interface IGroupRepository : IRepository<Group>
    {
        Task<IEnumerable<Report>> GetReportsInGroupAsync(string groupId);
    }
}
