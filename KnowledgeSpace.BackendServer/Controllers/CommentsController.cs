using KnowledgeSpace.BackendServer.Authorization;
using KnowledgeSpace.BackendServer.Constants;
using KnowledgeSpace.BackendServer.Extensions;
using KnowledgeSpace.BackendServer.Helpers;
using KnowledgeSpace.BackendServer.Models.Entities;
using KnowledgeSpace.ViewModels;
using KnowledgeSpace.ViewModels.Contents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KnowledgeSpace.BackendServer.Controllers
{
    public partial class KnowledgeBasesController
    {
        #region COMMENT MANAGERMENT
        /// <summary>
        /// GET COMMENTS OF KNOWLEDGE BASE.
        /// </summary>
        /// <param name="knowledgeBaseId">KEY OF KNOWLEDGE BASE.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpGet("{knowledgeBaseId}/comments")]
        [ClaimRequirement(FunctionCode.CONTENT_COMMENT, CommandCode.VIEW)]
        public async Task<IActionResult> GetComments(int? knowledgeBaseId)
        {
            //// GET ALL COMMENTS OF KNOWLEDGE BASE
            var query = from c in _context.Comments
                        select new { c };
            if (knowledgeBaseId.HasValue)
            {
                query = query.Where(x => x.c.KnowledgeBaseId == knowledgeBaseId.Value);
            }

            //// TOTAL RECORDS EQUAL NUMBER OF COMMENTS's ROWS
            var totalRecords = await query.CountAsync();

            //// TAKE RECORDS IN THE PAGE (NEXT PAGE) AND GIVE INFORMATIONS TO CommentVm (JUST SHOW FIELD NEEDED)
            var items = await query.OrderByDescending(x => x.c.CreateDate)
                .Select(c => new CommentVm()
                {
                    Id = c.c.Id,
                    Content = c.c.Content,
                    CreateDate = c.c.CreateDate,
                    KnowledgeBaseId = c.c.KnowledgeBaseId,
                    LastModifiedDate = c.c.LastModifiedDate,
                    OwnerUserId = c.c.OwnerUserId
                })
                .ToListAsync();
            return Ok(items);
        }

        /// <summary>
        /// GET COMMENTS OF KNOWLEDGE BASE WITH FILTER (KEYWORD SEARCH).
        /// </summary>
        /// <param name="knowledgeBaseId">KEY OF KNOWLEDGE BASE.</param>
        /// <param name="filter">KEYWORD SEARCH.</param>
        /// <param name="pageIndex">INDEX OF NEXT PAGE.</param>
        /// <param name="pageSize">NUMBER OF RECORDS IN EACH PAGE.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpGet("{knowledgeBaseId}/comments/filter")]
        [ClaimRequirement(FunctionCode.CONTENT_COMMENT, CommandCode.VIEW)]
        public async Task<IActionResult> GetCommentsPaging(int? knowledgeBaseId, string filter, int pageIndex, int pageSize)
        {
            //// GET ALL COMMENTS OF KNOWLEDGE BASE
            var query = from c in _context.Comments
                        join u in _context.Users
                            on c.OwnerUserId equals u.Id
                        select new { c, u };
            if (knowledgeBaseId.HasValue)
            {
                query = query.Where(x => x.c.KnowledgeBaseId == knowledgeBaseId.Value);
            }
            //// IF KEYWORD NOT NULL OR EMPTY, GET ALL COMMENTS WHICH CONSTAINS KEYWORD
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.c.Content.Contains(filter));
            }

            //// TOTAL RECORDS EQUAL NUMBER OF COMMENTS's ROWS
            var totalRecords = await query.CountAsync();

            //// TAKE RECORDS IN THE PAGE (NEXT PAGE) AND GIVE INFORMATIONS TO CommentVm (JUST SHOW FIELD NEEDED)
            var items = await query.OrderByDescending(x => x.c.CreateDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CommentVm()
                {
                    Id = c.c.Id,
                    Content = c.c.Content,
                    CreateDate = c.c.CreateDate,
                    KnowledgeBaseId = c.c.KnowledgeBaseId,
                    LastModifiedDate = c.c.LastModifiedDate,
                    OwnerUserId = c.c.OwnerUserId,
                    OwnerName = c.u.FirstName + " " + c.u.LastName
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
        [ClaimRequirement(FunctionCode.CONTENT_COMMENT, CommandCode.VIEW)]
        public async Task<IActionResult> GetCommentDetail(int commentId)
        {
            //// GET COMMENT WITH ID (KEY)
            var comment = await _context.Comments.FindAsync(commentId);
            //// IF KEY NOT EXIST (COMMENT IS NULL), RETURN STATUS 404
            if (comment == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found comment with id: {commentId}"));

            //// GET USER OWNER COMMENT
            var user = await _context.Users.FindAsync(comment.OwnerUserId);
            //// CREATE A CONSTANCE OF COMMENT JUST SHOW NEEDED FIELD
            var commentVm = new CommentVm()
            {
                Id = comment.Id,
                Content = comment.Content,
                CreateDate = comment.CreateDate,
                KnowledgeBaseId = comment.KnowledgeBaseId,
                LastModifiedDate = comment.LastModifiedDate,
                OwnerUserId = comment.OwnerUserId,
                OwnerName = user.FirstName + " " + user.LastName
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
        [ApiValidationFilter]
        public async Task<IActionResult> PostComment(int knowledgeBaseId, [FromBody] CommentCreateRequest request)
        {
            //// CREATE NEW CONSTANCE OF COMMENT WITH INFORMATION ARE INPUT DATA
            var comment = new Comment()
            {
                Content = request.Content,
                KnowledgeBaseId = knowledgeBaseId,
                OwnerUserId = User.GetUserId(),
                ReplyId = request.ReplyId
            };

            //// INSERT NEW COMMENT
            _context.Comments.Add(comment);
            //// GET KNOWLEDGE BASE WITH ID (KEY), IF KEY NOT EXIST RETURN STATUS 404
            var knowledgeBase = await _context.KnowledgeBases.FindAsync(knowledgeBaseId);
            if (knowledgeBase == null)
                return BadRequest(new ApiBadRequestResponse($"Cannot found knowledge base with id: {knowledgeBaseId}"));

            //// UPDATE NUMBER OF COMMENTS INCREASE 1 AND SAVE CHANGE
            knowledgeBase.NumberOfComments = knowledgeBase.NumberOfComments.GetValueOrDefault(0) + 1;
            _context.KnowledgeBases.Update(knowledgeBase);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER INSERT IS GREATER THAN 0 (TRUE), RETURN STATUS 201, ELSE RETURN STATUS 400
            if (result > 0)
            {
                await _cacheService.RemoveAsync(CacheConstants.RecentComments);
                return CreatedAtAction(nameof(GetCommentDetail), new { id = knowledgeBaseId, commentId = comment.Id }, new CommentVm()
                {
                    Id = comment.Id
                });
            }
            else
            {
                return BadRequest(new ApiBadRequestResponse("Create comment failed"));
            }
        }

        /// <summary>
        /// UPDATE COMMENT WITH ID (KEY).
        /// </summary>
        /// <param name="commentId">KEY OF COMMENT.</param>
        /// <param name="request">INPUT DATA.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpPut("{knowledgeBaseId}/comments/{commentId}")]
        [ClaimRequirement(FunctionCode.CONTENT_COMMENT, CommandCode.UPDATE)]
        [ApiValidationFilter]
        public async Task<IActionResult> PutComment(int commentId, [FromBody] CommentCreateRequest request)
        {
            //// GET COMMENT WITH ID (KEY)
            var comment = await _context.Comments.FindAsync(commentId);
            //// IF KEY IS NOT EXIST (COMMENT IS NULL), RETURN STATUS 404
            if (comment == null)
                return BadRequest(new ApiBadRequestResponse($"Cannot found comment with id: {commentId}"));
            //// IF OWNWER USER DIFFERENT CURRENT USER, RETURN STATUS 403
            if (comment.OwnerUserId != User.GetUserId())
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
            return BadRequest(new ApiBadRequestResponse($"Update comment failed"));
        }

        /// <summary>
        /// DELETE COMMENT WITH ID (KEY).
        /// </summary>
        /// <param name="knowledgeBaseId">KEY OF KNOWLEDGE BASE.</param>
        /// <param name="commentId">KEY OF COMMENT.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpDelete("{knowledgeBaseId}/comments/{commentId}")]
        [ClaimRequirement(FunctionCode.CONTENT_COMMENT, CommandCode.DELETE)]
        public async Task<IActionResult> DeleteComment(int knowledgeBaseId, int commentId)
        {
            //// GET COMMENT WITH ID (KEY), IF KEY IS NOT EXIST, RETURN STATUS 404
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found the comment with id: {commentId}"));

            //// REMOVE COMMENT
            _context.Comments.Remove(comment);

            //// GET KNOWLEDGE BASE WITH ID, IF NULL, RETURN STATUS 400
            var knowledgeBase = await _context.KnowledgeBases.FindAsync(knowledgeBaseId);
            if (knowledgeBase == null)
                return BadRequest(new ApiBadRequestResponse($"Cannot found knowledge base with id: {knowledgeBaseId}"));

            //// UPDATE NUMBER OF COMMENT IS DECREASE 1 AND SAVE CHANGE
            knowledgeBase.NumberOfComments = knowledgeBase.NumberOfComments.GetValueOrDefault(0) - 1;
            _context.KnowledgeBases.Update(knowledgeBase);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER DELETE IS GREATER THAN 0 (TRUE), RETURN STATUS 200, ELSE RETURN STATUS 400
            if (result > 0)
            {
                //Delete cache
                await _cacheService.RemoveAsync(CacheConstants.RecentComments);
                var commentVm = new CommentVm()
                {
                    Id = comment.Id,
                    Content = comment.Content,
                    CreateDate = comment.CreateDate,
                    KnowledgeBaseId = comment.KnowledgeBaseId,
                    LastModifiedDate = comment.LastModifiedDate,
                    OwnerUserId = comment.OwnerUserId
                };
                return Ok(commentVm);
            }
            return BadRequest(new ApiBadRequestResponse($"Delete comment failed"));
        }

        /// <summary>
        /// GET RECENT COMMENTS
        /// </summary>
        /// <param name="take">NUMBER OF RECORDS NEEDED</param>
        /// <returns>HTTP STATUS</returns>
        [HttpGet("comments/recent/{take}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRecentComments(int take)
        {
            var cachedData = await _cacheService.GetAsync<List<CommentVm>>(CacheConstants.RecentComments);
            if (cachedData == null)
            {
                var query = from c in _context.Comments
                            join u in _context.Users
                                on c.OwnerUserId equals u.Id
                            join k in _context.KnowledgeBases
                            on c.KnowledgeBaseId equals k.Id
                            orderby c.CreateDate descending
                            select new { c, u, k };

                var comments = await query.Take(take).Select(x => new CommentVm()
                {
                    Id = x.c.Id,
                    CreateDate = x.c.CreateDate,
                    KnowledgeBaseId = x.c.KnowledgeBaseId,
                    OwnerUserId = x.c.OwnerUserId,
                    KnowledgeBaseTitle = x.k.Title,
                    OwnerName = x.u.FirstName + " " + x.u.LastName,
                    Content = x.c.Content,
                    ReplyId = x.c.ReplyId,
                    KnowledgeBaseSeoAlias = x.k.SeoAlias
                }).ToListAsync();

                await _cacheService.SetAsync(CacheConstants.RecentComments, comments,24);
                cachedData = comments;
            }
                
            return Ok(cachedData);
        }

        /// <summary>
        /// GET ALL COMMENTS OF KNOWLEDGEBASE
        /// </summary>
        /// <param name="knowledgeBaseId">KEY OF KNOWLEDGEBASE</param>
        /// <returns>HTTP STATUS</returns>
        [HttpGet("{knowledgeBaseId}/comments/tree")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCommentTreeByKnowledgeBaseId(int knowledgeBaseId, int pageIndex, int pageSize)
        {
            var query = from c in _context.Comments
                        join u in _context.Users
                            on c.OwnerUserId equals u.Id
                        where c.KnowledgeBaseId == knowledgeBaseId
                        where c.ReplyId == null
                        select new { c, u };

            var totalRecords = await query.CountAsync();
            var rootComments = await query.OrderByDescending(x => x.c.CreateDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CommentVm()
                {
                    Id = x.c.Id,
                    CreateDate = x.c.CreateDate,
                    KnowledgeBaseId = x.c.KnowledgeBaseId,
                    OwnerUserId = x.c.OwnerUserId,
                    Content = x.c.Content,
                    ReplyId = x.c.ReplyId,
                    OwnerName = x.u.FirstName + " " + x.u.LastName,
                })
                .ToListAsync();

            foreach (var comment in rootComments)//only loop through root categories
            {
                // you can skip the check if you want an empty list instead of null
                // when there is no children
                var repliedQuery = from c in _context.Comments
                                   join u in _context.Users
                                       on c.OwnerUserId equals u.Id
                                   where c.KnowledgeBaseId == knowledgeBaseId
                                   where c.ReplyId == comment.Id
                                   select new { c, u };

                var totalRepliedCommentsRecords = await repliedQuery.CountAsync();
                var repliedComments = await repliedQuery.OrderByDescending(x => x.c.CreateDate)
                    .Take(pageSize)
                    .Select(x => new CommentVm()
                    {
                        Id = x.c.Id,
                        CreateDate = x.c.CreateDate,
                        KnowledgeBaseId = x.c.KnowledgeBaseId,
                        OwnerUserId = x.c.OwnerUserId,
                        Content = x.c.Content,
                        ReplyId = x.c.ReplyId,
                        OwnerName = x.u.FirstName + " " + x.u.LastName,
                    })
                    .ToListAsync();

                comment.Children = new Pagination<CommentVm>()
                {
                    PageIndex = 1,
                    PageSize = 10,
                    Items = repliedComments,
                    TotalRecords = totalRepliedCommentsRecords
                };
            }

            return Ok(new Pagination<CommentVm>
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Items = rootComments,
                TotalRecords = totalRecords
            });
        }

        [HttpGet("{knowledgeBaseId}/comments/{rootCommentId}/replied")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRepliedCommentsPaging(int knowledgeBaseId, int rootCommentId, int pageIndex, int pageSize)
        {
            var query = from c in _context.Comments
                        join u in _context.Users
                            on c.OwnerUserId equals u.Id
                        where c.KnowledgeBaseId == knowledgeBaseId
                        where c.ReplyId == rootCommentId
                        select new { c, u };

            var totalRecords = await query.CountAsync();
            var comments = await query.OrderByDescending(x => x.c.CreateDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CommentVm()
                {
                    Id = x.c.Id,
                    CreateDate = x.c.CreateDate,
                    KnowledgeBaseId = x.c.KnowledgeBaseId,
                    OwnerUserId = x.c.OwnerUserId,
                    Content = x.c.Content,
                    ReplyId = x.c.ReplyId,
                    OwnerName = x.u.FirstName + " " + x.u.LastName,
                })
                .ToListAsync();

            return Ok(new Pagination<CommentVm>
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Items = comments,
                TotalRecords = totalRecords
            });
        }
        #endregion
    }
}
