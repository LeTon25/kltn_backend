using KLTN.Domain;
using KLTN.Domain.Entities;
using KLTN.Domain.Repositories;
using KLTN.Domain.Util;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using File = KLTN.Domain.Entities.File;
namespace KLTN.Infrastructure.Seeders
{
    public class DbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly string AdminRoleName = "Admin";
        private readonly string LecturerRoleName = "Lecturer";
        private readonly string StudentRoleName = "Student";
        private readonly IUnitOfWork _unitOfWork;    
        public DbInitializer(ApplicationDbContext context,
                UserManager<User> userManager,
                RoleManager<IdentityRole> roleManager,
                IUnitOfWork unitOfWork)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }

        public async Task Seed() 
        {
            #region Quyền

            if (!_roleManager.Roles.Any())
            {
                await _roleManager.CreateAsync(new IdentityRole
                {
                    Id = AdminRoleName,
                    Name = AdminRoleName,
                    NormalizedName = AdminRoleName.ToUpper(),
                });
                await _roleManager.CreateAsync(new IdentityRole
                {
                    Id = LecturerRoleName,
                    Name = LecturerRoleName,
                    NormalizedName = LecturerRoleName.ToUpper(),
                });
                await _roleManager.CreateAsync(new IdentityRole
                {
                    Id = StudentRoleName,
                    Name = StudentRoleName,
                    NormalizedName = StudentRoleName.ToUpper(),
                });
            }

            #endregion Quyền

            #region Người dùng

            if (!_userManager.Users.Any())
            {

                var result = await _userManager.CreateAsync(new User
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "khoaluan",
                    Email = "khoaluantotnghiep2425@gmail.com",
                    LockoutEnabled = false,
                    Gender = "Nam",
                    DoB = new DateTime(2003,1,1),
                    UserType = Domain.Enums.UserType.Admin,
                    FullName = "Khóa luận tốt nghiệp",
                    CreatedAt = DateTime.Now,   
                }, "Kltn@2425");
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync("khoaluan");
                    await _userManager.AddToRoleAsync(user, AdminRoleName);
                }
                var result2 = await _userManager.CreateAsync(new User
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "thanhhung",
                    Email = "thanhhung@gmail.com",
                    LockoutEnabled = false,
                    Gender = "Nam",
                    DoB = new DateTime(2003, 1, 1),
                    UserType = Domain.Enums.UserType.Lecturer,
                    FullName = "Biện Thành Hưng",
                    CreatedAt = DateTime.Now,
                }, "Kltn@2425");
                if (result2.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync("thanhhung");
                    await _userManager.AddToRoleAsync(user, LecturerRoleName);
                }
                var result3 = await _userManager.CreateAsync(new User
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "letoan",
                    Email = "letanminhtoan2505@gmail.com",
                    LockoutEnabled = false,
                    Gender = "Nam",
                    DoB = new DateTime(2003, 1, 1),
                    UserType = Domain.Enums.UserType.Lecturer,
                    FullName = "Lê Tấn Minh Toàn",
                    CreatedAt = DateTime.Now,
                }, "Kltn@2425");
                if (result3.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync("letoan");
                    await _userManager.AddToRoleAsync(user, LecturerRoleName);
                }
            }

            #endregion Người dùng
            await _context.SaveChangesAsync();
        }
        public async Task SeedSetting()
        {
            if(! await _context.Settings.AnyAsync())
            {
                var allCourses= await _context.Courses.ToListAsync();
                var courseIds = allCourses.Select(c=>c.CourseId).ToList();
                foreach (var courseId in courseIds) 
                {
                    var newSetting = new Setting()
                    {
                        SettingId = Guid.NewGuid().ToString(),
                        CourseId = courseId,
                        StartGroupCreation = null,
                        EndGroupCreation = null,
                        AllowStudentCreateProject = false,
                        AllowGroupRegistration = false,
                        HasFinalScore = false,
                        MaxGroupSize = null,
                        MinGroupSize = null
                    };
                    _context.Settings.Add(newSetting);
                }
                await _context.SaveChangesAsync();
            }    
        }
        public async Task SeedFinalAssignmentS()
        {
            var courses = await _unitOfWork.CourseRepository.FindByCondition(c => true).ToListAsync();

            foreach (var course in courses) 
            {
                var endtermScore = await _unitOfWork.ScoreStructureRepository.GetFirstOrDefaultAsync(c => c.CourseId!.Equals(course.CourseId) && c.ColumnName.Equals(Constants.Score.EndtermColumnName), false);
                if(endtermScore == null)
                {
                    var scoreStructure = Generator.GenerateScoreStructureForCourse(course.CourseId);
                    await _unitOfWork.ScoreStructureRepository.AddAsync(scoreStructure);
                    await _unitOfWork.SaveChangesAsync();
                    
                    endtermScore = await _unitOfWork.ScoreStructureRepository.GetFirstOrDefaultAsync(c => c.CourseId!.Equals(course.CourseId) && c.ColumnName.Equals(Constants.Score.EndtermColumnName), false);
                }

                var endtermAssignment = new Assignment()
                {
                    AssignmentId = Guid.NewGuid().ToString(),
                    CourseId = course.CourseId,
                    ScoreStructureId = endtermScore.Id,
                    Title = "Bài nộp cuối kỳ",
                    Type = Constants.AssignmentType.Final,
                    Content = "Nơi để học sinh nộp bài cuối kỳ",
                    Attachments = new List<File>(),
                    AttachedLinks = new List<MetaLinkData>(),
                    IsGroupAssigned = false,
                    CreatedAt = DateTime.Now,
                };
                _context.Assignments.Add(endtermAssignment);
            }
            await _context.SaveChangesAsync();
        }
    }
}
