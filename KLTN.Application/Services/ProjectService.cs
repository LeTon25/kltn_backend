using AutoMapper;
using KLTN.Application.DTOs.Projects;
using KLTN.Application.DTOs.Users;
using KLTN.Domain.Entities;
using KLTN.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.Services
{
    public class ProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper mapper;
        private readonly UserManager<User> userManager;
        public ProjectService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.userManager = userManager;
        }


        public async Task<ProjectDto> GetProjectDtoAsync(string id)
        {
            if(string.IsNullOrEmpty(id))
            {
                return null;
            }
            var project = await _unitOfWork.ProjectRepository.GetFirstOrDefaultAsync(c => c.ProjectId == id);
            if (project == null) {
                return null;
            }
            var projectDto = mapper.Map<ProjectDto>(project);   
            projectDto.CreateUser = mapper.Map<UserDto>(await userManager.FindByIdAsync(id));

            return projectDto;
        }    
    }
}
