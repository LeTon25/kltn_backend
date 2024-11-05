using KLTN.Application.DTOs.Settings;
using KLTN.Application.Services;
using KLTN.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KLTN.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SettingService _settingService;    
        public SettingsController(IUnitOfWork unitOfWork,
            SettingService settingService) 
        { 
            this._unitOfWork = unitOfWork;
            this._settingService = settingService;
        }
        [HttpPost("{courseId}/settings")]
        public async Task<IActionResult> UpdateSettingsForCourseAsync(string courseId,SettingDto dto)
        { 
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var respone = await this._settingService.UpdateSettingForCourseAsync(courseId, dto, userId!);
            return StatusCode(respone.StatusCode, respone);
        }
    }
}
