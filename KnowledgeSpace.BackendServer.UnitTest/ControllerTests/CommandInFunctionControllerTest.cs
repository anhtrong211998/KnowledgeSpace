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
    public class CommandInFunctionControllerTest
    {
        #region DECLARE SERVICE AND CONSTRUCTOR
        private KnowledgeSpaceContext _context;

        public CommandInFunctionControllerTest()
        {
            _context = new InMemoryDbContextFactory().GetApplicationDbContext();

            _context.Functions.AddRange(new List<Function>()
            {
                new Function(){
                    Id = "GetFunctionsPaging_NoFilter_ReturnSuccess1",
                    ParentId = null,
                    Name = "GetFunctionsPaging_NoFilter_ReturnSuccess1",
                    SortOrder =1,
                    Url ="/test1"
                },
                 new Function(){
                    Id = "GetFunctionsPaging_NoFilter_ReturnSuccess2",
                    ParentId = null,
                    Name = "GetFunctionsPaging_NoFilter_ReturnSuccess2",
                    SortOrder =2,
                    Url ="/test2"
                },
                  new Function(){
                    Id = "GetFunctionsPaging_NoFilter_ReturnSuccess3",
                    ParentId = null,
                    Name = "GetFunctionsPaging_NoFilter_ReturnSuccess3",
                    SortOrder = 3,
                    Url ="/test3"
                },
                   new Function(){
                    Id = "GetFunctionsPaging_NoFilter_ReturnSuccess4",
                    ParentId = null,
                    Name = "GetFunctionsPaging_NoFilter_ReturnSuccess4",
                    SortOrder =4,
                    Url ="/test4"
                }
            });
             _context.SaveChangesAsync();

            _context.Commands.AddRange(new List<Command>()
                {
                    new Command(){Id = "TEST1", Name = "Xem"},
                    new Command(){Id = "TEST2", Name = "Thêm"},
                    new Command(){Id = "TEST3", Name = "Sửa"},
                    new Command(){Id = "TEST4", Name = "Xoá"},
                    new Command(){Id = "TEST5", Name = "Duyệt"},
                });

            var functions = _context.Functions;

            if (!_context.CommandInFunctions.Any())
            {
                foreach (var function in functions)
                {
                    var createAction = new CommandInFunction()
                    {
                        CommandId = "TEST2",
                        FunctionId = function.Id
                    };
                    _context.CommandInFunctions.Add(createAction);

                    var updateAction = new CommandInFunction()
                    {
                        CommandId = "TEST3",
                        FunctionId = function.Id
                    };
                    _context.CommandInFunctions.Add(updateAction);
                    var deleteAction = new CommandInFunction()
                    {
                        CommandId = "TEST4",
                        FunctionId = function.Id
                    };
                    _context.CommandInFunctions.Add(deleteAction);

                    var viewAction = new CommandInFunction()
                    {
                        CommandId = "TEST1",
                        FunctionId = function.Id
                    };
                    _context.CommandInFunctions.Add(viewAction);
                }
            }

            _context.SaveChangesAsync();
        }
        #endregion

        /// TEST CONSTRUCTOR CONTROLLER.
        /// </summary>
        [Fact]
        public void Should_Create_Instance_Not_Null_Success()
        {
            var controller = new FunctionsController(_context);
            Assert.NotNull(controller);
        }

        /// <summary>
        /// TEST GET COMMAND BY FUNCTION.
        /// </summary>
        /// <returns>LIST OF COMMANDS IN FUNCTION ARE GREATER THAN 0</returns>
        [Fact]
        public async Task GetCommantsInFunction_HasData_ReturnSuccess()
        {
            var commandsController = new FunctionsController(_context);
            var result = await commandsController.GetCommantsInFunction("GetFunctionsPaging_NoFilter_ReturnSuccess1");
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);

            var commandVm = okResult.Value as IEnumerable<CommandVm>;
            Assert.True(commandVm.Count() > 0); ;
        }

        /// <summary>
        /// TEST GET COMMAND WITHOUT FUNCTION (NOT IN FUNCTION).
        /// </summary>
        /// <returns>LIST OF COMMAND ARE GREATER THAN 0.</returns>
        [Fact]
        public async Task GetCommantsNotInFunction_HasData_ReturnSuccess()
        {
            var commandsController = new FunctionsController(_context);
            var result = await commandsController.GetCommantsNotInFunction("GetFunctionsPaging_NoFilter_ReturnSuccess1");
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);

            var commandVm = okResult.Value as IEnumerable<CommandVm>;
            Assert.True(commandVm.Count() > 0); ;
        }



    }
}
