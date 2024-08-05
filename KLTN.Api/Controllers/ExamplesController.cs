using KLTN.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KLTN.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExamplesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public ExamplesController(IUnitOfWork unitOfWork) 
        { 
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetExamples()
        {
            try
            {
                var data = await _unitOfWork.ExampleRepository.GetAllAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
