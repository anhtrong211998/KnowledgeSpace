using KnowledgeSpace.BackendServer.Extensions;
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
        #region REPORTS MANAGEMENT
        /// <summary>
        /// GET ALL REPORTS OF KNOWLEDGE BASE.
        /// </summary>
        /// <param name="knowledgeBaseId">KEY OF KNOWLEDGE BASE.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpGet("{knowledgeBaseId}/reports")]
        public async Task<IActionResult> GetReports(int? knowledgeBaseId)
        {
            //// GET ALL REPORT OF KNOWLEDGE BASE
            var query = from r in _context.Reports
                        select new { r};
            if (knowledgeBaseId.HasValue)
            {
                query = query.Where(x => x.r.KnowledgeBaseId == knowledgeBaseId.Value);
            }

            //// TOTAL RECORDS IS NUMBER OF REPROTS's ROWS
            var totalRecords = await query.CountAsync();

            //// TAKE RECORDS IN THE PAGE (NEXT PAGE)
            var items = await query.Select(c => new ReportVm()
                {
                    Id = c.r.Id,
                    Content = c.r.Content,
                    CreateDate = c.r.CreateDate,
                    KnowledgeBaseId = c.r.KnowledgeBaseId,
                    LastModifiedDate = c.r.LastModifiedDate,
                    IsProcessed = false,
                    ReportUserId = c.r.ReportUserId,
                })
                .ToListAsync();

            return Ok(items);
        }


        /// <summary>
        /// GET ALL REPORTS OF KNOWLEDGE BASE.
        /// </summary>
        /// <param name="knowledgeBaseId">KEY OF KNOWLEDGE BASE.</param>
        /// <param name="filter">KEYWORD SEARCH.</param>
        /// <param name="pageIndex">INDEX OF NEXT PAGE.</param>
        /// <param name="pageSize">NUMBER OF RECORDS IN EACH PAGE.</param>
        /// <returns>HTTP STATUS.</returns>
        [HttpGet("{knowledgeBaseId}/reports/filter")]
        public async Task<IActionResult> GetReportsPaging(int? knowledgeBaseId, string filter, int pageIndex, int pageSize)
        {
            //// GET ALL REPORT OF KNOWLEDGE BASE
            var query = from r in _context.Reports
                        join u in _context.Users
                            on r.ReportUserId equals u.Id
                        select new { r, u };
            if (knowledgeBaseId.HasValue)
            {
                query = query.Where(x => x.r.KnowledgeBaseId == knowledgeBaseId.Value);
            }

            //// IF KEYSEARCH IS NOT NULL OR EMPTY, GET RECORDS WHICH CONSTAINS KEYWORD
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.r.Content.Contains(filter));
            }

            //// TOTAL RECORDS IS NUMBER OF REPROTS's ROWS
            var totalRecords = await query.CountAsync();

            //// TAKE RECORDS IN THE PAGE (NEXT PAGE)
            var items = await query.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ReportVm()
                {
                    Id = c.r.Id,
                    Content = c.r.Content,
                    CreateDate = c.r.CreateDate,
                    KnowledgeBaseId = c.r.KnowledgeBaseId,
                    LastModifiedDate = c.r.LastModifiedDate,
                    IsProcessed = false,
                    ReportUserId = c.r.ReportUserId,
                    ReportUserName = c.u.FirstName + " " + c.u.LastName
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
                return NotFound(new ApiNotFoundResponse($"Cannot found report with key {reportId}"));
            var user = await _context.Users.FindAsync(report.ReportUserId);

            //// GIVE INFORMATIONS TO ReportVm (JUST SHOW FIELD NEEDED
            var reportVm = new ReportVm()
            {
                Id = report.Id,
                Content = report.Content,
                CreateDate = report.CreateDate,
                KnowledgeBaseId = report.KnowledgeBaseId,
                LastModifiedDate = report.LastModifiedDate,
                IsProcessed = report.IsProcessed,
                ReportUserId = report.ReportUserId,
                ReportUserName = user.FirstName + " " + user.LastName
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
        [ApiValidationFilter]
        public async Task<IActionResult> PostReport(int knowledgeBaseId, [FromBody] ReportCreateRequest request)
        {
            //// CREATE A CONSTANCE OF REPORT WITH INFORS ARE INPUT DATA
            var report = new Report()
            {
                Content = request.Content,
                KnowledgeBaseId = knowledgeBaseId,
                ReportUserId = User.GetUserId(),
                IsProcessed = false
            };

            //// INSERT NEW REPORT INTO DATABASE
            _context.Reports.Add(report);

            //// GET KNOWLEDGE BASE WITH ID, IF KEY NOT EXIST RETURN STATUS 400
            var knowledgeBase = await _context.KnowledgeBases.FindAsync(knowledgeBaseId);
            if (knowledgeBase == null)
                return BadRequest(new ApiBadRequestResponse($"Cannot found knowledge base with key {knowledgeBaseId}"));
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
                return BadRequest(new ApiBadRequestResponse($"Create report failed"));
            }
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
                return NotFound(new ApiNotFoundResponse($"Cannot found report with key {reportId}"));
            //// REMOVE REPORT
            _context.Reports.Remove(report);
            //// GET KNOWLEDGE BASE WITH KEY, IF KEY NOT EXIST, RETURN STATUS 400
            var knowledgeBase = await _context.KnowledgeBases.FindAsync(knowledgeBaseId);
            if (knowledgeBase == null)
                return BadRequest(new ApiBadRequestResponse($"Cannot found knowledge base with key {knowledgeBaseId}"));

            //// UPDATE NUMBER OF REPORTS IS DECREASE 1 AND SAVE CHANGES
            knowledgeBase.NumberOfReports = knowledgeBase.NumberOfReports.GetValueOrDefault(0) - 1;
            _context.KnowledgeBases.Update(knowledgeBase);
            var result = await _context.SaveChangesAsync();

            //// IF RESULT AFTER DELETE IS GREATER THAN 0 (TRUE), RETURN STATUS 200, ELSE RETURN STATUS 400
            if (result > 0)
            {
                return Ok();
            }
            return BadRequest(new ApiBadRequestResponse($"Delete report failed"));
        }
        #endregion
    }
}
