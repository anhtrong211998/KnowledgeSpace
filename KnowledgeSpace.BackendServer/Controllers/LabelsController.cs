using KnowledgeSpace.BackendServer.Helpers;
using KnowledgeSpace.BackendServer.Models;
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
    public class LabelsController : BaseController
    {
        private readonly KnowledgeSpaceContext _context;

        public LabelsController(KnowledgeSpaceContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET POPULAR LABELS.
        /// </summary>
        /// <param name="take">NUMBER OF RECORDS NEED SHOW</param>
        /// <returns>HTTP STATUS</returns>
        [HttpGet("popular/{take:int}")]
        [AllowAnonymous]
        public async Task<List<LabelVm>> GetPopularLabels(int take)
        {
            //// GET LABELS IN KNOWLEDGE BASES
            var query = from l in _context.Labels
                        join lik in _context.LabelInKnowledgeBases on l.Id equals lik.LabelId
                        group new { l.Id, l.Name } by new { l.Id, l.Name } into g
                        select new
                        {
                            g.Key.Id,
                            g.Key.Name,
                            Count = g.Count()
                        };

            //// JUST SHOW NEEDED FIELDS
            var labels = await query.OrderByDescending(x => x.Count).Take(take)
                .Select(l => new LabelVm()
                {
                    Id = l.Id,
                    Name = l.Name
                }).ToListAsync();

            return labels;
        }


        /// <summary>
        /// GET LABEL BY ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(string id)
        {
            var label = await _context.Labels.FindAsync(id);
            if (label == null)
                return NotFound(new ApiNotFoundResponse($"Label with id: {id} is not found"));

            var labelVm = new LabelVm()
            {
                Id = label.Id,
                Name = label.Name
            };

            return Ok(labelVm);
        }
    }
}
