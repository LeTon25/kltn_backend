using KLTN.Domain.Entities;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public DbInitializer(ApplicationDbContext context,
                UserManager<User> userManager,
                RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
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
    }
}
