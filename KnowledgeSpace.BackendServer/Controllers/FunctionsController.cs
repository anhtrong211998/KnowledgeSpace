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

        /// <summary>
        /// GET: api/Functions
        /// GET ALL FUNCTIONS.
        /// </summary>
        /// <returns>LIST OF FUNCTIONS.</returns>
        [HttpGet]
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
        public async Task<IActionResult> GetById(string id)
        {
            //// GET FUNCTION WITH KEY INPUT
            var function = await _context.Functions.FindAsync(id);

            //// IF KEY NOT EXIST (FUNCTION IS NULL), RETURN STATUS IS 404
            if (function == null)
                return NotFound();

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
        public async Task<IActionResult> PostFunction([FromBody] FunctionCreateRequest request)
        {
            var dbFunction = await _context.Functions.FindAsync(request.Id);
            if(dbFunction != null)
            {
                return BadRequest($"Function with key {request.Id} is already exists!");
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
                return BadRequest();
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
        public async Task<IActionResult> PutFunction(string id, [FromBody] FunctionCreateRequest request)
        {
            //// GET FUNCTION WITH ID (KEY)
            var function = await _context.Functions.FindAsync(id);

            //// IF KEY IS NOT EXIST (FUNCTION IS NULL), RETURN STATUS 404
            if (function == null)
                return NotFound();

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
            return BadRequest();
        }

        /// <summary>
        /// DELETE: api/Functions/{id}
        /// DELETE FUNCTION WITH ID (KEY).
        /// </summary>
        /// <param name="id">KEY OF FUNCTION.</param>
        /// <returns>HTTP STATUS</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFunction(string id)
        {
            //// GET FUNCTION WITH ID (KEY).
            var function = await _context.Functions.FindAsync(id);

            //// IF KEY IS NOT EXIST (FUNCTION IS NULL), RETURN STATUS 404
            if (function == null)
                return NotFound();

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
            return BadRequest();
        }
    }
}
