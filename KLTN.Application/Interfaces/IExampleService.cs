using KLTN.Domain.Entities.Examples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.Interfaces
{
    public interface IExampleService
    {
        Task<IEnumerable<Example>> GetAllExampleAsync();
    }
}
