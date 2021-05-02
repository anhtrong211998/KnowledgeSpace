using KnowledgeSpace.BackendServer.Models;
using KnowledgeSpace.ViewModels.Systems;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KnowledgeSpace.BackendServer.Controllers
{
    public class CommandsController : BaseController
    {
        private readonly KnowledgeSpaceContext _context;

        /// <summary>
        /// CONSTRUCTOR CONTROLLER.
        /// </summary>
        /// <param name="context">DbContext.</param>
        public CommandsController(KnowledgeSpaceContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET ALL COMMANTS.
        /// </summary>
        /// <returns>LIST OF COMMANDS.</returns>
        [HttpGet()]
        public async Task<IActionResult> GetCommands()
        {
            var commands = _context.Commands;

            var commandVms = await commands.Select(u => new CommandVm()
            {
                Id = u.Id,
                Name = u.Name,
            }).ToListAsync();

            return Ok(commandVms);
        }
    }
}
