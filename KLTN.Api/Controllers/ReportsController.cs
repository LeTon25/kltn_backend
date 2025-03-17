using KLTN.Api.Filters;
using KLTN.Application.DTOs.Reports;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KLTN.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly ReportService reportService;
        public ReportsController(
            ReportService reportService
            ) {
            this.reportService = reportService;
        }
        [HttpGet("{groupId}/report/{reportId}")]
        [ServiceFilter(typeof(GroupResourceAccessFilter))]
        public async Task<IActionResult> GetByIdAsync(string reportId)
        {
            var response = await reportService.GetReportByIdAsync(reportId);
            return StatusCode(response.StatusCode,response);  

        }
        [HttpPost("{groupId}/report")]
        [ApiValidationFilter]
        [ServiceFilter(typeof(GroupResourceAccessFilter))]

        public async Task<IActionResult> PostReportAsync(CreateReportRequestDto requestDto)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await reportService.CreateReportAsync(userId!, requestDto);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPatch("{groupId}/report/{reportId}")]
        [ApiValidationFilter]
        [ServiceFilter(typeof(GroupResourceAccessFilter))]
        public async Task<IActionResult> PutReportId(string reportId, [FromBody] CreateReportRequestDto requestDto)
        {
            var response = await reportService.UpdateReportAsync(reportId, requestDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{groupId}/report/{reportId}")]
        [ServiceFilter(typeof(GroupResourceAccessFilter))]
        public async Task<IActionResult> DeleteReportAsync(string reportId)
        {
            var response = await reportService.DeleteReportAsync(reportId);
            return StatusCode(response.StatusCode, response);
        }

    }
}
