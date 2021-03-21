using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KnowledgeSpace.ViewModels;
using KnowledgeSpace.ViewModels.Systems;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeSpace.BackendServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManger;

        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManger = roleManager;
        }

        //// URL: GET: /api/roles
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var role =  _roleManger.Roles;

            var roleVms = await role.Select(r => new RoleVm()
            {
                Id = r.Id,
                Name = r.Name
            }).ToListAsync();

            return Ok(roleVms);
        }

        //// URL: GET: /api/roles/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var role = await _roleManger.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var roleVm = new RoleVm()
            {
                Id = role.Id,
                Name = role.Name,
            };

            return Ok(roleVm);
        }

        //// URL: GET: /api/roles/?filter={filter}&pageIndex=1&pageSize=10
        [HttpGet("filter")]
        public async Task<IActionResult> GetRolesPagin(string filter, int pageIndex, int pageSize)
        {
            var roles =  _roleManger.Roles;

            //// if we want search filter which is not null
            if (!string.IsNullOrEmpty(filter))
            {
                roles = roles.Where(r => r.Id.Contains(filter) ||
                                    r.Name.ToLower().Contains(filter.ToLower())
                                    ); 
            }

            //// get number of records
            var totalRecords = await roles.CountAsync();

            //// get items per page
            var items = await roles.Skip((pageIndex - 1) * pageSize)
                             .Take(pageSize)
                             .Select(r => new RoleVm() {
                                 Id = r.Id,
                                 Name = r.Name
                             }).ToListAsync();

            var pagination = new Pagination<RoleVm>
            {
                Items = items,
                TotalRecords = totalRecords
            };

            return Ok(pagination);
        }

        //// URL: POST: /api/roles
        [HttpPost]
        public async Task<IActionResult> PostRole(RoleVm roleVm)
        {
            var role = new IdentityRole()
            {
                Id = roleVm.Id,
                Name = roleVm.Name,
                NormalizedName = roleVm.Name.ToUpper()
            };

            var result = await _roleManger.CreateAsync(role);

            if (result.Succeeded)
            {
                return CreatedAtAction(nameof(GetById), new { id = role.Id }, roleVm);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        //// URL: PUT: /api/roles/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRole(string id, RoleVm roleVm)
        {
            if(id != roleVm.Id)
            {
                return BadRequest();
            }

            var role = await _roleManger.FindByIdAsync(id);
            if(role == null)
            {
                return NotFound();
            }

            role.Name = roleVm.Name;
            role.NormalizedName = roleVm.Name.ToUpper();

            var result = await _roleManger.UpdateAsync(role);
            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Errors);
        }

        //// URL: Delete: /api/roles/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManger.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var result = await _roleManger.DeleteAsync(role);
            if (result.Succeeded)
            {
                var roleVm = new RoleVm()
                {
                    Id = role.Id,
                    Name = role.Name
                };
                return Ok(roleVm);
            }

            return BadRequest(result.Errors);
        }

    }
}
