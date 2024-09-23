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
    public class ScoreStructureRepository : Repository<ScoreStructure>, IScoreStructureRepository
    {
        private readonly ApplicationDbContext _context;
        public ScoreStructureRepository(ApplicationDbContext db) : base(db)
        {
            this._context = db;
        }

        public async Task<ScoreStructure> GetScoreStructureWithChildAsync(string id)
        {
            var result = await _context.ScoreStructures
                .Include(c => c.Children)
                .FirstOrDefaultAsync(c => c.Id.Equals(id));
            return result;
        }
    }
}
