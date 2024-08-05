using KLTN.Contracts.DTOs.Example;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Contracts.Service
{
    public interface IExampleService
    {
        Task<IQueryable<ExampleDTO>> GetExamplesAsync();
    }
}
