using KnowledgeSpace.BackendServer.Models.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KnowledgeSpace.BackendServer.Models
{
    public class DbInitializer
    {
        private readonly KnowledgeSpaceContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly string AdminRoleName = "Admin";
        private readonly string UserRoleName = "Member";

        public DbInitializer(KnowledgeSpaceContext context,
          UserManager<User> userManager,
          RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task Seed()
        {
            #region ROLE

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
                    Id = UserRoleName,
                    Name = UserRoleName,
                    NormalizedName = UserRoleName.ToUpper(),
                });
            }

            #endregion

            #region USER

            if (!_userManager.Users.Any())
            {
                var result = await _userManager.CreateAsync(new User
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "admin",
                    FirstName = "Quản trị",
                    LastName = "1",
                    Email = "info.hoangdinhtrong.hy@gmail.com",
                    LockoutEnabled = false
                }, "Admin@123");
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync("admin");
                    await _userManager.AddToRoleAsync(user, AdminRoleName);
                }
            }

            #endregion

            #region FUNCTION

            if (!_context.Functions.Any())
            {
                _context.Functions.AddRange(new List<Function>
                {
                    new Function {Id = "DASHBOARD", Name = "DASHBOARD", ParentId = null, SortOrder = 0,Url = "/dashboard",Icon="fa-dashboard" },

                    new Function {Id = "CONTENT",Name = "Nội dung",ParentId = null, SortOrder = 2,Url = "/contents",Icon="fa-table" },

                    new Function {Id = "CONTENT_CATEGORY",Name = "Danh mục",ParentId ="CONTENT",SortOrder = 1,Url = "/contents/categories",Icon="fa-edit"  },
                    new Function {Id = "CONTENT_KNOWLEDGEBASE",Name = "Bài viết",ParentId = "CONTENT",SortOrder = 2,Url = "/contents/knowledge-bases",Icon="fa-edit" },
                    new Function {Id = "CONTENT_COMMENT",Name = "Trang",ParentId = "CONTENT",SortOrder = 3,Url = "/contents/comments",Icon="fa-edit" },
                    new Function {Id = "CONTENT_REPORT",Name = "Báo xấu",ParentId = "CONTENT",SortOrder = 4,Url = "/contents/reports",Icon="fa-edit" },

                    new Function {Id = "STATISTIC",Name = "Thống kê", ParentId = null, SortOrder = 3, Url = "/statistics",Icon="fa-bar-chart-o" },

                    new Function {Id = "STATISTIC_MONTHLY_NEWMEMBER",Name = "Đăng ký từng tháng",ParentId = "STATISTIC",SortOrder = 1,Url = "/statistics/monthly-new-members",Icon = "fa-wrench"},
                    new Function {Id = "STATISTIC_MONTHLY_NEWKB",Name = "Bài đăng hàng tháng",ParentId = "STATISTIC",SortOrder = 2,Url = "/statistics/monthly-new-kbs",Icon = "fa-wrench"},
                    new Function {Id = "STATISTIC_MONTHLY_COMMENT",Name = "Comment theo tháng",ParentId = "STATISTIC",SortOrder = 3,Url = "/statistics/monthly-new-comments",Icon = "fa-wrench" },

                    new Function {Id = "SYSTEM", Name = "Hệ thống", ParentId = null, SortOrder = 1, Url = "/systems",Icon="fa-th-list" },

                    new Function {Id = "SYSTEM_USER", Name = "Người dùng",ParentId = "SYSTEM",Url = "/systems/users",Icon="fa-desktop"},
                    new Function {Id = "SYSTEM_ROLE", Name = "Nhóm quyền",ParentId = "SYSTEM",Url = "/systems/roles",Icon="fa-desktop"},
                    new Function {Id = "SYSTEM_FUNCTION", Name = "Chức năng",ParentId = "SYSTEM",Url = "/systems/functions",Icon="fa-desktop"},
                    new Function {Id = "SYSTEM_PERMISSION", Name = "Quyền hạn",ParentId = "SYSTEM",Url = "/systems/permissions",Icon="fa-desktop"},
                });
                await _context.SaveChangesAsync();
            }
            #endregion

            #region COMMAND
            if (!_context.Commands.Any())
            {
                _context.Commands.AddRange(new List<Command>()
                {
                    new Command(){Id = "VIEW", Name = "Xem"},
                    new Command(){Id = "CREATE", Name = "Thêm"},
                    new Command(){Id = "UPDATE", Name = "Sửa"},
                    new Command(){Id = "DELETE", Name = "Xoá"},
                    new Command(){Id = "APPROVE", Name = "Duyệt"},
                });
            }

            #endregion

            #region COMMAND IN FUNCTION

            var functions = _context.Functions;

            if (!_context.CommandInFunctions.Any())
            {
                foreach (var function in functions)
                {
                    var createAction = new CommandInFunction()
                    {
                        CommandId = "CREATE",
                        FunctionId = function.Id
                    };
                    _context.CommandInFunctions.Add(createAction);

                    var updateAction = new CommandInFunction()
                    {
                        CommandId = "UPDATE",
                        FunctionId = function.Id
                    };
                    _context.CommandInFunctions.Add(updateAction);
                    var deleteAction = new CommandInFunction()
                    {
                        CommandId = "DELETE",
                        FunctionId = function.Id
                    };
                    _context.CommandInFunctions.Add(deleteAction);

                    var viewAction = new CommandInFunction()
                    {
                        CommandId = "VIEW",
                        FunctionId = function.Id
                    };
                    _context.CommandInFunctions.Add(viewAction);
                }
            }
            #endregion

            #region PERMISSION
            if (!_context.Permissions.Any())
            {
                var adminRole = await _roleManager.FindByNameAsync(AdminRoleName);
                foreach (var function in functions)
                {
                    _context.Permissions.Add(new Permission(function.Id, adminRole.Id, "CREATE"));
                    _context.Permissions.Add(new Permission(function.Id, adminRole.Id, "UPDATE"));
                    _context.Permissions.Add(new Permission(function.Id, adminRole.Id, "DELETE"));
                    _context.Permissions.Add(new Permission(function.Id, adminRole.Id, "VIEW"));
                }
            }

            #endregion

            await _context.SaveChangesAsync();
        }
    }
}
