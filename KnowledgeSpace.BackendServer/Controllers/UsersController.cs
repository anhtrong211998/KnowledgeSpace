using KnowledgeSpace.BackendServer.Authorization;
using KnowledgeSpace.BackendServer.Constants;
using KnowledgeSpace.BackendServer.Models;
using KnowledgeSpace.BackendServer.Models.Entities;
using KnowledgeSpace.ViewModels;
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
                LastName = u.LastName
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
            var items = await query.Skip((pageIndex - 1 * pageSize))
                .Take(pageSize)
                .Select(u => new UserVm()
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Dob = u.Dob,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    FirstName = u.FirstName,
                    LastName = u.LastName
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
                return NotFound();

            //// GIVE INFO INTO UserVn (JUST SHOW NEEDED FIELD)
            var userVm = new UserVm()
            {
                Id = user.Id,
                UserName = user.UserName,
                Dob = user.Dob,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName
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
        public async Task<IActionResult> PostUser(UserCreateRequest request)
        {
            //// CREATE NEW INSTANCE OF USER WITH INFO IS INPUT DATA
            var user = new User()
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                Dob = request.Dob,
                UserName = request.UserName,
                LastName = request.LastName,
                FirstName = request.FirstName,
                PhoneNumber = request.PhoneNumber
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
                return BadRequest(result.Errors);
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
                return NotFound();
            }

            //// GIVE INPUT DATA FOR EACH FIELD OF OBJECT WHICH NEED UPDATE INFOMATIONS
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Dob = request.Dob;

            //// UPDATE USER AND SAVE CHANGE INTO DATATABLE IN DATABASE
            var result = await _userManager.UpdateAsync(user);

            //// IF RESULT AFTER UPDATE IS SUCCEEDED, RETURN STATUS 201, ELSE RETURN STATUS 400
            if (result.Succeeded)
            {
                return NoContent();
            }
            return BadRequest(result.Errors);
        }

        /// <summary>
        /// CHANGE PASSWORD OF USER.
        /// </summary>
        /// <param name="id">KEY OF USER.</param>
        /// <param name="request">CURRENT PASSWORD AND NEW PASSWORD.</param>
        /// <returns></returns>
        [HttpPut("{id}/change-password")]
        [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.UPDATE)]
        public async Task<IActionResult> PutUserPassword(string id, [FromBody] UserPasswordChangeRequest request)
        {
            //// GET USER WITH ID (KEY)
            var user = await _userManager.FindByIdAsync(id);

            //// IF KEY IS NOT EXSIST (USER IS NULL), RETURN STATUS 404
            if (user == null)
            {
                return NotFound();
            }

            //// CHANGE PASSWORD USE SERVICE OF IDENTITYSERVER4
            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            //// IF RESULT IS SUCCEEDED, CHANGE SUCCESS AND RETURN STATUS 201, ELSE RETURN STATUS 400
            if (result.Succeeded)
            {
                return NoContent();
            }
            return BadRequest(result.Errors);
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
                return NotFound();
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
                    LastName = user.LastName
                };
                return Ok(uservm);
            }
            return BadRequest(result.Errors);
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
                        };
            var data = await query.Distinct()
                .OrderBy(x => x.ParentId)
                .ThenBy(x => x.SortOrder)
                .ToListAsync();
            return Ok(data);
        }
        #endregion
    }
}
