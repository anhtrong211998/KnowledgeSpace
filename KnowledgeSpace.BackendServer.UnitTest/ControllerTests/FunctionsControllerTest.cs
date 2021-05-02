using KnowledgeSpace.BackendServer.Controllers;
using KnowledgeSpace.BackendServer.Models;
using KnowledgeSpace.BackendServer.Models.Entities;
using KnowledgeSpace.ViewModels;
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
    public class FunctionsControllerTest
    {
        #region DECLARE SERVICE AND CONSTRUCTOR
        private KnowledgeSpaceContext _context;

        public FunctionsControllerTest()
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
            var controller = new FunctionsController(_context);
            Assert.NotNull(controller);
        }

        #region TEST GET FUNCTION
        /// <summary>
        /// GET ALL FUNCTIONS.
        /// </summary>
        /// <returns>COUNT OF LIST OF FUNCTIONS IS GREATER THAN 0.</returns>
        [Fact]
        public async Task GetFunction_HasData_ReturnSuccess()
        {
            _context.Functions.AddRange(new List<Function>()
            {
                new Function(){
                    Id = "GetFunction_HasData_ReturnSuccess",
                    ParentId = null,
                    Name = "GetFunction_HasData_ReturnSuccess",
                    SortOrder =1,
                    Url ="/GetFunction_HasData_ReturnSuccess"
                }
            });
            await _context.SaveChangesAsync();
            var functionsController = new FunctionsController(_context);
            var result = await functionsController.GetFunctions();
            var okResult = result as OkObjectResult;
            var FunctionVms = okResult.Value as IEnumerable<FunctionVm>;
            Assert.True(FunctionVms.Count() > 0);
        }

        /// <summary>
        /// PAGINATION WITH NO FILTER (KEYWORD IS NULL).
        /// </summary>
        /// <returns>TOTAL RECORDS ARE 4 AND TOTAL ROWS IN PAGE ARE 2</returns>
        [Fact]
        public async Task GetFunctionsPaging_NoFilter_ReturnSuccess()
        {
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
            await _context.SaveChangesAsync();
            var functionsController = new FunctionsController(_context);
            var result = await functionsController.GetFunctionsPaging(null, 1, 2);
            var okResult = result as OkObjectResult;
            var FunctionVms = okResult.Value as Pagination<FunctionVm>;
            Assert.Equal(4, FunctionVms.TotalRecords);
            Assert.Equal(2, FunctionVms.Items.Count);
        }

        /// <summary>
        /// PAGINATION WITH FILTER (KEYWORID is GetFunctionsPaging_HasFilter_ReturnSuccess).
        /// </summary>
        /// <returns>TOTAL RECORDS ARE 1 AND TOTAL ROWS IN PAGE ARE SINGLE.</returns>
        [Fact]
        public async Task GetFunctionsPaging_HasFilter_ReturnSuccess()
        {
            _context.Functions.AddRange(new List<Function>()
            {
                new Function(){
                    Id = "GetFunctionsPaging_HasFilter_ReturnSuccess",
                    ParentId = null,
                    Name = "GetFunctionsPaging_HasFilter_ReturnSuccess",
                    SortOrder = 3,
                    Url ="/GetFunctionsPaging_HasFilter_ReturnSuccess"
                }
            });
            await _context.SaveChangesAsync();

            var functionsController = new FunctionsController(_context);
            var result = await functionsController.GetFunctionsPaging("GetFunctionsPaging_HasFilter_ReturnSuccess", 1, 2);
            var okResult = result as OkObjectResult;
            var FunctionVms = okResult.Value as Pagination<FunctionVm>;
            Assert.Equal(1, FunctionVms.TotalRecords);
            Assert.Single(FunctionVms.Items);
        }

        /// <summary>
        /// GET FUNCTION WITH ID (KEY).
        /// </summary>
        /// <returns>RESULT NOT NULL.</returns>
        [Fact]
        public async Task GetById_HasData_ReturnSuccess()
        {
            _context.Functions.AddRange(new List<Function>()
            {
                new Function(){
                    Id = "GetById_HasData_ReturnSuccess",
                    ParentId = null,
                    Name = "GetById_HasData_ReturnSuccess",
                    SortOrder =1,
                    Url ="/GetById_HasData_ReturnSuccess"
                }
            });
            await _context.SaveChangesAsync();
            var functionsController = new FunctionsController(_context);
            var result = await functionsController.GetById("GetById_HasData_ReturnSuccess");
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);

            var functionVm = okResult.Value as FunctionVm;
            Assert.Equal("GetById_HasData_ReturnSuccess", functionVm.Id);
        }
        #endregion

        #region TEST POST FUNCTION
        /// <summary>
        /// CREATE NEW FUNCTION SUCCESS.
        /// </summary>
        /// <returns>HTTP STATUS IS 201.</returns>
        [Fact]
        public async Task PostFunction_ValidInput_Success()
        {
            var functionsController = new FunctionsController(_context);
            var result = await functionsController.PostFunction(new FunctionCreateRequest()
            {
                Id = "PostFunction_ValidInput_Success",
                ParentId = null,
                Name = "PostFunction_ValidInput_Success",
                SortOrder = 5,
                Url = "/PostFunction_ValidInput_Success"
            });

            Assert.IsType<CreatedAtActionResult>(result);
        }

        /// <summary>
        /// CREATE NEW FUNCTION FAILED.
        /// </summary>
        /// <returns>HTTP STATUS IS 400.</returns>
        [Fact]
        public async Task PostFunction_ValidInput_Failed()
        {
            _context.Functions.AddRange(new List<Function>()
            {
                new Function(){
                    Id = "PostFunction_ValidInput_Failed",
                    ParentId = null,
                    Name = "PostFunction_ValidInput_Failed",
                    SortOrder =1,
                    Url ="/PostFunction_ValidInput_Failed"
                }
            });
            await _context.SaveChangesAsync();
            var functionsController = new FunctionsController(_context);

            var result = await functionsController.PostFunction(new FunctionCreateRequest()
            {
                Id = "PostFunction_ValidInput_Failed",
                ParentId = null,
                Name = "PostFunction_ValidInput_Failed",
                SortOrder = 5,
                Url = "/PostFunction_ValidInput_Failed"
            });

            Assert.IsType<BadRequestObjectResult>(result);
        }
        #endregion

        #region TEST PUT FUNCTION
        /// <summary>
        /// UPDATE FUNCTION SUCCESS.
        /// </summary>
        /// <returns>HTTP STATUS IS 204.</returns>
        [Fact]
        public async Task PutFunction_ValidInput_Success()
        {
            _context.Functions.AddRange(new List<Function>()
            {
                new Function(){
                    Id = "PutFunction_ValidInput_Success",
                    ParentId = null,
                    Name = "PutFunction_ValidInput_Success",
                    SortOrder =1,
                    Url ="/PutFunction_ValidInput_Success"
                }
            });
            await _context.SaveChangesAsync();
            var functionsController = new FunctionsController(_context);
            var result = await functionsController.PutFunction("PutFunction_ValidInput_Success", new FunctionCreateRequest()
            {
                ParentId = null,
                Name = "PutFunction_ValidInput_Success updated",
                SortOrder = 6,
                Url = "/PutFunction_ValidInput_Success"
            });
            Assert.IsType<NoContentResult>(result);
        }

        /// <summary>
        /// UPDATE FUNCTION FAILED.
        /// </summary>
        /// <returns>HTTP STATUS IS 400.</returns>
        [Fact]
        public async Task PutFunction_ValidInput_Failed()
        {
            var functionsController = new FunctionsController(_context);

            var result = await functionsController.PutFunction("PutFunction_ValidInput_Failed", new FunctionCreateRequest()
            {
                ParentId = null,
                Name = "PutFunction_ValidInput_Failed update",
                SortOrder = 6,
                Url = "/PutFunction_ValidInput_Failed"
            });
            Assert.IsType<NotFoundResult>(result);
        }
        #endregion

        #region TEST DELETE FUNCTION
        /// <summary>
        /// DELETE FUNCTION SUCCESS.
        /// </summary>
        /// <returns>HTTP STATUS IS 200.</returns>
        [Fact]
        public async Task DeleteFunction_ValidInput_Success()
        {
            _context.Functions.AddRange(new List<Function>()
            {
                new Function(){
                    Id = "DeleteFunction_ValidInput_Success",
                    ParentId = null,
                    Name = "DeleteFunction_ValidInput_Success",
                    SortOrder =1,
                    Url ="/DeleteFunction_ValidInput_Success"
                }
            });
            await _context.SaveChangesAsync();
            var functionsController = new FunctionsController(_context);
            var result = await functionsController.DeleteFunction("DeleteFunction_ValidInput_Success");
            Assert.IsType<OkObjectResult>(result);
        }

        /// <summary>
        /// DELETE FUNCTION FAILED.
        /// </summary>
        /// <returns>HTTP STATUS IS 404.</returns>
        [Fact]
        public async Task DeleteFunction_ValidInput_Failed()
        {
            var functionsController = new FunctionsController(_context);
            var result = await functionsController.DeleteFunction("DeleteFunction_ValidInput_Failed");
            Assert.IsType<NotFoundResult>(result);
        }
        #endregion
    }
}
