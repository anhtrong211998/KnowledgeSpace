using KnowledgeSpace.BackendServer.Authorization;
using KnowledgeSpace.BackendServer.Constants;
using KnowledgeSpace.BackendServer.Extensions;
using KnowledgeSpace.BackendServer.Helpers;
using KnowledgeSpace.BackendServer.Models;
using KnowledgeSpace.BackendServer.Models.Entities;
using KnowledgeSpace.BackendServer.Services;
using KnowledgeSpace.ViewModels;
using KnowledgeSpace.ViewModels.Contents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace KnowledgeSpace.BackendServer.Controllers
{
    public partial class KnowledgeBasesController : BaseController
    {
        private readonly KnowledgeSpaceContext _context;
        private readonly ISequenceService _sequenceService;
        private readonly IStorageService _storageService;
        private readonly ICacheService _cacheService;
        /// <summary>
        /// CONSTRUCTOR CONTROLLER.
        /// </summary>
        /// <param name="context">DbContext.</param>
        public KnowledgeBasesController(KnowledgeSpaceContext context,
            ISequenceService sequenceService,
            IStorageService storageService,
            ICacheService cacheService)
        {
            _context = context;
            _sequenceService = sequenceService;
            _storageService = storageService;
            _cacheService = cacheService;
        }

        #region KNOWLEDGE BASE MANAGERMENT
        /// <summary>
        /// GET: api/knowledgebases
        /// GET ALL KNOWLEDGEBASE.
        /// </summary>
        /// <returns>HTTP STATUS WITH LIST OF KNOWLEDGE BASE.</returns>
        [HttpGet]
        [ClaimRequirement(FunctionCode.CONTENT_KNOWLEDGEBASE, CommandCode.VIEW)]
        public async Task<IActionResult> GetKnowledgeBases()
        {
            //// GET ALL KNOWLEDGE BASE IN DATABASE
            var knowledgeBases = _context.KnowledgeBases;

            //// TAKE INFOMATIONS OF KNOWLEDGE BASE NEED SHOW AND RETURN HTTP STATUS 200
            var knowledgeBasevms = await knowledgeBases.Select(u => new KnowledgeBaseQuickVm()
            {
                Id = u.Id,
                CategoryId = u.CategoryId.Value,
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
        [AllowAnonymous]
        public async Task<IActionResult> GetKnowledgeBasesPaging(string filter, int? categoryId, int pageIndex, int pageSize)
        {
            //// GET ALL KNOWLEDGE BASES OF CATEGORIES
            var query = from k in _context.KnowledgeBases
                        join c in _context.Categories on k.CategoryId equals c.Id
                        select new { k, c };

            //// IF KEYWORD NOT NULL, GET ALL KNOWLEDGE BASES WHICH CONSTAINS KEYWORD
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.k.Title.Contains(filter));
            }
            //// IF KEYWORD NOT NULL, GET ALL KNOWLEDGE BASES WHICH CONSTAINS CategoryId
            if (categoryId.HasValue)
            {
                query = query.Where(x => x.k.CategoryId == categoryId.Value);
            }

            //// TOTAL RECORDS EQUAL NUMBER OF KNOWLEDGEBASES's ROWS
            var totalRecords = await query.CountAsync();

            //// TAKE RECORDS IN THE PAGE (NEXT PAGE)
            var items = await query.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new KnowledgeBaseQuickVm()
                {
                    Id = u.k.Id,
                    CategoryId = u.k.CategoryId.Value,
                    Description = u.k.Description,
                    SeoAlias = u.k.SeoAlias,
                    Title = u.k.Title,
                    CategoryAlias = u.c.SeoAlias,
                    CategoryName = u.c.Name,
                    NumberOfVotes = u.k.NumberOfVotes,
                    CreateDate = u.k.CreateDate,
                    NumberOfComments = u.k.NumberOfComments
                })
                .ToListAsync();

            //// PAGINATION
            var pagination = new Pagination<KnowledgeBaseQuickVm>
            {
                PageSize = pageSize,
                PageIndex = pageIndex,
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
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            //// GET KNOWLEDGE BASE WITH ID (KEY)
            var knowledgeBase = await _context.KnowledgeBases.FindAsync(id);

            //// IF KEY IS NOT EXIST (RESULT IS NULL), RETURN STATUS 404
            if (knowledgeBase == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found knowledge base with id: {id}"));

            //// GIVE INFO KnowledgeBaseVm (JUST SHOW NEEDED FIELDS)
            var attachments = await _context.Attachments
                .Where(x => x.KnowledgeBaseId == id)
                .Select(x => new AttachmentVm()
                {
                    FileName = x.FileName,
                    FilePath = x.FilePath,
                    FileSize = x.FileSize,
                    Id = x.Id,
                    FileType = x.FileType
                }).ToListAsync();
            var knowledgeBaseVm = CreateKnowledgeBaseVm(knowledgeBase);
            knowledgeBaseVm.Attachments = attachments;
            return Ok(knowledgeBaseVm);
        }

        /// <summary>
        /// GET LASTEST KNOWLEDGE BASE.
        /// </summary>
        /// <param name="take">NUMBER OF RECORDS NEED SHOW</param>
        /// <returns>HTTP STATUS</returns>
        [HttpGet("latest/{take:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLatestKnowledgeBases(int take)
        {
            var cachedData = await _cacheService.GetAsync<List<KnowledgeBaseQuickVm>>(CacheConstants.LatestKnowledgeBases);
            if (cachedData == null)
            {
                //// GET NUMBER OF RECORDS NEED SHOW ORDER BY CREATEDATE DESC
                var knowledgeBases = from k in _context.KnowledgeBases
                                     join c in _context.Categories on k.CategoryId equals c.Id
                                     orderby k.CreateDate descending
                                     select new { k, c };

                //// GET INFOMATION OF FIELDS NEEDED SHOW
                var knowledgeBaseVms = await knowledgeBases.Take(take)
                    .Select(u => new KnowledgeBaseQuickVm()
                    {
                        Id = u.k.Id,
                        CategoryId = u.k.CategoryId.Value,
                        Description = u.k.Description,
                        SeoAlias = u.k.SeoAlias,
                        Title = u.k.Title,
                        CategoryAlias = u.c.SeoAlias,
                        CategoryName = u.c.Name,
                        ViewCount = u.k.ViewCount,
                        NumberOfVotes = u.k.NumberOfVotes,
                        CreateDate = u.k.CreateDate
                    }).ToListAsync();
                await _cacheService.SetAsync(CacheConstants.LatestKnowledgeBases, knowledgeBaseVms, 2);
                cachedData = knowledgeBaseVms;
            }
                

            return Ok(cachedData);
        }

        /// <summary>
        /// GET POPULAR KNOWLEDGE BASE.
        /// </summary>
        /// <param name="take">NUMBER OF RECORDS NEED SHOW.</param>
        /// <returns>HTTP STATUS</returns>
        [HttpGet("popular/{take:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPopularKnowledgeBases(int take)
        {
            var cachedData = await _cacheService.GetAsync<List<KnowledgeBaseQuickVm>>(CacheConstants.PopularKnowledgeBases);
            if (cachedData == null)
            {
                //// GET NUMBER OF RECORDS NEED SHOW ORDER BY VIEWCOUNT DESC
                var knowledgeBases = from k in _context.KnowledgeBases
                                     join c in _context.Categories on k.CategoryId equals c.Id
                                     orderby k.ViewCount descending
                                     select new { k, c };

                //// GET INFOMATION OF FIELDS NEEDED SHOW
                var knowledgeBaseVms = await knowledgeBases.Take(take)
                    .Select(u => new KnowledgeBaseQuickVm()
                    {
                        Id = u.k.Id,
                        CategoryId = u.k.CategoryId.Value,
                        Description = u.k.Description,
                        SeoAlias = u.k.SeoAlias,
                        Title = u.k.Title,
                        CategoryAlias = u.c.SeoAlias,
                        CategoryName = u.c.Name,
                        ViewCount = u.k.ViewCount,
                        NumberOfVotes = u.k.NumberOfVotes,
                        CreateDate = u.k.CreateDate
                    }).ToListAsync();
                await _cacheService.SetAsync(CacheConstants.PopularKnowledgeBases, knowledgeBaseVms, 24);
                cachedData = knowledgeBaseVms;
            }
                

            return Ok(cachedData);
        }

        /// <summary>
        /// CREATE NEW KNOWLEDGE BASE
        /// </summary>
        /// <param name="request">INPUT DATA.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpPost]
        [ClaimRequirement(FunctionCode.CONTENT_KNOWLEDGEBASE, CommandCode.CREATE)]
        [ApiValidationFilter]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PostKnowledgeBase([FromForm] KnowledgeBaseCreateRequest request)
        {
            //// CREATE NEW INSTANCE OF KNOWLEDGE BASE WITH INFOS ARE INPUT DATA
            KnowledgeBase knowledgeBase = CreateKnowledgeBaseEntity(request);
            knowledgeBase.Id = await _sequenceService.GetKnowledgeBaseNewId();

            //// GET CURRENT ID (IS OWNER USER)
            knowledgeBase.OwnerUserId = User.GetUserId();
            //// CONVERT SIGN STRING TO UNSIGN STRING
            if (string.IsNullOrEmpty(knowledgeBase.SeoAlias))
            {
                knowledgeBase.SeoAlias = TextHelper.ToUnsignString(knowledgeBase.Title);
            }

            //// PROCESS ATTACHMENT
            if (request.Attachments != null && request.Attachments.Count > 0)
            {
                foreach (var attachment in request.Attachments)
                {
                    var attachmentEntity = await SaveFile(knowledgeBase.Id, attachment);
                    _context.Attachments.Add(attachmentEntity);
                }
            }

            _context.KnowledgeBases.Add(knowledgeBase);

            //// PROCESS LABEL
            if (request.Labels?.Length > 0)
            {
                await ProcessLabel(request, knowledgeBase);
            }

            //// INSERT NEW KNOWLEDGE BASE INTO DATABASE AND SAVE CHANGE
            _context.KnowledgeBases.Add(knowledgeBase);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER INSERT IS GREATER THAN 0 (TRUE), RETURN STATUS 201, ELSE RETURN STATUS 400
            if (result > 0)
            {
                await _cacheService.RemoveAsync(CacheConstants.LatestKnowledgeBases);
                await _cacheService.RemoveAsync(CacheConstants.PopularKnowledgeBases);

                return CreatedAtAction(nameof(GetById), new { id = knowledgeBase.Id });
            }
            else
            {
                return BadRequest(new ApiBadRequestResponse("Create knowledge base failed"));
            }
        }

        /// <summary>
        /// UPDATE KNOWLEDGE BASE WITH ID (KEY).
        /// </summary>
        /// <param name="id">KEY OF KNOWLEDGE BASE.</param>
        /// <param name="request">INPUT DATA NEED CHANGE.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpPut("{id}")]
        [ClaimRequirement(FunctionCode.CONTENT_KNOWLEDGEBASE, CommandCode.UPDATE)]
        [ApiValidationFilter]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PutKnowledgeBase(int id, [FromForm] KnowledgeBaseCreateRequest request)
        {
            //// GET KNOWLEDGE BASE WITH ID (KEY)
            var knowledgeBase = await _context.KnowledgeBases.FindAsync(id);

            //// IF KEY IS NOT EXIST (RESULT IS NULL), RETURN STATUS 404
            if (knowledgeBase == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found knowledge base with id {id}"));

            //// GIVE INPUT DATA FOR EACH FIELD OF OBJECT WHICH NEED UPDATE INFOMATIONS
            UpdateKnowledgeBase(request, knowledgeBase);

            //// PROCESS ATTACHMENT
            if (request.Attachments != null && request.Attachments.Count > 0)
            {
                foreach (var attachment in request.Attachments)
                {
                    var attachmentEntity = await SaveFile(knowledgeBase.Id, attachment);
                    _context.Attachments.Add(attachmentEntity);
                }
            }
            //// PROCESS LABEL
            if (request.Labels?.Length > 0)
            {
                await ProcessLabel(request, knowledgeBase);
            }

            //// UPDATE KNOWLEDGE BASE AND SAVE CHANGE
            _context.KnowledgeBases.Update(knowledgeBase);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER UPDATE IS GREATER THAN 0 (TRUE), RETURN STATUS 204, ELSE RETURN STATUS 400
            if (result > 0)
            {
                await _cacheService.RemoveAsync(CacheConstants.LatestKnowledgeBases);
                await _cacheService.RemoveAsync(CacheConstants.PopularKnowledgeBases);
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse($"Update knowledge base failed"));
        }

        /// <summary>
        /// DELETE KNOWLEDGE BASE WITH ID (KEY).
        /// </summary>
        /// <param name="id">KEY OF KNOWLEDGE BASE.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpDelete("{id}")]
        [ClaimRequirement(FunctionCode.CONTENT_KNOWLEDGEBASE, CommandCode.DELETE)]
        public async Task<IActionResult> DeleteKnowledgeBase(int id)
        {
            //// GET KNOWLEDGE BASE WITH ID (KEY)
            var knowledgeBase = await _context.KnowledgeBases.FindAsync(id);

            //// IF KEY IS NOT EXIST (RESULT IS NULL), RETURN STATUS 404
            if (knowledgeBase == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found knowledge base with id {id}"));

            //// DELETE KNOWLEDGE BASE FROM DATABASE AND SAVE CHANGE
            _context.KnowledgeBases.Remove(knowledgeBase);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER DELETE IS GREATER THAN 0 (TRUE), RETURN STATUS 200, ELSE RETURN STATUS 400
            if (result > 0)
            {
                await _cacheService.RemoveAsync(CacheConstants.LatestKnowledgeBases);
                await _cacheService.RemoveAsync(CacheConstants.PopularKnowledgeBases);
                KnowledgeBaseVm knowledgeBasevm = CreateKnowledgeBaseVm(knowledgeBase);
                return Ok(knowledgeBasevm);
            }
            return BadRequest(new ApiBadRequestResponse($"Delete knowledge base failed"));
        }

        /// <summary>
        /// UPDATE VIEW COUNT WHEN VIEW DETAIL
        /// </summary>
        /// <param name="id">KEY OF KNOWLEDGEBASE</param>
        /// <returns>HTTP STATUS</returns>
        [HttpPut("{id}/view-count")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateViewCount(int id)
        {
            var knowledgeBase = await _context.KnowledgeBases.FindAsync(id);
            if (knowledgeBase == null)
            {
                return NotFound();
            }

            knowledgeBase.ViewCount += 1;
            _context.KnowledgeBases.Update(knowledgeBase);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok();
            }
            return BadRequest();
        }
        #endregion

        #region PRIVATE METHOD
        /// <summary>
        /// SAVE FILE (ATTACHMENT) PROCESS 
        /// </summary>
        /// <param name="knowledegeBaseId">KEY OF KNOWLEDGE BASE</param>
        /// <param name="file">IFormFile</param>
        /// <returns></returns>
        private async Task<Attachment> SaveFile(int knowledegeBaseId, IFormFile file)
        {
            //// GET RAW NAME OF FILE
            var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            //// GENERATION NAME OF FILE
            var fileName = $"{originalFileName.Substring(0, originalFileName.LastIndexOf('.'))}{Path.GetExtension(originalFileName)}";
            //// SAVE FILE
            await _storageService.SaveFileAsync(file.OpenReadStream(), fileName);
            var attachmentEntity = new Attachment()
            {
                FileName = fileName,
                FilePath = _storageService.GetFileUrl(fileName),
                FileSize = file.Length,
                FileType = Path.GetExtension(fileName),
                KnowledgeBaseId = knowledegeBaseId,
            };
            return attachmentEntity;
        }

        /// <summary>
        /// CREATE A CONSTANCE OF KNOWLEDGE BASE JUST SHOW NEEDED FIELDS (KnowledgeBaseVm).
        /// </summary>
        /// <param name="knowledgeBase">OBJECT.</param>
        /// <returns>KnowledgeBaseVm.</returns>
        private static KnowledgeBaseVm CreateKnowledgeBaseVm(KnowledgeBase knowledgeBase)
        {
            return new KnowledgeBaseVm()
            {
                Id = knowledgeBase.Id,

                CategoryId = knowledgeBase.CategoryId.Value,

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

                Labels = !string.IsNullOrEmpty(knowledgeBase.Labels) ? knowledgeBase.Labels.Split(',') : null,

                CreateDate = knowledgeBase.CreateDate,

                LastModifiedDate = knowledgeBase.LastModifiedDate,

                NumberOfComments = knowledgeBase.NumberOfComments,

                NumberOfVotes = knowledgeBase.NumberOfVotes,

                NumberOfReports = knowledgeBase.NumberOfReports
            };
        }

        /// <summary>
        /// ADD LABEL FOR KNOWLEDGE BASE.
        /// </summary>
        /// <param name="request">INPUT DATA OF KNOWLEDGE BASE.</param>
        /// <param name="knowledgeBase"></param>
        /// <returns></returns>
        private async Task ProcessLabel(KnowledgeBaseCreateRequest request, KnowledgeBase knowledgeBase)
        {
            //// SPLIT STRING LABEL OF KNOWLEDGE BASE TO A ARRAY
            foreach (var labelText in request.Labels)
            {
                //// IF LABEL NULL IGNORE THIS LOOP 
                if (labelText == null) continue;

                //// CONVERT SEALED CHARACTERS TO UNSIGN STRING, AND IT IS ID OF LABEL
                var labelId = TextHelper.ToUnsignString(labelText.ToString());

                //// GET LABEL WITH ID, IF KEY NOT EXIST, CREATE NEW LABEL 
                var existingLabel = await _context.Labels.FindAsync(labelId);
                if (existingLabel == null)
                {
                    var labelEntity = new Label()
                    {
                        Id = labelId,
                        Name = labelText.ToString()
                    };
                    _context.Labels.Add(labelEntity);
                }

                //// ADD NEW LABEL FOR KNOWLEDGE BASE
                if (await _context.LabelInKnowledgeBases.FindAsync(labelId, knowledgeBase.Id) == null)
                {
                    _context.LabelInKnowledgeBases.Add(new LabelInKnowledgeBase()
                    {
                        KnowledgeBaseId = knowledgeBase.Id,
                        LabelId = labelId
                    });
                }
            }
        }

        /// <summary>
        /// CREATE A CONSTANCE OF KNOWLEDGE BASE
        /// </summary>
        /// <param name="request">KnowledgeBaseCreateRequest</param>
        /// <returns>KnowledgeBase</returns>
        private static KnowledgeBase CreateKnowledgeBaseEntity(KnowledgeBaseCreateRequest request)
        {
            var entity = new KnowledgeBase()
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

                Note = request.Note
            };
            if (request.Labels?.Length > 0)
            {
                entity.Labels = string.Join(',', request.Labels);
            }
            return entity;
        }

        /// <summary>
        /// MAPPING FIELDS DATA OF KNOWLEDGE BASE
        /// </summary>
        /// <param name="request">KnowledgeBaseCreateRequest</param>
        /// <param name="knowledgeBase">KnowledgeBase</param>
        private static void UpdateKnowledgeBase(KnowledgeBaseCreateRequest request, KnowledgeBase knowledgeBase)
        {
            knowledgeBase.CategoryId = request.CategoryId;

            knowledgeBase.Title = request.Title;

            if (string.IsNullOrEmpty(request.SeoAlias))
            {
                knowledgeBase.SeoAlias = TextHelper.ToUnsignString(request.Title);
            }
            else
            {
                knowledgeBase.SeoAlias = request.SeoAlias;
            }

            knowledgeBase.Description = request.Description;

            knowledgeBase.Environment = request.Environment;

            knowledgeBase.Problem = request.Problem;

            knowledgeBase.StepToReproduce = request.StepToReproduce;

            knowledgeBase.ErrorMessage = request.ErrorMessage;

            knowledgeBase.Workaround = request.Workaround;

            knowledgeBase.Note = request.Note;

            if (request.Labels != null)
            {
                knowledgeBase.Labels = string.Join(',', request.Labels);
            }
        }
        #endregion

        #region Management Labels
        /// <summary>
        /// GET LABELS OF KNOWLEDGEBASE
        /// </summary>
        /// <param name="knowlegeBaseId">KEY OF KNOWLEDGE BASE</param>
        /// <returns>HTTP STATUS</returns>
        [HttpGet("{knowlegeBaseId}/labels")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLabelsByKnowledgeBaseId(int knowlegeBaseId)
        {
            //// GET ALL LABEL OF KNOWLEDGE BASE
            var query = from lik in _context.LabelInKnowledgeBases
                        join l in _context.Labels on lik.LabelId equals l.Id
                        orderby l.Name ascending
                        where lik.KnowledgeBaseId == knowlegeBaseId
                        select new { l.Id, l.Name };

            //// JUST SHOW NEEDED FIELD
            var labels = await query.Select(u => new LabelVm()
            {
                Id = u.Id,
                Name = u.Name
            }).ToListAsync();

            return Ok(labels);
        }

        /// <summary>
        /// GET KNOWLEDGEBASEs OF LABELS
        /// </summary>
        /// <param name="labelId">KEY OF LABEL</param>
        /// <param name="pageIndex">INDEX OF NEXT PAGE</param>
        /// <param name="pageSize">NUMBER RECORDS PER PAGE</param>
        /// <returns>HTTP STATUS</returns>
        [HttpGet("tags/{labelId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetKnowledgeBasesByTagId(string labelId, int pageIndex, int pageSize)
        {
            var query = from k in _context.KnowledgeBases
                        join lik in _context.LabelInKnowledgeBases on k.Id equals lik.KnowledgeBaseId
                        join l in _context.Labels on lik.LabelId equals l.Id
                        join c in _context.Categories on k.CategoryId equals c.Id
                        where lik.LabelId == labelId
                        select new { k, l, c };

            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new KnowledgeBaseQuickVm()
                {
                    Id = u.k.Id,
                    CategoryId = u.k.CategoryId.Value,
                    Description = u.k.Description,
                    SeoAlias = u.k.SeoAlias,
                    Title = u.k.Title,
                    CategoryAlias = u.c.SeoAlias,
                    CategoryName = u.c.Name,
                    NumberOfVotes = u.k.NumberOfVotes,
                    CreateDate = u.k.CreateDate,
                    NumberOfComments = u.k.NumberOfComments
                })
                .ToListAsync();

            var pagination = new Pagination<KnowledgeBaseQuickVm>
            {
                PageSize = pageSize,
                PageIndex = pageIndex,
                Items = items,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }
        #endregion
    }
}
