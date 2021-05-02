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
    public class KnowledgeBasesController : BaseController
    {
        private readonly KnowledgeSpaceContext _context;

        /// <summary>
        /// CONSTRUCTOR CONTROLLER.
        /// </summary>
        /// <param name="context">DbContext.</param>
        public KnowledgeBasesController(KnowledgeSpaceContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: api/knowledgebases
        /// GET ALL KNOWLEDGEBASE.
        /// </summary>
        /// <returns>HTTP STATUS WITH LIST OF KNOWLEDGE BASE.</returns>
        [HttpGet]
        public async Task<IActionResult> GetKnowledgeBases()
        {
            //// GET ALL KNOWLEDGE BASE IN DATABASE
            var knowledgeBases = _context.KnowledgeBases;

            //// TAKE INFOMATIONS OF KNOWLEDGE BASE NEED SHOW AND RETURN HTTP STATUS 200
            var knowledgeBasevms = await knowledgeBases.Select(u => new KnowledgeBaseQuickVm()
            {
                Id = u.Id,
                CategoryId = (int)u.CategoryId,
                Description = u.Description,
                SeoAlias = u.SeoAlias,
                Title = u.Title
            }).ToListAsync();

            return Ok(knowledgeBasevms);
        }

        /// <summary>
        /// GET KNOWLEDGE BASE WITH KEYWORD AND PAGINATION.
        /// </summary>
        /// <param name="filter">KEYWORD SEARCH.</param>
        /// <param name="pageIndex">INDEX OF NEXT PAGE.</param>
        /// <param name="pageSize">NUMBER OF RECORDS EACH PAGE.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpGet("filter")]
        public async Task<IActionResult> GetKnowledgeBasesPaging(string filter, int pageIndex, int pageSize)
        {
            //// GET ALL KNOWLEDGE BASES
            var query = _context.KnowledgeBases.AsQueryable();

            //// IF KEYWORD NOT NULL, GET ALL KNOWLEDGE BASES WHICH CONSTAINS KEYWORD
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Title.Contains(filter));
            }

            //// TOTAL RECORDS EQUAL NUMBER OF KNOWLEDGEBASES's ROWS
            var totalRecords = await query.CountAsync();

            //// TAKE RECORDS IN THE PAGE (NEXT PAGE)
            var items = await query.Skip((pageIndex - 1 * pageSize))
                .Take(pageSize)
                .Select(u => new KnowledgeBaseQuickVm()
                {
                    Id = u.Id,
                    CategoryId = (int)u.CategoryId,
                    Description = u.Description,
                    SeoAlias = u.SeoAlias,
                    Title = u.Title
                })
                .ToListAsync();
            
            //// PAGINATION
            var pagination = new Pagination<KnowledgeBaseQuickVm>
            {
                Items = items,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }

        /// <summary>
        /// GET KNOWLEDGE BASE WITH ID (KEY)
        /// </summary>
        /// <param name="id">KEY OF KNOWLEDGE BASE.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            //// GET KNOWLEDGE BASE WITH ID (KEY)
            var knowledgeBase = await _context.KnowledgeBases.FindAsync(id);

            //// IF KEY IS NOT EXIST (RESULT IS NULL), RETURN STATUS 404
            if (knowledgeBase == null)
                return NotFound();

            //// GIVE INFO KnowledgeBaseVm (JUST SHOW NEEDED FIELDS)
            var knowledgeBaseVm = new KnowledgeBaseVm()
            {
                Id = knowledgeBase.Id,

                CategoryId = (int)knowledgeBase.CategoryId,

                Title = knowledgeBase.Title,

                SeoAlias = knowledgeBase.SeoAlias,

                Description = knowledgeBase.Description,

                Environment = knowledgeBase.Environment,

                Problem = knowledgeBase.Problem,

                StepToReproduce = knowledgeBase.StepToReproduce,

                ErrorMessage = knowledgeBase.ErrorMessage,

                Workaround = knowledgeBase.Workaround,

                Note = knowledgeBase.Note,

                OwnerUserId = knowledgeBase.OwnerUserId,

                Labels = knowledgeBase.Labels,

                CreateDate = knowledgeBase.CreateDate,

                LastModifiedDate = knowledgeBase.LastModifiedDate,

                NumberOfComments = knowledgeBase.CategoryId,

                NumberOfVotes = knowledgeBase.CategoryId,

                NumberOfReports = knowledgeBase.CategoryId,
            };
            return Ok(knowledgeBaseVm);
        }

        /// <summary>
        /// CREATE NEW KNOWLEDGE BASE
        /// </summary>
        /// <param name="request">INPUT DATA.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpPost]
        public async Task<IActionResult> PostKnowledgeBase([FromBody] KnowledgeBaseCreateRequest request)
        {
            //// CREATE NEW INSTANCE OF KNOWLEDGE BASE WITH INFOS ARE INPUT DATA
            var knowledgeBase = new KnowledgeBase()
            {
                CategoryId = request.CategoryId,

                Title = request.Title,

                SeoAlias = request.SeoAlias,

                Description = request.Description,

                Environment = request.Environment,

                Problem = request.Problem,

                StepToReproduce = request.StepToReproduce,

                ErrorMessage = request.ErrorMessage,

                Workaround = request.Workaround,

                Note = request.Note,

                Labels = request.Labels,
            };

            //// INSERT NEW KNOWLEDGE BASE INTO DATABASE AND SAVE CHANGE
            _context.KnowledgeBases.Add(knowledgeBase);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER INSERT IS GREATER THAN 0 (TRUE), RETURN STATUS 201, ELSE RETURN STATUS 400
            if (result > 0)
            {
                return CreatedAtAction(nameof(GetById), new { id = knowledgeBase.Id }, request);
            }
            else
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// UPDATE KNOWLEDGE BASE WITH ID (KEY).
        /// </summary>
        /// <param name="id">KEY OF KNOWLEDGE BASE.</param>
        /// <param name="request">INPUT DATA NEED CHANGE.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutKnowledgeBase(int id, [FromBody] KnowledgeBaseCreateRequest request)
        {
            //// GET KNOWLEDGE BASE WITH ID (KEY)
            var knowledgeBase = await _context.KnowledgeBases.FindAsync(id);

            //// IF KEY IS NOT EXIST (RESULT IS NULL), RETURN STATUS 404
            if (knowledgeBase == null)
                return NotFound();

            //// GIVE INPUT DATA FOR EACH FIELD OF OBJECT WHICH NEED UPDATE INFOMATIONS
            knowledgeBase.CategoryId = request.CategoryId;

            knowledgeBase.Title = request.Title;

            knowledgeBase.SeoAlias = request.SeoAlias;

            knowledgeBase.Description = request.Description;

            knowledgeBase.Environment = request.Environment;

            knowledgeBase.Problem = request.Problem;

            knowledgeBase.StepToReproduce = request.StepToReproduce;

            knowledgeBase.ErrorMessage = request.ErrorMessage;

            knowledgeBase.Workaround = request.Workaround;

            knowledgeBase.Note = request.Note;

            knowledgeBase.Labels = request.Labels;

            //// UPDATE KNOWLEDGE BASE AND SAVE CHANGE
            _context.KnowledgeBases.Update(knowledgeBase);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER UPDATE IS GREATER THAN 0 (TRUE), RETURN STATUS 204, ELSE RETURN STATUS 400
            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest();
        }

        /// <summary>
        /// DELETE KNOWLEDGE BASE WITH ID (KEY).
        /// </summary>
        /// <param name="id">KEY OF KNOWLEDGE BASE.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKnowledgeBase(string id)
        {
            //// GET KNOWLEDGE BASE WITH ID (KEY)
            var knowledgeBase = await _context.KnowledgeBases.FindAsync(id);

            //// IF KEY IS NOT EXIST (RESULT IS NULL), RETURN STATUS 404
            if (knowledgeBase == null)
                return NotFound();

            //// DELETE KNOWLEDGE BASE FROM DATABASE AND SAVE CHANGE
            _context.KnowledgeBases.Remove(knowledgeBase);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER DELETE IS GREATER THAN 0 (TRUE), RETURN STATUS 200, ELSE RETURN STATUS 400
            if (result > 0)
            {
                var knowledgeBasevm = new KnowledgeBaseVm()
                {
                    Id = knowledgeBase.Id,

                    CategoryId = (int)knowledgeBase.CategoryId,

                    Title = knowledgeBase.Title,

                    SeoAlias = knowledgeBase.SeoAlias,

                    Description = knowledgeBase.Description,

                    Environment = knowledgeBase.Environment,

                    Problem = knowledgeBase.Problem,

                    StepToReproduce = knowledgeBase.StepToReproduce,

                    ErrorMessage = knowledgeBase.ErrorMessage,

                    Workaround = knowledgeBase.Workaround,

                    Note = knowledgeBase.Note,

                    OwnerUserId = knowledgeBase.OwnerUserId,

                    Labels = knowledgeBase.Labels,

                    CreateDate = knowledgeBase.CreateDate,

                    LastModifiedDate = knowledgeBase.LastModifiedDate,

                    NumberOfComments = knowledgeBase.CategoryId,

                    NumberOfVotes = knowledgeBase.CategoryId,

                    NumberOfReports = knowledgeBase.CategoryId,
                };
                return Ok(knowledgeBasevm);
            }
            return BadRequest();
        }
    }
}
