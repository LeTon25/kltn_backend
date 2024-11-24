using KLTN.Application.DTOs.Dashboards;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KLTN.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RoleRequirement(["Admin"])]
    public class DashboardsController : ControllerBase
    {
        private readonly DashboardService _dashboardService;
        public DashboardsController(DashboardService dashboardService) 
        { 
            _dashboardService = dashboardService;
        }   
        [HttpGet("static")]
        public async Task<IActionResult> GetStaticAsync()
        {
           var response = await  _dashboardService.GetStaticAsync();
            return StatusCode(response.StatusCode, response);
        }
    }
}
