using AutoMapper;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Response;
using KLTN.Application.Services;
using KLTN.Domain.Entities;
using KLTN.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KLTN.Api.Controllers
{
    public class AllsController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly CourseService _courseService;
        private readonly AnnoucementService _annoucementService;
        private readonly AssignmentService _assignmentService;  
        private readonly ProjectService _projectService;
        private readonly SubjectService _subjectService;
        private readonly CommentService _commentService;
        private readonly GroupService _groupService;
        private readonly ReportService _reportService;
        private readonly RequestService _requestService;
        private readonly IMapper mapper;

        public AllsController(UserManager<User> userManager,
            CourseService courseService,
            AnnoucementService annoucementService,
            AssignmentService assignmentService,
            ProjectService projectService,
            SubjectService subjectService,
            CommentService commentService,
            GroupService groupService,
            ReportService reportService,
            RequestService requestService,
        IMapper mapper)
        {
            this._courseService = courseService;
            this._userManager = userManager;
            _annoucementService = annoucementService;
            _assignmentService = assignmentService;
            _projectService = projectService;
            _subjectService = subjectService;
            _commentService = commentService;
            this._groupService = groupService;  
            this._reportService = reportService;    
            this.mapper = mapper;
            _requestService = requestService;   
        }
        [HttpGet("courses")]
        public async Task<IActionResult> GetAllCoursesAsync()
        {
            var response = await _courseService.GetAllCoursesAsync();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("annoucements")]
        public async Task<IActionResult> GetAllAnnoucementsAsync()
        {
            var response = await _annoucementService.GetAllAnnouncementsAsync();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("projects")]
        public async Task<IActionResult> GetAllProjectsAsync()
        {
            var response = await _projectService.GetAllProjectsAsync();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("subjects")]
        public async Task<IActionResult> GetAllSubjectsAsync()
        {
            var response = await _subjectService.GetAllSubjectAsync();
            return StatusCode(response.StatusCode, response);
        }
        
        [HttpGet("comments")]
        public async Task<IActionResult> GetAllCommentsAsync()
        {
            var response = await _commentService.GetAllCommentAsync();
            return StatusCode(response.StatusCode, response);
        }
        //
        [HttpGet("groups")]
        public async Task<IActionResult> GetAllGroupsAsync()
        {
            var response = await _groupService.GetAllGroupsAsync();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("reports")]
        public async Task<IActionResult> GetAllReportsAsync()
        {
            var response = await _reportService.GetAllReportsAsync();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("assignments")]
        public async Task<IActionResult> GetAllAssignmentsAsync()
        {
            var response = await _assignmentService.GetAllAssignmentsAsync();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("requests")]
        public async Task<IActionResult> GetAllRequestsAsync()
        {
            var response = await _requestService.GetAllRequestAsync();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var dto = mapper.Map<List<UserDto>>(users);
            var response = new ApiResponse<List<UserDto>>(200,"Thành công",dto);
            return StatusCode(response.StatusCode, response);
        }
    }
}
