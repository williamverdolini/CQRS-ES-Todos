using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Todo.Infrastructure;
using Todo.Legacy.Migration.Worker;

namespace Web.UI.Controllers
{
    public class MigrationController : ApiController
    {
        private readonly LegacyMigrationWorker Worker;

        public MigrationController(LegacyMigrationWorker worker)
        {
            Contract.Requires<System.ArgumentNullException>(worker != null, "worker");
            Worker = worker;
        }

        [Route("api/Migrate")]
        [HttpPost]
        public IHttpActionResult Migrate()
        {
            try
            {
                Worker.ExecuteMigration();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
