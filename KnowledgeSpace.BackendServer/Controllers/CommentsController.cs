using KnowledgeSpace.BackendServer.Authorization;
using KnowledgeSpace.BackendServer.Constants;
using KnowledgeSpace.BackendServer.Helpers;
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
    public partial class KnowledgeBasesController
    {
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
        [ClaimRequirement(FunctionCode.CONTENT_COMMENT, CommandCode.VIEW)]
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
                    OwnerUserId = c.OwnerUserId
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

            //// CREATE A CONSTANCE OF COMMENT JUST SHOW NEEDED FIELD
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

        /// <summary>
        /// CREATE NEW COMMENT.
        /// </summary>
        /// <param name="knowledgeBaseId">KEY OF KNOWLEDGE BASE.</param>
        /// <param name="request">INPUT DATA.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpPost("{knowledgeBaseId}/comments")]
        [ClaimRequirement(FunctionCode.CONTENT_COMMENT, CommandCode.CREATE)]
        [ApiValidationFilter]
        public async Task<IActionResult> PostComment(int knowledgeBaseId, [FromBody] CommentCreateRequest request)
        {
            //// CREATE NEW CONSTANCE OF COMMENT WITH INFORMATION ARE INPUT DATA
            var comment = new Comment()
            {
                Content = request.Content,
                KnowledgeBaseId = request.KnowledgeBaseId,
                OwnerUserId = string.Empty/*TODO: GET USER FROM CLAIM*/,
            };

            //// INSERT NEW COMMENT
            _context.Comments.Add(comment);
            //// GET KNOWLEDGE BASE WITH ID (KEY), IF KEY NOT EXIST RETURN STATUS 404
            var knowledgeBase = await _context.KnowledgeBases.FindAsync(knowledgeBaseId);
            if (knowledgeBase == null)
                return BadRequest(new ApiBadRequestResponse($"Cannot found knowledge base with id: {knowledgeBaseId}"));

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
            if (comment.OwnerUserId != User.Identity.Name)
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
                    OwnerUserId = comment.OwnerUserId
                };
                return Ok(commentVm);
            }
            return BadRequest(new ApiBadRequestResponse($"Delete comment failed"));
        }
        #endregion
    }
}
