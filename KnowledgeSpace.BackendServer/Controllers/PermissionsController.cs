using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using KnowledgeSpace.ViewModels.Systems;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace KnowledgeSpace.BackendServer.Controllers
{
    public class PermissionsController : BaseController
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// CONSTRUCTOR CONTROLLER.
        /// </summary>
        /// <param name="configuration">IConfiguration.</param>
        public PermissionsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// SHOW LIST FUNCTION WITH CORRESSPONDING ACTION INCLUDED IN EACH FUNCTIONS.
        /// </summary>
        /// <returns>LIST OF PERMISSION IN EACH FUNCTIONS.</returns>
        [HttpGet]
        public async Task<IActionResult> GetCommandViews()
        {
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                //// IF CONNECTION IS CLOSED, OPEN CONNECTION
                if (conn.State == ConnectionState.Closed)
                {
                    await conn.OpenAsync();
                }

                //// GET PERMISSION BY FUNCTION WITH CORESSPONDING COMMAND
                var sql = @"SELECT f.Id,
	                       f.Name,
	                       f.ParentId,
	                       sum(case when sa.Id = 'CREATE' then 1 else 0 end) as HasCreate,
	                       sum(case when sa.Id = 'UPDATE' then 1 else 0 end) as HasUpdate,
	                       sum(case when sa.Id = 'DELETE' then 1 else 0 end) as HasDelete,
	                       sum(case when sa.Id = 'VIEW' then 1 else 0 end) as HasView,
	                       sum(case when sa.Id = 'APPROVE' then 1 else 0 end) as HasApprove
                        from Functions f join CommandInFunctions cif on f.Id = cif.FunctionId
		                    left join Commands sa on cif.CommandId = sa.Id
                        GROUP BY f.Id,f.Name, f.ParentId
                        order BY f.ParentId";

                //// GIVE INFO INTO PermissionScreenVm (JUST SHOW NEEDED FIELD ) AND RETURN WITH HTTP STATUS IS 200
                var result = await conn.QueryAsync<PermissionScreenVm>(sql, null, null, 120, CommandType.Text);
                return Ok(result.ToList());
            }
        }
    }
}
