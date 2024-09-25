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
    }
}
