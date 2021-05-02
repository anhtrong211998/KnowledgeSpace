using KnowledgeSpace.BackendServer.Controllers;
using KnowledgeSpace.BackendServer.Models;
using KnowledgeSpace.BackendServer.Models.Entities;
using KnowledgeSpace.ViewModels.Systems;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace KnowledgeSpace.BackendServer.UnitTest.ControllerTests
{
    public class CommandsControllerTest
    {
        #region DECLARE SERVICE AND CONSTRUCTOR
        private KnowledgeSpaceContext _context;

        public CommandsControllerTest()
        {
            _context = new InMemoryDbContextFactory().GetApplicationDbContext();
        }
        #endregion

        /// <summary>
        /// TEST CONSTRUCTOR CONTROLLER.
        /// </summary>
        [Fact]
        public void Should_Create_Instance_Not_Null_Success()
        {
            var controller = new CommandsController(_context);
            Assert.NotNull(controller);
        }

        #region TEST GET COMMAND
        /// <summary>
        /// GET ALL FUNCTIONS.
        /// </summary>
        /// <returns>COUNT OF LIST OF FUNCTIONS IS GREATER THAN 0.</returns>
        [Fact]
        public async Task GetCommands_HasData_ReturnSuccess()
        {
            _context.Commands.AddRange(new List<Command>()
                {
                    new Command(){Id = "TEST1", Name = "Xem"},
                    new Command(){Id = "TEST2", Name = "Thêm"},
                    new Command(){Id = "TEST3", Name = "Sửa"},
                    new Command(){Id = "TEST4", Name = "Xoá"},
                    new Command(){Id = "TEST5", Name = "Duyệt"},
                });
            await _context.SaveChangesAsync();

            var commandsController = new CommandsController(_context);
            var result = await commandsController.GetCommands();
            var okResult = result as OkObjectResult;
            var FunctionVms = okResult.Value as IEnumerable<CommandVm>;
            Assert.True(FunctionVms.Count() > 0);
        }
        #endregion
    }
}