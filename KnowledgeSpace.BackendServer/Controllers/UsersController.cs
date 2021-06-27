using KnowledgeSpace.BackendServer.Authorization;
using KnowledgeSpace.BackendServer.Constants;
using KnowledgeSpace.BackendServer.Helpers;
using KnowledgeSpace.BackendServer.Models;
using KnowledgeSpace.BackendServer.Models.Entities;
using KnowledgeSpace.ViewModels;
using KnowledgeSpace.ViewModels.Contents;
using KnowledgeSpace.ViewModels.Systems;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KnowledgeSpace.BackendServer.Controllers
{
    public class UsersController : BaseController
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly KnowledgeSpaceContext _context;

        /// <summary>
        /// CONSTRUCTOR CONTROLLER.
        /// </summary>
        /// <param name="userManager">USER MANAGER SERVICE.</param>
        public UsersController(UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            KnowledgeSpaceContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        #region MANAGER USER
        /// <summary>
        /// GET: api/Users
        /// GET ALL USERS.
        /// </summary>
        /// <returns>LIST OF USERS.</returns>
        [HttpGet]
        [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.VIEW)]
        public async Task<IActionResult> GetUsers()
        {
            //// GET ALL USERS
            var users = _userManager.Users;

            //// TAKE INFOMATIONS OF USER NEED SHOW
            var uservms = await users.Select(u => new UserVm()
            {
                Id = u.Id,
                UserName = u.UserName,
                Dob = u.Dob,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                FirstName = u.FirstName,
                LastName = u.LastName,
                CreateDate = u.CreateDate
            }).ToListAsync();

            return Ok(uservms);
        }

        /// <summary>
        /// GET: api/Users/filter.
        /// GET USERS WITH KEYWORD AND PAGINATION.
        /// </summary>
        /// <param name="filter">KEYWORD SEARCH.</param>
        /// <param name="pageIndex">INDEX OF NEXT PAGE.</param>
        /// <param name="pageSize">NUMBER OF RECORDS EACH PAGE.</param>
        /// <returns>HTTP STATUS WITH LIST OF USERS.</returns>
        [HttpGet("filter")]
        [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.VIEW)]
        public async Task<IActionResult> GetUsersPaging(string filter, int pageIndex, int pageSize)
        {
            //// GET ALL USERS
            var query = _userManager.Users;

            //// IF KEYWORD IS NOT NULL OR EMPTY, GET USERS CONSTAINS THIS KEYWORD
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Email.Contains(filter)
                || x.UserName.Contains(filter)
                || x.PhoneNumber.Contains(filter));
            }

            //// TOTAL RECORDS EQUAL NUMBER OF USERS's ROW
            var totalRecords = await query.CountAsync();

            //// TAKE RECORDS IN THE PAGE (NEXT PAGE)
            var items = await query.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserVm()
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Dob = u.Dob,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    CreateDate = u.CreateDate
                })
                .ToListAsync();

            //// PAGINATION
            var pagination = new Pagination<UserVm>
            {
                Items = items,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }

        /// <summary>
        /// api/Users/{id}
        /// GET USER WITH ID (KEY).
        /// </summary>
        /// <param name="id">KEY OF USER.</param>
        /// <returns>HTTP STATUS CODE.</returns>
        [HttpGet("{id}")]
        [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.VIEW)]
        public async Task<IActionResult> GetById(string id)
        {
            //// GET USER WITH ID (KEY)
            var user = await _userManager.FindByIdAsync(id);

            //// IF KEY IS NOT EXIST (USER IS NULL), RETURN STATUS 404
            if (user == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found user with id: {id}"));

            //// GIVE INFO INTO UserVn (JUST SHOW NEEDED FIELD)
            var userVm = new UserVm()
            {
                Id = user.Id,
                UserName = user.UserName,
                Dob = user.Dob,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreateDate = user.CreateDate
            };

            return Ok(userVm);
        }

        /// <summary>
        /// POST: api/Users
        /// CREATE NEW USER.
        /// </summary>
        /// <param name="request">INPUT USER's INFO.</param>
        /// <returns>HTTP STATUS CODE.</returns>
        [HttpPost]
        [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.CREATE)]
        [ApiValidationFilter]
        public async Task<IActionResult> PostUser(UserCreateRequest request)
        {
            //// CREATE NEW INSTANCE OF USER WITH INFO IS INPUT DATA
            var user = new User()
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                Dob = DateTime.Parse(request.Dob),
                UserName = request.UserName,
                LastName = request.LastName,
                FirstName = request.FirstName,
                PhoneNumber = request.PhoneNumber,
                CreateDate = DateTime.Now,
                LastModifiedDate = DateTime.Now
            };

            //// INSERT NEW USER INTO DATATABLE OF DATABASE AND SAVE CHANGE
            var result = await _userManager.CreateAsync(user, request.Password);

            //// IF RESULT AFTER INSERT IS SUCCEEDED, INSERT SUCCESS AND RETURN STATUS 201, ELSE RETURN STATUS IS 400
            if (result.Succeeded)
            {
                return CreatedAtAction(nameof(GetById), new { id = user.Id }, request);
            }
            else
            {
                return BadRequest(new ApiBadRequestResponse(result));
            }
        }

        /// <summary>
        /// PUT: api/Users/{id}
        /// UPDATE USER WITH ID (KEY).
        /// </summary>
        /// <param name="id">KEY OF USER.</param>
        /// <param name="request">INFO NEED UPDATE (INPUT DATA).</param>
        /// <returns>HTTP STATUS CODE.</returns>
        [HttpPut("{id}")]
        [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.UPDATE)]
        public async Task<IActionResult> PutUser(string id, [FromBody] UserCreateRequest request)
        {
            //// GET USER WITH ID (KEY)
            var user = await _userManager.FindByIdAsync(id);

            //// IF KEY IS NOT EXIST (USER IS NULL), RETURN STATUS 404
            if (user == null)
            {
                return NotFound(new ApiNotFoundResponse($"Cannot found user with id: {id}"));
            }

            //// GIVE INPUT DATA FOR EACH FIELD OF OBJECT WHICH NEED UPDATE INFOMATIONS
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Dob = DateTime.Parse(request.Dob);
            user.LastModifiedDate = DateTime.Now;
            //// UPDATE USER AND SAVE CHANGE INTO DATATABLE IN DATABASE
            var result = await _userManager.UpdateAsync(user);

            //// IF RESULT AFTER UPDATE IS SUCCEEDED, RETURN STATUS 201, ELSE RETURN STATUS 400
            if (result.Succeeded)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse(result));
        }

        /// <summary>
        /// CHANGE PASSWORD OF USER.
        /// </summary>
        /// <param name="id">KEY OF USER.</param>
        /// <param name="request">CURRENT PASSWORD AND NEW PASSWORD.</param>
        /// <returns></returns>
        [HttpPut("{id}/change-password")]
        [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.UPDATE)]
        [ApiValidationFilter]
        public async Task<IActionResult> PutUserPassword(string id, [FromBody] UserPasswordChangeRequest request)
        {
            //// GET USER WITH ID (KEY)
            var user = await _userManager.FindByIdAsync(id);

            //// IF KEY IS NOT EXSIST (USER IS NULL), RETURN STATUS 404
            if (user == null)
            {
                return NotFound(new ApiNotFoundResponse($"Cannot found user with id: {id}"));
            }

            //// CHANGE PASSWORD USE SERVICE OF IDENTITYSERVER4
            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            //// IF RESULT IS SUCCEEDED, CHANGE SUCCESS AND RETURN STATUS 201, ELSE RETURN STATUS 400
            if (result.Succeeded)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse(result));
        }

        /// <summary>
        /// DELETE: api/Users/{id}
        /// DELETE USER WITH ID.
        /// </summary>
        /// <param name="id">KEY OF USER.</param>
        /// <returns>HTTP STATUS CODE.</returns>
        [HttpDelete("{id}")]
        [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.DELETE)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            //// GET USER WITH ID (KEY)
            var user = await _userManager.FindByIdAsync(id);

            //// IF KEY IS NOT EXIST (USER IS NULL), RETURN STATUS 404
            if (user == null)
            {
                return NotFound(new ApiNotFoundResponse($"Cannot found user with id: {id}"));
            }

            //// NOT DELETE ADMIN USER
            var adminUsers = await _userManager.GetUsersInRoleAsync(SystemConstants.Roles.Admin);
            var otherUsers = adminUsers.Where(x => x.Id != id).ToList();
            if (otherUsers.Count == 0)
            {
                return BadRequest(new ApiBadRequestResponse("You cannot remove the only admin user remaining."));
            }

            //// DELETE USER FROM DATATABLE IN DATABASE AND SAVE CHANGE
            var result = await _userManager.DeleteAsync(user);

            //// IF RESULT AFTER DELETE IS SUCCEEDED, DELETE SUCCESS AND RETURN STATUS 200, ELSE RETURN STATUS 400
            if (result.Succeeded)
            {
                var uservm = new UserVm()
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Dob = user.Dob,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    CreateDate = user.CreateDate
                };
                return Ok(uservm);
            }
            return BadRequest(new ApiBadRequestResponse(result));
        }
        #endregion

        #region MANAGER PERMISSION BY USER
        /// <summary>
        /// DYNAMIC MENU BY PERMISSION IN EACH USER.
        /// </summary>
        /// <param name="userId">KEY OF USER.</param>
        /// <returns>LIST FUNCTIONS IN EACH USER.</returns>
        [HttpGet("{userId}/menu")]
        public async Task<IActionResult> GetMenuByUserPermission(string userId)
        {
            //// GET USER WITH ID (KEY)
            var user = await _userManager.FindByIdAsync(userId);

            //// GET ROLE OF USER
            var roles = await _userManager.GetRolesAsync(user);

            //// GET FUNCTION WHICH USER HAS PERMISSION (NOT DUPLICATE) AND RETURN WITH HTTP STATUS 200
            var query = from f in _context.Functions
                        join p in _context.Permissions
                            on f.Id equals p.FunctionId
                        join r in _roleManager.Roles on p.RoleId equals r.Id
                        join a in _context.Commands
                            on p.CommandId equals a.Id
                        where roles.Contains(r.Name) && a.Id == "VIEW"
                        select new FunctionVm
                        {
                            Id = f.Id,
                            Name = f.Name,
                            Url = f.Url,
                            ParentId = f.ParentId,
                            SortOrder = f.SortOrder,
                            Icon = f.Icon
                        };
            var data = await query.Distinct()
                .OrderBy(x => x.ParentId)
                .ThenBy(x => x.SortOrder)
                .ToListAsync();
            return Ok(data);
        }
        #endregion

        #region MANAGEMENT ROLE BY USER
        /// <summary>
        /// GET ROLES OF USER BY ID.
        /// </summary>
        /// <param name="userId">KEY OF USER</param>
        /// <returns>HTTP STATUS</returns>
        [HttpGet("{userId}/roles")]
        [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.VIEW)]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found user with id: {userId}"));
            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);
        }

        /// <summary>
        /// ASSIGN ROLES FOR USER
        /// </summary>
        /// <param name="userId">KEY OF USER</param>
        /// <param name="request">INPUT DATA</param>
        /// <returns>HTTP STATUS</returns>
        [HttpPost("{userId}/roles")]
        [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.UPDATE)]
        public async Task<IActionResult> PostRolesToUserUser(string userId, [FromBody] RoleAssignRequest request)
        {
            if (request.RoleNames?.Length == 0)
            {
                return BadRequest(new ApiBadRequestResponse("Role names cannot empty"));
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found user with id: {userId}"));
            var result = await _userManager.AddToRolesAsync(user, request.RoleNames);
            if (result.Succeeded)
                return Ok();

            return BadRequest(new ApiBadRequestResponse(result));
        }

        /// <summary>
        /// REMOVE ROLE FROM USER
        /// </summary>
        /// <param name="userId">KEY OF USER</param>
        /// <param name="request">INPUT DATA</param>
        /// <returns>HTTP STATUS</returns>
        [HttpDelete("{userId}/roles")]
        [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.VIEW)]
        public async Task<IActionResult> RemoveRolesFromUser(string userId, [FromQuery] RoleAssignRequest request)
        {
            if (request.RoleNames?.Length == 0)
            {
                return BadRequest(new ApiBadRequestResponse("Role names cannot empty"));
            }
            if (request.RoleNames.Length == 1 && request.RoleNames[0] == SystemConstants.Roles.Admin)
            {
                return BadRequest(new ApiBadRequestResponse($"Cannot remove {SystemConstants.Roles.Admin} role"));
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found user with id: {userId}"));
            var result = await _userManager.RemoveFromRolesAsync(user, request.RoleNames);
            if (result.Succeeded)
                return Ok();

            return BadRequest(new ApiBadRequestResponse(result));
        }
        #endregion

        #region MANAGEMENT KNOWLEDGE BASE BY USER
        /// <summary>
        /// GET KNOWLEDBASE OF CURRENT USER
        /// </summary>
        /// <param name="userId">KEY OF USER</param>
        /// <param name="pageIndex">NEXT PAGE</param>
        /// <param name="pageSize">NUMBER OF RECORDS PER PAGE</param>
        /// <returns>HTTP STATUS</returns>
        [HttpGet("{userId}/knowledgeBases")]
        public async Task<IActionResult> GetKnowledgeBasesByUserId(string userId, int pageIndex, int pageSize)
        {
            var query = from k in _context.KnowledgeBases
                        join c in _context.Categories on k.CategoryId equals c.Id
                        where k.OwnerUserId == userId
                        orderby k.CreateDate descending
                        select new { k, c };

            var totalRecords = await query.CountAsync();

            var items = await query.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
               .Select(u => new KnowledgeBaseQuickVm()
               {
                   Id = u.k.Id,
                   CategoryId = u.k.CategoryId.Value,
                   Description = u.k.Description,
                   SeoAlias = u.k.SeoAlias,
                   Title = u.k.Title,
                   CategoryAlias = u.c.SeoAlias,
                   CategoryName = u.c.Name,
                   NumberOfVotes = u.k.NumberOfVotes,
                   ViewCount = u.k.ViewCount,
                   CreateDate = u.k.CreateDate
               }).ToListAsync();

            var pagination = new Pagination<KnowledgeBaseQuickVm>
            {
                Items = items,
                TotalRecords = totalRecords,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            return Ok(pagination);
        }
        #endregion
    }
}
