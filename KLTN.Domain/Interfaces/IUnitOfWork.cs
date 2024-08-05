using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable  
    {
        IExampleRepository ExampleRepository { get; }
        public Task<int> SaveChangeAsync();
    }
}
