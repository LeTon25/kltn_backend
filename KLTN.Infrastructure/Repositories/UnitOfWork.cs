using KLTN.Domain.Interfaces;
using KLTN.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public IExampleRepository ExampleRepository { get; set; }
        private readonly ApplicationDbContext context;
        public UnitOfWork(ApplicationDbContext context) 
        {
            this.context = context;
            ExampleRepository = new ExampleRepository(context);
        }

        public void Dispose()
        {
            context.Dispose();
        }

        public async Task<int> SaveChangeAsync()
        {
           return await context.SaveChangesAsync();
        }
    }
}
