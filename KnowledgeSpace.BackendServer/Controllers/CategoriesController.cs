using KnowledgeSpace.BackendServer.Authorization;
using KnowledgeSpace.BackendServer.Constants;
using KnowledgeSpace.BackendServer.Helpers;
using KnowledgeSpace.BackendServer.Models;
using KnowledgeSpace.BackendServer.Models.Entities;
using KnowledgeSpace.ViewModels;
using KnowledgeSpace.ViewModels.Contents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KnowledgeSpace.BackendServer.Controllers
{
    public class CategoriesController : BaseController
    {
        private readonly KnowledgeSpaceContext _context;

        /// <summary>
        /// CONSTRUCTOR CONTROLLER.
        /// </summary>
        /// <param name="context"></param>
        public CategoriesController(KnowledgeSpaceContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: api/categories
        /// GET ALL CATEGORIES.
        /// </summary>
        /// <returns>HTTP STATUS WITH LIST OF CATEGORIES.</returns>
        [HttpGet]
        [ClaimRequirement(FunctionCode.CONTENT_CATEGORY, CommandCode.VIEW)]
        public async Task<IActionResult> GetCategories()
        {
            //// GET ALL CATEGORIES FROM DATABASE
            var categorys = await _context.Categories.ToListAsync();

            //// TAKE INFORMATIONS OF CATEGORY NEED SHOW AND RETURN STATUS 200
            var categoryvms = categorys.Select(c => CreateCategoryVm(c)).ToList();

            return Ok(categoryvms);
        }

        /// <summary>
        /// GET CATEGORIES WITH KEYWORD AND PAGINATION THEM.
        /// </summary>
        /// <param name="filter">KEYWORD SEARCH.</param>
        /// <param name="pageIndex">INDEX OF NEXT PAGE.</param>
        /// <param name="pageSize">NUMBER OF RECORDS IN EACH PAGE.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpGet("filter")]
        [ClaimRequirement(FunctionCode.CONTENT_CATEGORY, CommandCode.VIEW)]
        public async Task<IActionResult> GetCategoriesPaging(string filter, int pageIndex, int pageSize)
        {
            //// GET ALL CATEGORIES
            var query = _context.Categories.AsQueryable();
            //// IF KEYWORD NOT NULL, GET ALL CATEGORIES WHICH CONSTAINS KEYWORD
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Name.Contains(filter)
                || x.Name.Contains(filter));
            }
            //// TOTAL RECORDS EQUAL NUMBER OF CATEGORIES's ROW
            var totalRecords = await query.CountAsync();
            //// TAKE RECORDS IN THE PAGE (NEXT PAGE)
            var items = await query.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize).ToListAsync();

            //// GIVE INFORMATIONS TO CategoryVm (JUST SHOW FIELD NEEDED)
            var data = items.Select(c => CreateCategoryVm(c)).ToList();

            //// PAGINATION
            var pagination = new Pagination<CategoryVm>
            {
                Items = data,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }

        /// <summary>
        /// GET CATEGOR WITH ID (KEY OF CATEGORY).
        /// </summary>
        /// <param name="id">KEY OF CATEGORY.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpGet("{id}")]
        [ClaimRequirement(FunctionCode.CONTENT_CATEGORY, CommandCode.VIEW)]
        public async Task<IActionResult> GetById(int id)
        {
            //// GET CATEGORY WITH ID (KEY)
            var category = await _context.Categories.FindAsync(id);
            //// IF KEY IS NOT EXIST (CATEGORY IS NULL), RETURN STATUS 404
            if (category == null)
                return NotFound(new ApiNotFoundResponse($"Category with id: {id} is not found"));

            //// GIVE INFORMATIONS TO CategoryVm (JUST SHOW FIELD NEEDED
            CategoryVm categoryvm = CreateCategoryVm(category);
            return Ok(categoryvm);
        }

        /// <summary>
        /// CREATE NEW CATEGORY.
        /// </summary>
        /// <param name="request">INPUT DATA.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpPost]
        [ClaimRequirement(FunctionCode.CONTENT_CATEGORY, CommandCode.CREATE)]
        [ApiValidationFilter]
        public async Task<IActionResult> PostCategory([FromBody] CategoryCreateRequest request)
        {
            //// CREATE A CONSTANCE OF CATEGORY WITH INFORS ARE INPUT DATA
            var category = new Category()
            {
                Name = request.Name,
                ParentId = request.ParentId,
                SortOrder = request.SortOrder,
                SeoAlias = request.SeoAlias,
                SeoDescription = request.SeoDescription
            };

            //// INSERT NEW CATEGORY INTO DATABASE AND SAVE CHANGE
            _context.Categories.Add(category);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER INSERT IS GREATER THAN 0 (TRUE), RETURN STATUS 201, ELSE RETURN STATUS 400
            if (result > 0)
            {
                return CreatedAtAction(nameof(GetById), new { id = category.Id }, request);
            }
            else
            {
                return BadRequest(new ApiBadRequestResponse("Create category failed"));
            }
        }

        /// <summary>
        /// UPDATE INFORMATIONS OF CATEGORY WITH ID (KEY).
        /// </summary>
        /// <param name="id">KEY OF CATEGORY.</param>
        /// <param name="request">INPUT DATA.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpPut("{id}")]
        [ClaimRequirement(FunctionCode.CONTENT_CATEGORY, CommandCode.UPDATE)]
        [ApiValidationFilter]
        public async Task<IActionResult> PutCategory(int id, [FromBody] CategoryCreateRequest request)
        {
            //// GET CATEGORY WITH ID (KEY)
            var category = await _context.Categories.FindAsync(id);
            //// IF KEY NOT EXIST (CATEGORY IS NULL), RETURN STATUS 404
            if (category == null)
                return NotFound(new ApiNotFoundResponse($"Category with id: {id} is not found"));

            //// IF ID EQUAL PARENT_ID, RETURN STATUS 400 (THIS CATEGORY CANNOT BE A CHILD ITSELFT)
            if (id == request.ParentId)
            {
                return BadRequest(new ApiBadRequestResponse("Category cannot be a child itself."));
            }

            //// GIVE INPUT DATA FOR EACH FIELD OF OBJECT WHICH NEED UPDATE INFOMATIONS
            category.Name = request.Name;
            category.ParentId = request.ParentId;
            category.SortOrder = request.SortOrder;
            category.SeoDescription = request.SeoDescription;
            category.SeoAlias = request.SeoAlias;

            //// UPDATE CATEGORY AND SAVE CHANGE
            _context.Categories.Update(category);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER UPDATE IS GREATER THAN 0 (TRUE), RETURN STATUS 204, ELSE RETURN STATUS 400
            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse("Update category failed"));
        }

        /// <summary>
        /// DELETE CATEGORY WITH ID (KEY).
        /// </summary>
        /// <param name="id">KEY OF CATEGORY.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpDelete("{id}")]
        [ClaimRequirement(FunctionCode.CONTENT_CATEGORY, CommandCode.DELETE)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            //// GET CATEGORY WITH ID (KEY)
            var category = await _context.Categories.FindAsync(id);
            //// IF KEY NOT EXIST (CATEGORY IS NULL), RETURN STATUS 404
            if (category == null)
                return NotFound(new ApiNotFoundResponse($"Category with id: {id} is not found"));

            //// REMOVE CATEGORY FROM DATABASE AND SAVE CHANGE
            _context.Categories.Remove(category);
            var result = await _context.SaveChangesAsync();
            //// IF RESULT AFTER DELETE IS GREATER THAN 0 (TRUE), RETURN HTTP STATUS 200, ELSE RETURN STATUS 400
            if (result > 0)
            {
                CategoryVm categoryvm = CreateCategoryVm(category);
                return Ok(categoryvm);
            }
            return BadRequest(new ApiBadRequestResponse("Delete category failed"));
        }

        /// <summary>
        /// CREATE A CONSTANCE OF CATEGORY JUST SHOW NEEDED FIELD (CategoryVm).
        /// </summary>
        /// <param name="category">OBJECT.</param>
        /// <returns>CategoryVm.</returns>
        private static CategoryVm CreateCategoryVm(Category category)
        {
            return new CategoryVm()
            {
                Id = category.Id,
                Name = category.Name,
                SortOrder = category.SortOrder,
                ParentId = category.ParentId,
                NumberOfTickets = category.NumberOfTickets,
                SeoDescription = category.SeoDescription,
                SeoAlias = category.SeoDescription
            };
        }
    }
}
