﻿using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using KnowledgeSpace.BackendServer.Constants;
using KnowledgeSpace.BackendServer.Models;
using KnowledgeSpace.BackendServer.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KnowledgeSpace.BackendServer.Services
{
    public class IdentityProfileService : IProfileService
    {
        private readonly IUserClaimsPrincipalFactory<User> _claimsFactory;
        private readonly UserManager<User> _userManager;
        private readonly KnowledgeSpaceContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IdentityProfileService(IUserClaimsPrincipalFactory<User> claimsFactory,
            UserManager<User> userManager,
            KnowledgeSpaceContext context,
           RoleManager<IdentityRole> roleManager)
        {
            _claimsFactory = claimsFactory;
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
        }

        /// <summary>
        /// GET PROFILE OF USER
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            if (user == null)
            {
                throw new ArgumentException("");
            }

            var principal = await _claimsFactory.CreateAsync(user);
            var claims = principal.Claims.ToList();
            var roles = await _userManager.GetRolesAsync(user);

            var query = from p in _context.Permissions
                        join c in _context.Commands
                        on p.CommandId equals c.Id
                        join f in _context.Functions
                        on p.FunctionId equals f.Id
                        join r in _roleManager.Roles on p.RoleId equals r.Id
                        where roles.Contains(r.Name)
                        select f.Id + "_" + c.Id;
            var permissions = await query.Distinct().ToListAsync();

            //// ADD MORE CLAIMS LIKE THIS
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            claims.Add(new Claim(ClaimTypes.Role, string.Join(";", roles)));
            claims.Add(new Claim(SystemConstants.Claims.Permissions, JsonConvert.SerializeObject(permissions)));

            context.IssuedClaims = claims;
        }

        /// <summary>
        /// CHECK IF USER IS ACTIVE OR NOT
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            context.IsActive = user != null;
        }
    }
}
