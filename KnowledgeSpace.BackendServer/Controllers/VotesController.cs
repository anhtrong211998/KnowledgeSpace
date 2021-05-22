using KnowledgeSpace.BackendServer.Helpers;
using KnowledgeSpace.BackendServer.Models.Entities;
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
        [ApiValidationFilter]
        public async Task<IActionResult> PostVote(int knowledgeBaseId, [FromBody] VoteCreateRequest request)
        {
            //// GET VOTE WITH ID AND  USER ID (KEY), IF KEY EXIST RETURN STATUS 400
            var vote = await _context.Votes.FindAsync(knowledgeBaseId, request.UserId);
            if (vote != null)
                return BadRequest(new ApiBadRequestResponse("This user has been voted for this KB"));

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
                return BadRequest(new ApiBadRequestResponse($"Cannot found knowledge base with id {knowledgeBaseId}"));
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
                return BadRequest(new ApiBadRequestResponse($"Vote failed"));
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
                return NotFound(new ApiNotFoundResponse("Cannot found vote"));
            //// GET KNOWLEDGE BASE WITH ID, IF NULL RETURN STATUS 400
            var knowledgeBase = await _context.KnowledgeBases.FindAsync(knowledgeBaseId);
            if (knowledgeBase == null)
                return BadRequest(new ApiBadRequestResponse($"Cannot found knowledge base with id {knowledgeBaseId}"));

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
            return BadRequest(new ApiBadRequestResponse("Delete vote failed"));
        }
        #endregion
    }
}
