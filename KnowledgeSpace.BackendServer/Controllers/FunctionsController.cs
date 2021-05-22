using KnowledgeSpace.BackendServer.Authorization;
using KnowledgeSpace.BackendServer.Constants;
using KnowledgeSpace.BackendServer.Helpers;
using KnowledgeSpace.BackendServer.Models;
using KnowledgeSpace.BackendServer.Models.Entities;
using KnowledgeSpace.ViewModels;
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
    public class FunctionsController : BaseController
    {
        private readonly KnowledgeSpaceContext _context;

        /// <summary>
        /// CONSTRUCTOR CONTROLLER.
        /// </summary>
        /// <param name="context">DbContext.</param>
        public FunctionsController(KnowledgeSpaceContext context)
        {
            _context = context;
        }
        #region BUSSINESS FOR FUNCTION ONLY
        /// <summary>
        /// GET: api/Functions
        /// GET ALL FUNCTIONS.
        /// </summary>
        /// <returns>LIST OF FUNCTIONS.</returns>
        [HttpGet]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.VIEW)]
        public async Task<IActionResult> GetFunctions()
        {
            //// GET ALL FUNCTIONS
            var functions = _context.Functions;

            //// TAKE INFOMATIONS OF FUNCTION NEED SHOW
            var functionvms = await functions.Select(u => new FunctionVm()
            {
                Id = u.Id,
                Name = u.Name,
                Url = u.Url,
                SortOrder = u.SortOrder,
                ParentId = u.ParentId
            }).ToListAsync();

            return Ok(functionvms);
        }

        /// <summary>
        /// GET: api/Functions/?filter={filter}&pageIndex=1&pageSize=10
        /// GET FUNCTIONS WITH KEYWORD AND PAGINATION.
        /// </summary>
        /// <param name="filter">KEYWORD SEARCH.</param>
        /// <param name="pageIndex">INDEX OF NEXT PAGE.</param>
        /// <param name="pageSize">NUMBER OF RECORDS EACH PAGE.</param>
        /// <returns>HTTP STATUS WITH LIST OF FUNCTIONS.</returns>
        [HttpGet("filter")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.VIEW)]
        public async Task<IActionResult> GetFunctionsPaging(string filter, int pageIndex, int pageSize)
        {
            //// GET ALL FUNCTIONS
            var query = _context.Functions.AsQueryable();

            //// IF KEYWORD NOT NULL OR EMPTY, GET FUNCTIONS CONTAINS THIS KEYWORD
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Name.Contains(filter)
                || x.Id.Contains(filter)
                || x.Url.Contains(filter));
            }

            //// TOTAL RECORDS EQUAL NUMBER OF FUNCTION'S ROWS
            var totalRecords = await query.CountAsync();

            //// TAKE RECORDS IN THE PAGE (NEXT PAGE)
            var items = await query.Skip((pageIndex - 1 * pageSize))
                .Take(pageSize)
                .Select(u => new FunctionVm()
                {
                    Id = u.Id,
                    Name = u.Name,
                    Url = u.Url,
                    SortOrder = u.SortOrder,
                    ParentId = u.ParentId
                })
                .ToListAsync();

            //// PAGINATION.
            var pagination = new Pagination<FunctionVm>
            {
                Items = items,
                TotalRecords = totalRecords,
            };

            return Ok(pagination);
        }

        /// <summary>
        /// GET: api/Functions/{id}
        /// GET FUNCTION BY ID (KEY).
        /// </summary>
        /// <param name="id">KEY OF FUNCTION.</param>
        /// <returns>FUNCTION WITH KEY.</returns>
        [HttpGet("{id}")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.VIEW)]
        public async Task<IActionResult> GetById(string id)
        {
            //// GET FUNCTION WITH KEY INPUT
            var function = await _context.Functions.FindAsync(id);

            //// IF KEY NOT EXIST (FUNCTION IS NULL), RETURN STATUS IS 404
            if (function == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found function with id {id}"));

            //// GIVE INFO INTO FunctionVm (JUST SHOW NEEDED FIELD)
            var functionVm = new FunctionVm()
            {
                Id = function.Id,
                Name = function.Name,
                Url = function.Url,
                SortOrder = function.SortOrder,
                ParentId = function.ParentId
            };

            return Ok(functionVm);
        }

        /// <summary>
        /// POST: api/Functions
        /// CREATE NEW FUNCTIONS.
        /// </summary>
        /// <param name="request">INPUT FUNCTION's INFO.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpPost]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.CREATE)]
        [ApiValidationFilter]
        public async Task<IActionResult> PostFunction([FromBody] FunctionCreateRequest request)
        {
            //// GET FUNCTION WITH ID (KEY)
            var dbFunction = await _context.Functions.FindAsync(request.Id);

            //// IF RESULT NOT NULL, FUNCTION ALREADY EXISTS, RETURN STATUS 400
            if (dbFunction != null)
            {
                return BadRequest(new ApiBadRequestResponse($"Function with id {request.Id} is existed."));

            }
            //// CREATE A INSTANCE OF FUNCTION WITH INFO IS INPUT DATA
            var function = new Function()
            {
                Id = request.Id,
                Name = request.Name,
                ParentId = request.ParentId,
                SortOrder = request.SortOrder,
                Url = request.Url
            };

            //// INSERT NEW FUNCTION INTO DATATABLE IN DATABASE AND SAVE CHANGE
            _context.Functions.Add(function);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER ADD GREATER 0, INSERT SUCCESS AND RETURN STATUS 201, ELSE RETURN STATUS IS 400
            if (result > 0)
            {
                return CreatedAtAction(nameof(GetById), new { id = function.Id }, request);
            }
            else
            {
                return BadRequest(new ApiBadRequestResponse("Create function is failed"));
            }
        }

        /// <summary>
        /// PUT: api/Functions/{id}
        /// UPDATE FUNCTION WITH ID (KEY)
        /// </summary>
        /// <param name="id">KEY OF FUNCTION.</param>
        /// <param name="request">INFO NEED UPDATE (INPUT DATA).</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpPut("{id}")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.UPDATE)]
        [ApiValidationFilter]
        public async Task<IActionResult> PutFunction(string id, [FromBody] FunctionCreateRequest request)
        {
            //// GET FUNCTION WITH ID (KEY)
            var function = await _context.Functions.FindAsync(id);

            //// IF KEY IS NOT EXIST (FUNCTION IS NULL), RETURN STATUS 404
            if (function == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found function with id {id}"));

            //// GIVE INPUT DATA FOR EACH FIELD OF OBJECT WHICH NEED UPDATE INFOMATIONS
            function.Name = request.Name;
            function.ParentId = request.ParentId;
            function.SortOrder = request.SortOrder;
            function.Url = request.Url;

            //// UPDATE FUNCTION AND SAVE CHANGE INTO DATATABLE OF DATABASE
            _context.Functions.Update(function);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER UPDATE IS GREATER THAN 0, UPDATE SUCCESS, RETURN STATUS IS 204, ELSE RETURN STATUS IS 400
            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse("Create function failed"));
        }

        /// <summary>
        /// DELETE: api/Functions/{id}
        /// DELETE FUNCTION WITH ID (KEY).
        /// </summary>
        /// <param name="id">KEY OF FUNCTION.</param>
        /// <returns>HTTP STATUS</returns>
        [HttpDelete("{id}")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.DELETE)]
        public async Task<IActionResult> DeleteFunction(string id)
        {
            //// GET FUNCTION WITH ID (KEY).
            var function = await _context.Functions.FindAsync(id);

            //// IF KEY IS NOT EXIST (FUNCTION IS NULL), RETURN STATUS 404
            if (function == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found function with id {id}"));

            //// DELETE FUNCTION FROM DATASET (DATABASE INSTANCE) AND SAVE CHANGE
            _context.Functions.Remove(function);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER DELETE IS GREATER THAN 0, DELETE SUCCESS AND RETURN STATUS 200 WITH INFO OF THIS FUNCTION
            //// ELSE RETURN STATUS 400
            if (result > 0)
            {
                var functionvm = new FunctionVm()
                {
                    Id = function.Id,
                    Name = function.Name,
                    Url = function.Url,
                    SortOrder = function.SortOrder,
                    ParentId = function.ParentId
                };
                return Ok(functionvm);
            }
            return BadRequest(new ApiBadRequestResponse("Delete function failed"));
        }
        #endregion

        #region BUSSINESS FOR COMMAND OF FUNCTION
        /// <summary>
        /// GET: api/functions/{functionId}/commands
        /// GET ALL COMMANDS BY FUNCTION.
        /// </summary>
        /// <param name="functionId">KEY OF FUNCTION.</param>
        /// <returns>LIST COMMAND</returns>
        [HttpGet("{functionId}/commands")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.VIEW)]
        public async Task<IActionResult> GetCommantsInFunction(string functionId)
        {
            //// SELECT Id, Name AND FunctionId FROM Command TABLE, CommandInFunction TABLE AND Function TABLE
            //// WHERE functionId EQUAL CommandInFunction.FunctionId
            var query = from a in _context.Commands
                        join cif in _context.CommandInFunctions on a.Id equals cif.CommandId into result1
                        from commandInFunction in result1.DefaultIfEmpty()
                        join f in _context.Functions on commandInFunction.FunctionId equals f.Id into result2
                        from function in result2.DefaultIfEmpty()
                        select new
                        {
                            a.Id,
                            a.Name,
                            commandInFunction.FunctionId
                        };
            query = query.Where(x => x.FunctionId == functionId);

            //// GIVE INFO INTO CommandVm (JUST SHOW NEEDED FIELD)
            var data = await query.Select(x => new CommandVm()
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();

            return Ok(data);
        }

        /// <summary>
        /// GET: api/functions/{functionId}/commands/not-in-function
        /// GET COMMAND NOT IN FUNCTION.
        /// </summary>
        /// <param name="functionId">KEY OF FUNCTION.</param>
        /// <returns></returns>
        [HttpGet("{functionId}/commands/not-in-function")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.VIEW)]
        public async Task<IActionResult> GetCommantsNotInFunction(string functionId)
        {
            //// SELECT Id, Name AND FunctionId FROM Command TABLE, CommandInFunction TABLE AND Function TABLE
            //// WHERE functionId NOT EQUAL CommandInFunction.FunctionId
            var query = from a in _context.Commands
                        join cif in _context.CommandInFunctions on a.Id equals cif.CommandId into result1
                        from commandInFunction in result1.DefaultIfEmpty()
                        join f in _context.Functions on commandInFunction.FunctionId equals f.Id into result2
                        from function in result2.DefaultIfEmpty()
                        select new
                        {
                            a.Id,
                            a.Name,
                            commandInFunction.FunctionId
                        };
            query = query.Where(x => x.FunctionId != functionId).Distinct();

            //// GIVE INFO INTO CommandVm (JUST SHOW NEEDED FIELD)
            var data = await query.Select(x => new CommandVm()
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();

            return Ok(data);
        }

        /// <summary>
        /// POST: api/functions/{functionId}/commands
        /// CREATE NEW COMMAND FOR FUNCTION.
        /// </summary>
        /// <param name="functionId">KEY OF FUNCTION.</param>
        /// <param name="request">INPUT COMMAND's INFO</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpPost("{functionId}/commands")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.CREATE)]
        [ApiValidationFilter]
        public async Task<IActionResult> PostCommandToFunction(string functionId, [FromBody] AddCommandToFunctionRequest request)
        {
            //// GET COMMAND BY FUNCTION (CommandId,FunctionId)
            var commandInFunction = await _context.CommandInFunctions.FindAsync(request.CommandId, request.FunctionId);

            //// IF RESULT NOT NULL, COMMAND ALREADY EXIST, CREATE FAILED, RETURN STATUS 400
            if (commandInFunction != null)
            {
                return BadRequest(new ApiBadRequestResponse($"This command has been added to function"));
            }

            //// CREATE NEW INSTANCE OF COMMAND IN FUNCTION
            var entity = new CommandInFunction()
            {
                CommandId = request.CommandId,
                FunctionId = request.FunctionId
            };

            //// INSERT NEW COMMAND INTO DATABASE AND SAVE CHANGE
            _context.CommandInFunctions.Add(entity);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER INSERT IS GREATER THAN 0, ADD SUCCESS AND RETURN STATUS 201, ELSE RETURN STATUS 400
            if (result > 0)
            {
                return CreatedAtAction(nameof(GetById), new { commandId = request.CommandId, functionId = request.FunctionId }, request);
            }
            else
            {
                return BadRequest(new ApiBadRequestResponse("Add command to function failed"));
            }
        }

        /// <summary>
        /// DELETE: api/functions/{functionId}/commands/{commandId}
        /// DELETE COMMAND OF FUNCTION.
        /// </summary>
        /// <param name="functionId">KEY OF FUNCTION.</param>
        /// <param name="commandId">KEY OF COMMAND.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpDelete("{functionId}/commands/{commandId}")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.DELETE)]
        public async Task<IActionResult> DeleteCommandInFunction(string functionId, string commandId)
        {
            //// GET COMMAND BY FUNCTION (FunctionId, CommandId)
            var commandInFunction = await _context.CommandInFunctions.FindAsync(functionId, commandId);

            //// IF RESULT IS NULL, RETURN STATUS 404
            if (commandInFunction == null)
                return BadRequest(new ApiBadRequestResponse($"This command is not existed in function"));

            //// CREATE NEW INSTANCE OF COMMAND IN FUNCTION
            var entity = new CommandInFunction()
            {
                CommandId = commandId,
                FunctionId = functionId
            };

            //// DELETE COMMAND FROM DATABASE AND SAVE CHANGE
            _context.CommandInFunctions.Remove(entity);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER DELETE GREATER THAN 0 (TRUE), RETURN STATS 200, ELSE RETURN STATUS 400
            if (result > 0)
            {
                return Ok();
            }
            else
            {
                return BadRequest(new ApiBadRequestResponse("Delete command to function failed"));
            }
        }
        #endregion

    }
}
