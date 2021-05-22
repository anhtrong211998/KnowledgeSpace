using KnowledgeSpace.BackendServer.Helpers;
using KnowledgeSpace.BackendServer.Models;
using KnowledgeSpace.BackendServer.Models.Entities;
using KnowledgeSpace.BackendServer.Services;
using KnowledgeSpace.ViewModels;
using KnowledgeSpace.ViewModels.Contents;
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
    public class KnowledgeBasesController : BaseController
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
            var knowledgeBaseVm = CreateKnowledgeBaseVm(knowledgeBase);
            return Ok(knowledgeBaseVm);
        }

        /// <summary>
        /// CREATE NEW KNOWLEDGE BASE
        /// </summary>
        /// <param name="request">INPUT DATA.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpPost]
        public async Task<IActionResult> PostKnowledgeBase([FromForm] KnowledgeBaseCreateRequest request)
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

            knowledgeBase.Id = await _sequenceService.GetKnowledgeBaseNewId();

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
            if (!string.IsNullOrEmpty(request.Labels))
            {
                await ProcessLabel(request, knowledgeBase);
            }

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

            //// PROCESS LABEL
            if (!string.IsNullOrEmpty(request.Labels))
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
                KnowledgeBaseVm knowledgeBasevm = CreateKnowledgeBaseVm(knowledgeBase);
                return Ok(knowledgeBasevm);
            }
            return BadRequest();
        }

        private async Task<Attachment> SaveFile(int knowledegeBaseId, IFormFile file)
        {
            //// GET RAW NAME OF FILE
            var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            //// GENERATION NAME OF FILE
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
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

                Labels = knowledgeBase.Labels,

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
            string[] labels = request.Labels.Split(',');
            foreach (var labelText in labels)
            {
                //// CONVERT SEALED CHARACTERS TO UNSIGN STRING, AND IT IS ID OF LABEL
                var labelId = TextHelper.ToUnsignString(labelText);

                //// GET LABEL WITH ID, IF KEY NOT EXIST, CREATE NEW LABEL 
                var existingLabel = await _context.Labels.FindAsync(labelId);
                if (existingLabel == null)
                {
                    var labelEntity = new Label()
                    {
                        Id = labelId,
                        Name = labelText
                    };
                    _context.Labels.Add(labelEntity);
                }

                //// ADD LABEL FOR KNOWLEDGE BASE
                var labelInKnowledgeBase = new LabelInKnowledgeBase()
                {
                    KnowledgeBaseId = knowledgeBase.Id,
                    LabelId = labelId
                };
                _context.LabelInKnowledgeBases.Add(labelInKnowledgeBase);
            }
        }

        #endregion

        #region COMMENT MANAGERMENT
        /// <summary>
        /// GET COMMENTS OF KNOWLEDGE BASE WITH FILTER (KEYWORD SEARCH).
        /// </summary>
        /// <param name="knowledgeBaseId">KEY OF KNOWLEDGE BASE.</param>
        /// <param name="filter">KEYWORD SEARCH.</param>
        /// <param name="pageIndex">INDEX OF NEXT PAGE.</param>
        /// <param name="pageSize">NUMBER OF RECORDS IN EACH PAGE.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpGet("{knowledgeBaseId}/comments/filter")]
        public async Task<IActionResult> GetCommentsPaging(int knowledgeBaseId, string filter, int pageIndex, int pageSize)
        {
            //// GET ALL COMMENTS OF KNOWLEDGE BASE
            var query = _context.Comments.Where(x => x.KnowledgeBaseId == knowledgeBaseId).AsQueryable();
            //// IF KEYWORD NOT NULL OR EMPTY, GET ALL COMMENTS WHICH CONSTAINS KEYWORD
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Content.Contains(filter));
            }

            //// TOTAL RECORDS EQUAL NUMBER OF COMMENTS's ROWS
            var totalRecords = await query.CountAsync();

            //// TAKE RECORDS IN THE PAGE (NEXT PAGE) AND GIVE INFORMATIONS TO CommentVm (JUST SHOW FIELD NEEDED)
            var items = await query.Skip((pageIndex - 1 * pageSize))
                .Take(pageSize)
                .Select(c => new CommentVm()
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreateDate = c.CreateDate,
                    KnowledgeBaseId = c.KnowledgeBaseId,
                    LastModifiedDate = c.LastModifiedDate,
                    OwnwerUserId = c.OwnwerUserId
                })
                .ToListAsync();
            //// PAGINATION
            var pagination = new Pagination<CommentVm>
            {
                Items = items,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }

        /// <summary>
        /// GET COMMENT DETAIL.
        /// </summary>
        /// <param name="commentId">KEY OF COMMENT.</param>
        /// <returns>HTTP STATUS WITH COMMENT's INFORMATION.</returns>
        [HttpGet("{knowledgeBaseId}/comments/{commentId}")]
        public async Task<IActionResult> GetCommentDetail(int commentId)
        {
            //// GET COMMENT WITH ID (KEY)
            var comment = await _context.Comments.FindAsync(commentId);
            //// IF KEY NOT EXIST (COMMENT IS NULL), RETURN STATUS 404
            if (comment == null)
                return NotFound();

            //// CREATE A CONSTANCE OF COMMENT JUST SHOW NEEDED FIELD
            var commentVm = new CommentVm()
            {
                Id = comment.Id,
                Content = comment.Content,
                CreateDate = comment.CreateDate,
                KnowledgeBaseId = comment.KnowledgeBaseId,
                LastModifiedDate = comment.LastModifiedDate,
                OwnwerUserId = comment.OwnwerUserId
            };

            return Ok(commentVm);
        }

        /// <summary>
        /// CREATE NEW COMMENT.
        /// </summary>
        /// <param name="knowledgeBaseId">KEY OF KNOWLEDGE BASE.</param>
        /// <param name="request">INPUT DATA.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpPost("{knowledgeBaseId}/comments")]
        public async Task<IActionResult> PostComment(int knowledgeBaseId, [FromBody] CommentCreateRequest request)
        {
            //// CREATE NEW CONSTANCE OF COMMENT WITH INFORMATION ARE INPUT DATA
            var comment = new Comment()
            {
                Content = request.Content,
                KnowledgeBaseId = request.KnowledgeBaseId,
                OwnwerUserId = string.Empty/*TODO: GET USER FROM CLAIM*/,
            };

            //// INSERT NEW COMMENT
            _context.Comments.Add(comment);
            //// GET KNOWLEDGE BASE WITH ID (KEY), IF KEY NOT EXIST RETURN STATUS 404
            var knowledgeBase = await _context.KnowledgeBases.FindAsync(knowledgeBaseId);
            if (knowledgeBase == null)
                return BadRequest();

            //// UPDATE NUMBER OF COMMENTS INCREASE 1 AND SAVE CHANGE
            knowledgeBase.NumberOfComments = knowledgeBase.NumberOfVotes.GetValueOrDefault(0) + 1;
            _context.KnowledgeBases.Update(knowledgeBase);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER INSERT IS GREATER THAN 0 (TRUE), RETURN STATUS 201, ELSE RETURN STATUS 400
            if (result > 0)
            {
                return CreatedAtAction(nameof(GetCommentDetail), new { id = knowledgeBaseId, commentId = comment.Id }, request);
            }
            else
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// UPDATE COMMENT WITH ID (KEY).
        /// </summary>
        /// <param name="commentId">KEY OF COMMENT.</param>
        /// <param name="request">INPUT DATA.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpPut("{knowledgeBaseId}/comments/{commentId}")]
        public async Task<IActionResult> PutComment(int commentId, [FromBody] CommentCreateRequest request)
        {
            //// GET COMMENT WITH ID (KEY)
            var comment = await _context.Comments.FindAsync(commentId);
            //// IF KEY IS NOT EXIST (COMMENT IS NULL), RETURN STATUS 404
            if (comment == null)
                return NotFound();
            //// IF OWNWER USER DIFFERENT CURRENT USER, RETURN STATUS 403
            if (comment.OwnwerUserId != User.Identity.Name)
                return Forbid();

            //// UPDATE INFORMATION AND SAVE CHANGE
            comment.Content = request.Content;
            _context.Comments.Update(comment);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER UPDATE IS GREATER THAN 0 (TRUE), RETURN STATUS 204, ELSE RETURN STATUS 400
            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest();
        }

        /// <summary>
        /// DELETE COMMENT WITH ID (KEY).
        /// </summary>
        /// <param name="knowledgeBaseId">KEY OF KNOWLEDGE BASE.</param>
        /// <param name="commentId">KEY OF COMMENT.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpDelete("{knowledgeBaseId}/comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(int knowledgeBaseId, int commentId)
        {
            //// GET COMMENT WITH ID (KEY), IF KEY IS NOT EXIST, RETURN STATUS 404
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
                return NotFound();

            //// REMOVE COMMENT
            _context.Comments.Remove(comment);

            //// GET KNOWLEDGE BASE WITH ID, IF NULL, RETURN STATUS 400
            var knowledgeBase = await _context.KnowledgeBases.FindAsync(knowledgeBaseId);
            if (knowledgeBase == null)
                return BadRequest();

            //// UPDATE NUMBER OF COMMENT IS DECREASE 1 AND SAVE CHANGE
            knowledgeBase.NumberOfComments = knowledgeBase.NumberOfVotes.GetValueOrDefault(0) - 1;
            _context.KnowledgeBases.Update(knowledgeBase);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER DELETE IS GREATER THAN 0 (TRUE), RETURN STATUS 200, ELSE RETURN STATUS 400
            if (result > 0)
            {
                var commentVm = new CommentVm()
                {
                    Id = comment.Id,
                    Content = comment.Content,
                    CreateDate = comment.CreateDate,
                    KnowledgeBaseId = comment.KnowledgeBaseId,
                    LastModifiedDate = comment.LastModifiedDate,
                    OwnwerUserId = comment.OwnwerUserId
                };
                return Ok(commentVm);
            }
            return BadRequest();
        }
        #endregion

        #region VOTES MANAGERMENT
        /// <summary>
        /// GET ALL VOTES OF KNOWLEDGE BASE.
        /// </summary>
        /// <param name="knowledgeBaseId">KEY OF KNOWLEDGE BASE.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpGet("{knowledgeBaseId}/votes")]
        public async Task<IActionResult> GetVotes(int knowledgeBaseId)
        {
            //// GET ALL VOTE WITH CONDITION KNOWLEDGEBASE_ID OF VOTE EQUAL ID OF KNOWLEDGE BASE
            var votes = await _context.Votes
                .Where(x => x.KnowledgeBaseId == knowledgeBaseId)
                .Select(x => new VoteVm()
                {
                    UserId = x.UserId,
                    KnowledgeBaseId = x.KnowledgeBaseId,
                    CreateDate = x.CreateDate
                }).ToListAsync();
            return Ok(votes);
        }

        /// <summary>
        /// CREATE NEW VOTE.
        /// </summary>
        /// <param name="knowledgeBaseId">KEY OF KNOWLEDGE BASE.</param>
        /// <param name="request">INPUT DATA.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpPost("{knowledgeBaseId}/votes")]
        public async Task<IActionResult> PostVote(int knowledgeBaseId, [FromBody] VoteCreateRequest request)
        {
            //// GET VOTE WITH ID AND  USER ID (KEY), IF KEY EXIST RETURN STATUS 400
            var vote = await _context.Votes.FindAsync(knowledgeBaseId, request.UserId);
            if (vote != null)
                return BadRequest("This user has been voted for this KB");

            //// CREATE A CONSTANCE OF VOTE
            vote = new Vote()
            {
                KnowledgeBaseId = knowledgeBaseId,
                UserId = request.UserId
            };

            //// INSERT INTO DATABASE
            _context.Votes.Add(vote);

            //// GET KNOWLEDGE BASE WITH ID, IF NULL RETURN STATUS 400
            var knowledgeBase = await _context.KnowledgeBases.FindAsync(knowledgeBaseId);
            if (knowledgeBase == null)
                return BadRequest();
            //// UPDATE NUMBER OF VOTES INCREASE 1 AND  SAVE CHANGE
            knowledgeBase.NumberOfVotes = knowledgeBase.NumberOfVotes.GetValueOrDefault(0) + 1;
            _context.KnowledgeBases.Update(knowledgeBase);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER INSERT IS GREATER THAN 0 (TRUE), RETURN STATUS 204, ELSE RETURN STATUS 400
            if (result > 0)
            {
                return NoContent();
            }
            else
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// DELETE VOTE.
        /// </summary>
        /// <param name="knowledgeBaseId">KEY OF KNOWLEDGE BASE.</param>
        /// <param name="userId">CURRENT USE LOGIN.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpDelete("{knowledgeBaseId}/votes/{userId}")]
        public async Task<IActionResult> DeleteVote(int knowledgeBaseId, string userId)
        {
            //// GET VOTE WITH ID AND  USER ID (KEY), IF KEY EXIST RETURN STATUS 400
            var vote = await _context.Votes.FindAsync(knowledgeBaseId, userId);
            if (vote == null)
                return NotFound();
            //// GET KNOWLEDGE BASE WITH ID, IF NULL RETURN STATUS 400
            var knowledgeBase = await _context.KnowledgeBases.FindAsync(knowledgeBaseId);
            if (knowledgeBase == null)
                return BadRequest();

            //// UPDATE NUMBER OF VOTES DECREASE 1 
            knowledgeBase.NumberOfVotes = knowledgeBase.NumberOfVotes.GetValueOrDefault(0) - 1;
            _context.KnowledgeBases.Update(knowledgeBase);

            //// REMOVE VOTE AND  SAVE CHANGE
            _context.Votes.Remove(vote);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER DELETE IS GREATER THAN 0 (TRUE), RETURN STATUS 200, ELSE RETURN STATUS 400
            if (result > 0)
            {
                return Ok();
            }
            return BadRequest();
        }
        #endregion

        #region REPORTS MANAGEMENT
        /// <summary>
        /// GET ALL REPORTS OF KNOWLEDGE BASE.
        /// </summary>
        /// <param name="knowledgeBaseId">KEY OF KNOWLEDGE BASE.</param>
        /// <param name="filter">KEYWORD SEARCH.</param>
        /// <param name="pageIndex">INDEX OF NEXT PAGE.</param>
        /// <param name="pageSize">NUMBER OF RECORDS IN EACH PAGE.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpGet("{knowledgeBaseId}/reports/filter")]
        public async Task<IActionResult> GetReportsPaging(int knowledgeBaseId, string filter, int pageIndex, int pageSize)
        {
            //// GET ALL REPORT OF KNOWLEDGE BASE
            var query = _context.Reports.Where(x => x.KnowledgeBaseId == knowledgeBaseId).AsQueryable();
            //// IF KEYSEARCH IS NOT NULL OR EMPTY, GET RECORDS WHICH CONSTAINS KEYWORD
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Content.Contains(filter));
            }

            //// TOTAL RECORDS IS NUMBER OF REPROTS's ROWS
            var totalRecords = await query.CountAsync();

            //// TAKE RECORDS IN THE PAGE (NEXT PAGE)
            var items = await query.Skip((pageIndex - 1 * pageSize))
                .Take(pageSize)
                .Select(c => new ReportVm()
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreateDate = c.CreateDate,
                    KnowledgeBaseId = c.KnowledgeBaseId,
                    LastModifiedDate = c.LastModifiedDate,
                    IsProcessed = false,
                    ReportUserId = c.ReportUserId
                })
                .ToListAsync();

            //// PAGINATION
            var pagination = new Pagination<ReportVm>
            {
                Items = items,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }

        /// <summary>
        /// GET REPORT DETAIL.
        /// </summary>
        /// <param name="reportId">KEY OF REPORT.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpGet("{knowledgeBaseId}/reports/{reportId}")]
        public async Task<IActionResult> GetReportDetail(int reportId)
        {
            //// GET REPORT WITH KEY, IF KEY NOT EXIST RETURN STATUS 404
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
                return NotFound();

            //// GIVE INFORMATIONS TO ReportVm (JUST SHOW FIELD NEEDED
            var reportVm = new ReportVm()
            {
                Id = report.Id,
                Content = report.Content,
                CreateDate = report.CreateDate,
                KnowledgeBaseId = report.KnowledgeBaseId,
                LastModifiedDate = report.LastModifiedDate,
                IsProcessed = report.IsProcessed,
                ReportUserId = report.ReportUserId
            };

            return Ok(reportVm);
        }

        /// <summary>
        /// CREATE NEW REPORT.
        /// </summary>
        /// <param name="knowledgeBaseId">KEY OF KNOWLEDGE BASE.</param>
        /// <param name="request">INPUT DATA.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpPost("{knowledgeBaseId}/reports")]
        public async Task<IActionResult> PostReport(int knowledgeBaseId, [FromBody] ReportCreateRequest request)
        {
            //// CREATE A CONSTANCE OF REPORT WITH INFORS ARE INPUT DATA
            var report = new Report()
            {
                Content = request.Content,
                KnowledgeBaseId = knowledgeBaseId,
                ReportUserId = request.ReportUserId,
                IsProcessed = false
            };

            //// INSERT NEW REPORT INTO DATABASE
            _context.Reports.Add(report);

            //// GET KNOWLEDGE BASE WITH ID, IF KEY NOT EXIST RETURN STATUS 400
            var knowledgeBase = await _context.KnowledgeBases.FindAsync(knowledgeBaseId);
            if (knowledgeBase == null)
                return BadRequest();
            //// UPDATE NUMBER OF REPORT IS INCREASE 1 AND SAVE CHANGES
            knowledgeBase.NumberOfReports = knowledgeBase.NumberOfReports.GetValueOrDefault(0) + 1;
            _context.KnowledgeBases.Update(knowledgeBase);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER INSERT IS GREATER THAN 0 (TRUE), RETURN STATUS 200, ELSE RETURN STATUS 400
            if (result > 0)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// UPDATE REPORT.
        /// </summary>
        /// <param name="reportId">KEY OF REPORT.</param>
        /// <param name="request">INPUT DATA.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpPut("{knowledgeBaseId}/reports/{reportId}")]
        public async Task<IActionResult> PutReport(int reportId, [FromBody] CommentCreateRequest request)
        {
            //// GET REPORT WITH KEY, IF KEY NOT EXIST, RETURN STATUS 404
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
                return NotFound();

            //// IF REPORT USER DIFFERENT CURRENT USER, RETURN STATUS 403
            if (report.ReportUserId != User.Identity.Name)
                return Forbid();

            //// UPDATE INFORMATION AND SAVE CHANGE
            report.Content = request.Content;
            _context.Reports.Update(report);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER UPDATE IS GREATER THAN 0 (TRUE), RETURN STATUS 204, ELSE RETURN STATUS 400
            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest();
        }

        /// <summary>
        /// DELETE REPORT.
        /// </summary>
        /// <param name="knowledgeBaseId">KEY OF KNOWLEDGE BASE.</param>
        /// <param name="reportId">KEY OF REPORT.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpDelete("{knowledgeBaseId}/reports/{reportId}")]
        public async Task<IActionResult> DeleteReport(int knowledgeBaseId, int reportId)
        {
            //// GET REPORT WITH KEY, IF KEY NOT EXSIT RETURN STATUS 404
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
                return NotFound();
            //// REMOVE REPORT
            _context.Reports.Remove(report);
            //// GET KNOWLEDGE BASE WITH KEY, IF KEY NOT EXIST, RETURN STATUS 400
            var knowledgeBase = await _context.KnowledgeBases.FindAsync(knowledgeBaseId);
            if (knowledgeBase == null)
                return BadRequest();

            //// UPDATE NUMBER OF REPORTS IS DECREASE 1 AND SAVE CHANGES
            knowledgeBase.NumberOfReports = knowledgeBase.NumberOfReports.GetValueOrDefault(0) - 1;
            _context.KnowledgeBases.Update(knowledgeBase);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER DELETE IS GREATER THAN 0 (TRUE), RETURN STATUS 200, ELSE RETURN STATUS 400
            if (result > 0)
            {
                return Ok();
            }
            return BadRequest();
        }
        #endregion

        #region ATTACHMENT MANAGEMENT
        /// <summary>
        /// GET ATTACHMENTs OF KNOWLEDGE BASE
        /// </summary>
        /// <param name="knowledgeBaseId">KEY OF KNOWLEDGE BASE</param>
        /// <returns>HTTP STATUS</returns>
        [HttpGet("{knowledgeBaseId}/attachments")]
        public async Task<IActionResult> GetAttachment(int knowledgeBaseId)
        {
            var query = await _context.Attachments
                .Where(x => x.KnowledgeBaseId == knowledgeBaseId)
                .Select(c => new AttachmentVm()
                {
                    Id = c.Id,
                    LastModifiedDate = c.LastModifiedDate,
                    CreateDate = c.CreateDate,
                    FileName = c.FileName,
                    FilePath = c.FilePath,
                    FileSize = c.FileSize,
                    FileType = c.FileType,
                    KnowledgeBaseId = (int)c.KnowledgeBaseId
                }).ToListAsync();

            return Ok(query);
        }

        /// <summary>
        /// DELETE ATTACHMENT WITH ID.
        /// </summary>
        /// <param name="attachmentId">KEY OF ATTACHMENT</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpDelete("{knowledgeBaseId}/attachments/{attachmentId}")]
        public async Task<IActionResult> DeleteAttachment(int attachmentId)
        {
            var attachment = await _context.Attachments.FindAsync(attachmentId);
            if (attachment == null)
                return NotFound();

            _context.Attachments.Remove(attachment);

            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok();
            }
            return BadRequest();
        }
        #endregion
    }
}
