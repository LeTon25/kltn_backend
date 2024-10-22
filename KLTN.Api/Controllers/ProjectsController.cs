using AutoMapper;
using KLTN.Application.DTOs.Projects;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Services;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KLTN.Api.Controllers
{
    [Authorize]
    public class ProjectsController : BaseController
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly ProjectService projectService;
        public ProjectsController(ApplicationDbContext db, 
            IMapper mapper,
            ProjectService projectService)
        {
            this._db = db;
            this._mapper = mapper;
            this.projectService = projectService;
        }
        [HttpGet("{projectId}")]
        public async Task<IActionResult> GetByIdAsync(string projectId)
        {
            var response = await projectService.GetProjectByIdAsync(projectId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost]
        [ApiValidationFilter]
        public async Task<IActionResult> PostProjectAsync(CreateProjectRequestDto requestDto)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await projectService.CreateProjectAsync(userId!, requestDto);
            return StatusCode(response.StatusCode, response);   
        }
        [HttpPatch("{projectId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutProjectId(string projectId, [FromBody] CreateProjectRequestDto requestDto)
        {
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await projectService.UpdateProjectAsync(currentUserId!, projectId, requestDto);
            return StatusCode(response.StatusCode, response);

        }

        [HttpDelete("{projectId}")]
        public async Task<IActionResult> DeleteProjectAsync(string projectId)
        {
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await projectService.DeleteProjectAsync(currentUserId!, projectId);

            return StatusCode(response.StatusCode,response);
        }
    }
}
