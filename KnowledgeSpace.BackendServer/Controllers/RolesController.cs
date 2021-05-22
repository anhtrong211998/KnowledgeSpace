using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KnowledgeSpace.BackendServer.Authorization;
using KnowledgeSpace.BackendServer.Constants;
using KnowledgeSpace.BackendServer.Helpers;
using KnowledgeSpace.BackendServer.Models;
using KnowledgeSpace.BackendServer.Models.Entities;
using KnowledgeSpace.ViewModels;
using KnowledgeSpace.ViewModels.Systems;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeSpace.BackendServer.Controllers
{

    public class RolesController : BaseController
    {
        private readonly RoleManager<IdentityRole> _roleManger;
        private readonly KnowledgeSpaceContext _context;

        /// <summary>
        /// CONSTRUCTOR CONTROLLER.
        /// </summary>
        /// <param name="roleManager">ROLE MANAGER SERVICE.</param>
        public RolesController(RoleManager<IdentityRole> roleManager,
            KnowledgeSpaceContext context)
        {
            _roleManger = roleManager;
            _context = context;
        }

        #region MANAGER ROLE
        /// <summary>
        /// GET: api/Roles
        /// GET ALL ROLES.
        /// </summary>
        /// <returns>LIST OF ROLES.</returns>
        [HttpGet]
        [ClaimRequirement(FunctionCode.SYSTEM_ROLE, CommandCode.VIEW)]
        public async Task<IActionResult> GetAll()
        {
            //// GET ALL ROLES
            var role = _roleManger.Roles;

            //// TAKE INFOMATIONS OF ROLE NEED SHOW
            var roleVms = await role.Select(r => new RoleVm()
            {
                Id = r.Id,
                Name = r.Name
            }).ToListAsync();

            return Ok(roleVms);
        }

        /// <summary>
        /// GET: api/Roles/{id}
        /// GET ROLE WITH ID (KEY).
        /// </summary>
        /// <param name="id">KEY OF ROLE.</param>
        /// <returns>ROLE WITH KEY.</returns>
        [HttpGet("{id}")]
        [ClaimRequirement(FunctionCode.SYSTEM_ROLE, CommandCode.VIEW)]
        public async Task<IActionResult> GetById(string id)
        {
            //// GET ROLE WITH ID (KEY)
            var role = await _roleManger.FindByIdAsync(id);

            //// IF KEY IS NOT EXIST (ROLE IS NULL), RETURN STATUS 404
            if (role == null)
            {
                return NotFound(new ApiNotFoundResponse($"Cannot find role with id: {id}"));
            }

            //// GIVE INFO TO RoleVm (JUST SHOW NEEDED FIELD)
            var roleVm = new RoleVm()
            {
                Id = role.Id,
                Name = role.Name,
            };

            return Ok(roleVm);
        }

        /// <summary>
        /// GET: /api/roles/?filter={filter}&pageIndex=1&pageSize=10
        /// GET ROLES WITH KEYWORD AND PAGINATION.
        /// </summary>
        /// <param name="filter">KEYWORD SEARCH.</param>
        /// <param name="pageIndex">INDEX OF NEXT PAGE.</param>
        /// <param name="pageSize">NUMBER OF RECORDS EACH PAGE.</param>
        /// <returns>HTTP STATUS WITH LIST OF ROLES.</returns>
        [HttpGet("filter")]
        [ClaimRequirement(FunctionCode.SYSTEM_ROLE, CommandCode.VIEW)]
        public async Task<IActionResult> GetRolesPagin(string filter, int pageIndex, int pageSize)
        {
            //// GET ALL ROLES.
            var roles = _roleManger.Roles;

            //// IF KEYWORD NOT NULL OR EMPTY, GET ROLES CONTAINS THIS KEYWORD
            if (!string.IsNullOrEmpty(filter))
            {
                roles = roles.Where(r => r.Id.Contains(filter)
                                      || r.Name.ToLower().Contains(filter.ToLower())
                                    );
            }

            //// TOTAL RECORDS EQUAL NUMBER OF ROLE's ROWS
            var totalRecords = await roles.CountAsync();

            //// TAKE RECORDS IN THE PAGE (NEXT PAGE)
            var items = await roles.Skip((pageIndex - 1) * pageSize)
                             .Take(pageSize)
                             .Select(r => new RoleVm()
                             {
                                 Id = r.Id,
                                 Name = r.Name
                             }).ToListAsync();

            //// PAGINATION
            var pagination = new Pagination<RoleVm>
            {
                Items = items,
                TotalRecords = totalRecords
            };

            return Ok(pagination);
        }

        /// <summary>
        /// POST: api/Roles
        /// CREATE NEW ROLE.
        /// </summary>
        /// <param name="request">INPUT ROLE's INFO.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpPost]
        [ClaimRequirement(FunctionCode.SYSTEM_ROLE, CommandCode.CREATE)]
        [ApiValidationFilter]
        public async Task<IActionResult> PostRole(RoleCreateRequest request)
        {
            //// CREATE A INSTANCE OF ROLE WITH INFO IS INPUT DATA
            var role = new IdentityRole()
            {
                Id = request.Id,
                Name = request.Name,
                NormalizedName = request.Name.ToUpper()
            };

            //// INSERT NEW ROLE INTO DATATABLE IN DATABASE AND SAVE CHANGE
            var result = await _roleManger.CreateAsync(role);

            //// IF RESULT AFTER INSERT IS SUCCEEDED, INSERT SUCCESS AND RETURN STATUS 201, ELSE RETURN STATUS IS 400.
            if (result.Succeeded)
            {
                return CreatedAtAction(nameof(GetById), new { id = role.Id }, request);
            }
            else
            {
                return BadRequest(new ApiBadRequestResponse(result));
            }
        }

        /// <summary>
        /// PUT: api/Roles/{id}
        /// UPDATE ROLE WITH ID (KEY).
        /// </summary>
        /// <param name="id">KEY OF ROLE.</param>
        /// <param name="request">INFO NEED UPDATE (INPUT DATA).</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpPut("{id}")]
        [ClaimRequirement(FunctionCode.SYSTEM_ROLE, CommandCode.UPDATE)]
        [ApiValidationFilter]
        public async Task<IActionResult> PutRole(string id, RoleCreateRequest request)
        {
            //// IF ID(request.ID) INPUT AND id (KEY OF ROLE WHICH NEED UPDATE) ARE DIFFERENT, RETURN STATUS 400 
            if (id != request.Id)
            {
                return BadRequest(new ApiBadRequestResponse("Role id not match"));
            }

            //// GET ROLE WITH ID
            var role = await _roleManger.FindByIdAsync(id);

            //// IF ID IS NOT EXIST (ROLE IS NULL), RETURN STATUS 404
            if (role == null)
            {
                return NotFound(new ApiNotFoundResponse($"Cannot find role with id: {id}"));
            }

            //// GIVE INPUT DATA FOR EACH FIELD OF OBJECT WHICH NEED UPDATE INFOMATIONS
            role.Name = request.Name;
            role.NormalizedName = request.Name.ToUpper();

            //// UPDATE ROLE AND SAVE CHANGE INTO DATATABLE OF DATABASE
            var result = await _roleManger.UpdateAsync(role);

            //// IF RESULT AFTER UPDATE IS SUCCEEDED, UPDATE SUCCESS AND RETURN STATUS 204, ELSE RETURN STATUS 400
            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(new ApiBadRequestResponse(result));
        }

        /// <summary>
        /// DELETE : api/Roles/{id}
        /// DELETE ROLE WITH ID (KEY).
        /// </summary>
        /// <param name="id">KEY OF ROLE.</param>
        /// <returns>HTTP STATUS CODE.</returns>
        [HttpDelete("{id}")]
        [ClaimRequirement(FunctionCode.SYSTEM_ROLE, CommandCode.DELETE)]
        public async Task<IActionResult> DeleteRole(string id)
        {
            //// GET ROLE WITH ID (KEY)
            var role = await _roleManger.FindByIdAsync(id);

            //// IF KEY IS NOT EXIST (ROLE IS NULL), RETURN STATUS 404
            if (role == null)
            {
                return NotFound(new ApiNotFoundResponse($"Cannot find role with id: {id}"));
            }

            //// DELETE ROLE FROM DATATABLE IN DATABASE AND SAVE CHANGE
            var result = await _roleManger.DeleteAsync(role);

            //// IF RESULT AFTER DELETE IS SUCCEEDED, DELETE SUCCESS AND RETURN STATUS 200, ELSE RETURN STATUS 400
            if (result.Succeeded)
            {
                var roleVm = new RoleVm()
                {
                    Id = role.Id,
                    Name = role.Name
                };
                return Ok(roleVm);
            }

            return BadRequest(new ApiBadRequestResponse(result));
        }
        #endregion

        #region MANAGER PERMISSIONS IN EACH FUNCTION
        /// <summary>
        /// GET ALL PERMISSIONS BY ROLE ID (KEY OF ROLE).
        /// </summary>
        /// <param name="roleId">KEY OF ROLE.</param>
        /// <returns>HTTP STATUS 200 WITH LIST OF PERMISSION.</returns>
        [HttpGet("{roleId}/permissions")]
        [ClaimRequirement(FunctionCode.SYSTEM_PERMISSION, CommandCode.VIEW)]
        public async Task<IActionResult> GetPermissionByRoleId(string roleId)
        {
            //// GET ALL PERMISION IN FUNCTION WITH ID EXIST AND RETURN HTTP STATUS 200 WITH LIST OF PERMISSION
            var permissions = from p in _context.Permissions

                              join a in _context.Commands
                              on p.CommandId equals a.Id
                              where p.RoleId == roleId
                              select new PermissionVm()
                              {
                                  FunctionId = p.FunctionId,
                                  CommandId = p.CommandId,
                                  RoleId = p.RoleId
                              };

            return Ok(await permissions.ToListAsync());
        }

        /// <summary>
        /// UPDATE PERMISSION BY ROLE ID (KEY OF ROLE).
        /// </summary>
        /// <param name="roleId">KEY OF ROLE.</param>
        /// <param name="request">INFO PERMISSION NEED UPDATE (INPUT DATA).</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpPut("{roleId}/permissions")]
        [ClaimRequirement(FunctionCode.SYSTEM_PERMISSION, CommandCode.UPDATE)]
        [ApiValidationFilter]
        public async Task<IActionResult> PutPermissionByRoleId(string roleId, [FromBody] UpdatePermissionRequest request)
        {
            //create new permission list from user changed
            //// CREATE NEW LIST PERMISSION TO SAVE PERMISSION THOSE USER CHANGED 
            var newPermissions = new List<Permission>();
            foreach (var p in request.Permissions)
            {
                newPermissions.Add(new Permission(p.FunctionId, roleId, p.CommandId));
            }

            //// GET ALL PERMISSION WITH ROLE ID (KEY OF ROLE) WHICH EXIST IN DATABASE AND REMOVE THEM
            var existingPermissions = _context.Permissions.Where(x => x.RoleId == roleId);
            _context.Permissions.RemoveRange(existingPermissions);

            //// ADD NEW PERMISSIONS INTO DATABASE TO CHANGE PERMISSION OF THAT ROLE AND SAVE CHANGE
            _context.Permissions.AddRange(newPermissions);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER UPDATE IS GREATER THAN 0 (TRUE), RETURN STATUS 201, ELSE RETURN 400
            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse("Save permission failed"));
        }
        #endregion
    }
}
