using AutoMapper;
using KLTN.Application.DTOs.Dashboards;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.Services
{
    public class DashboardService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public DashboardService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<ApiResponse<List<OverviewItem>>> GetStaticAsync()
        {
            var currentDateTime = DateTime.Now;

            var year = currentDateTime.Year;

            var userMonthlyStatistic = await unitOfWork.UserRepository.GetMonthlyUserStatistics(year);
            var courseMonthlyStatistic = await unitOfWork.CourseRepository.GetMonthlyCourseStatistics(year);
            var subjectMonthlyStatistic = await unitOfWork.SubjectRepository.GetMonthlySubjectStatistics(year);

            var data = new List<OverviewItem>();    

            for(var i  = 1; i<=12;i++)
            {
                var newItem  = new OverviewItem();  
                newItem.Month = i.ToString();   
                newItem.Users = userMonthlyStatistic.Where(c=>c.Month.Equals(i.ToString())).FirstOrDefault()!.Count;
                newItem.Courses = courseMonthlyStatistic.Where(c => c.Month.Equals(i.ToString())).FirstOrDefault()!.Count;
                newItem.Subjects = subjectMonthlyStatistic.Where(c => c.Month.Equals(i.ToString())).FirstOrDefault()!.Count;
                data.Add(newItem);
            }
            return new ApiResponse<List<OverviewItem>>(200,"Lấy dữ liệu thành công",data);
        }
    }
}
