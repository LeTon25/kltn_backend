using KLTN.Domain.Entities;
using KLTN.Domain.Repositories;
using KLTN.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Infrastructure.Repositories
{
    public class ReportRepository : Repository<Report>,IReportRepository
    {
        private readonly ApplicationDbContext _context;
        public ReportRepository(ApplicationDbContext db):base(db) 
        { 
            _context = db;
        }

        public async Task<Report> GetDetailReportAsync(string reportId)
        {
            var entity = await _context.Reports.
                Where(c => c.ReportId.Equals(reportId))
                .Include(c => c.CreateUser)
                .Include(c => c.ReportComments)
                .ThenInclude(e => e.User)
                .FirstOrDefaultAsync();
            return entity;
        }
    }
}
