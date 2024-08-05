using KLTN.Application.Interfaces;
using KLTN.Domain.Entities.Examples;
using KLTN.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.Services
{
    public class ExampleService : IExampleService
    {
        private readonly IUnitOfWork unitOfWork;
        public ExampleService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<IEnumerable<Example>> GetAllExampleAsync()
        {
            return await this.unitOfWork.ExampleRepository.GetAllAsync();   
        }
    }
}
