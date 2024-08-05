using KLTN.Domain.Entities.Examples;
using KLTN.Domain.Interfaces;
using KLTN.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Infrastructure.Repositories
{
    public class ExampleRepository :Repository<Example,Guid>, IExampleRepository
    {
        public ExampleRepository(ApplicationDbContext context) : base(context) { }
    }
}
