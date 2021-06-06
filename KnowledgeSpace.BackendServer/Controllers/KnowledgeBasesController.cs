﻿using KnowledgeSpace.BackendServer.Authorization;
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

        /// <summary>
        /// CONSTRUCTOR CONTROLLER.
        /// </summary>
        /// <param name="context">DbContext.</param>
        public KnowledgeBasesController(KnowledgeSpaceContext context,
            ISequenceService sequenceService,
            IStorageService storageService)
        {
            _context = context;
            _sequenceService = sequenceService;
            _storageService = storageService;
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
        [ClaimRequirement(FunctionCode.CONTENT_KNOWLEDGEBASE, CommandCode.VIEW)]
        public async Task<IActionResult> GetKnowledgeBasesPaging(string filter, int pageIndex, int pageSize)
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

            //// TOTAL RECORDS EQUAL NUMBER OF KNOWLEDGEBASES's ROWS
            var totalRecords = await query.CountAsync();

            //// TAKE RECORDS IN THE PAGE (NEXT PAGE)
            var items = await query.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new KnowledgeBaseQuickVm()
                {
                    Id = u.k.Id,
                    CategoryId = (int)u.k.CategoryId,
                    Description = u.k.Description,
                    SeoAlias = u.k.SeoAlias,
                    Title = u.k.Title,
                    CategoryName = u.c.Name
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
        [ClaimRequirement(FunctionCode.CONTENT_KNOWLEDGEBASE, CommandCode.VIEW)]
        public async Task<IActionResult> GetById(int id)
        {
            //// GET KNOWLEDGE BASE WITH ID (KEY)
            var knowledgeBase = await _context.KnowledgeBases.FindAsync(id);

            //// IF KEY IS NOT EXIST (RESULT IS NULL), RETURN STATUS 404
            if (knowledgeBase == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found knowledge base with id: {id}"));

            //// VIEW COUNT INCREASE 1
            knowledgeBase.ViewCount += 1;
            _context.KnowledgeBases.Update(knowledgeBase);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER UPDATE IS GREATER THAN 0 (TRUE), RETURN STATUS 200, ELSE RETURN STATUS 400
            if (result > 0)
            {
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
            return BadRequest(new ApiBadRequestResponse($"Increase ViewCount failed"));
            
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
            //// GET NUMBER OF RECORDS NEED SHOW ORDER BY CREATEDATE DESC
            var knowledgeBases = from k in _context.KnowledgeBases
                                 join c in _context.Categories on k.CategoryId equals c.Id
                                 orderby k.CreateDate descending
                                 select new { k, c };

            //// GET INFOMATION OF FIELDS NEEDED SHOW
            var knowledgeBasevms = await knowledgeBases.Take(take)
                .Select(u => new KnowledgeBaseQuickVm()
                {
                    Id = u.k.Id,
                    CategoryId = (int)u.k.CategoryId,
                    Description = u.k.Description,
                    SeoAlias = u.k.SeoAlias,
                    Title = u.k.Title,
                    CategoryAlias = u.c.SeoAlias,
                    CategoryName = u.c.Name,
                    ViewCount = u.k.ViewCount,
                    NumberOfVotes = u.k.NumberOfVotes,
                    CreateDate = u.k.CreateDate
                }).ToListAsync();

            return Ok(knowledgeBasevms);
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
            //// GET NUMBER OF RECORDS NEED SHOW ORDER BY VIEWCOUNT DESC
            var knowledgeBases = from k in _context.KnowledgeBases
                                 join c in _context.Categories on k.CategoryId equals c.Id
                                 orderby k.ViewCount descending
                                 select new { k, c };

            //// GET INFOMATION OF FIELDS NEEDED SHOW
            var knowledgeBasevms = await knowledgeBases.Take(take)
                .Select(u => new KnowledgeBaseQuickVm()
                {
                    Id = u.k.Id,
                    CategoryId = (int)u.k.CategoryId,
                    Description = u.k.Description,
                    SeoAlias = u.k.SeoAlias,
                    Title = u.k.Title,
                    CategoryAlias = u.c.SeoAlias,
                    CategoryName = u.c.Name,
                    ViewCount = u.k.ViewCount,
                    NumberOfVotes = u.k.NumberOfVotes,
                    CreateDate = u.k.CreateDate
                }).ToListAsync();

            return Ok(knowledgeBasevms);
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
                KnowledgeBaseVm knowledgeBasevm = CreateKnowledgeBaseVm(knowledgeBase);
                return Ok(knowledgeBasevm);
            }
            return BadRequest(new ApiBadRequestResponse($"Delete knowledge base failed"));
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
                if (!string.IsNullOrEmpty(labelText))
                {
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

            knowledgeBase.SeoAlias = request.SeoAlias;

            knowledgeBase.Description = request.Description;

            knowledgeBase.Environment = request.Environment;

            knowledgeBase.Problem = request.Problem;

            knowledgeBase.StepToReproduce = request.StepToReproduce;

            knowledgeBase.ErrorMessage = request.ErrorMessage;

            knowledgeBase.Workaround = request.Workaround;

            knowledgeBase.Note = request.Note;

            knowledgeBase.Labels = string.Join(',', request.Labels);
        }
        #endregion
    }
}
