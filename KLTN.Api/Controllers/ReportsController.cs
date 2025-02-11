﻿using KLTN.Api.Filters;
using KLTN.Application.DTOs.Reports;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KLTN.Api.Controllers
{
    [Authorize]
    public class ReportsController : BaseController
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
            return SetResponse(await reportService.GetReportByIdAsync(reportId));  

        }
        [HttpPost("{groupId}/report")]
        [ApiValidationFilter]
        [ServiceFilter(typeof(GroupResourceAccessFilter))]

        public async Task<IActionResult> PostReportAsync(CreateReportRequestDto requestDto)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return SetResponse(await reportService.CreateReportAsync(userId!,requestDto));
        }
        [HttpPatch("{groupId}/report/{reportId}")]
        [ApiValidationFilter]
        [ServiceFilter(typeof(GroupResourceAccessFilter))]
        public async Task<IActionResult> PutReportId(string reportId, [FromBody] CreateReportRequestDto requestDto)
        {
            return SetResponse(await reportService.UpdateReportAsync(reportId, requestDto));
        }

        [HttpDelete("{groupId}/report/{reportId}")]
        [ServiceFilter(typeof(GroupResourceAccessFilter))]
        public async Task<IActionResult> DeleteReportAsync(string reportId)
        {
            return SetResponse(await reportService.DeleteReportAsync(reportId));
        }

    }
}
