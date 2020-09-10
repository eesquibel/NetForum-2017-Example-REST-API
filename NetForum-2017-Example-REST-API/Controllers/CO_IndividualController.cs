using Avectra.netForum.Common;
using Avectra.netForum.Data;
using NetForumRestExample.Utility;
using Newtonsoft.Json.Linq;

using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace NetForumRestExample.Controllers
{
    [Authorize(Roles = "iWeb"), Authorize(Roles = "netFORUMUser")]
    public class CO_IndividualController : ApiController
    {
        [NonAction]
        private JObject GetFields(FacadeClass individual)
        {
            var fields = new JObject();
            foreach (var column in new string[] { "cst_key", "ind_first_name", "ind_last_name", "ind_badge_name", "eml_address", "cxa_mailing_label" })
            {
                var field = individual.GetField(column);

                if (field != null)
                {
                    fields[column] = field.ValueNative;
                }
            }

            return fields;
        }

        [HttpGet]
        public IHttpActionResult Get()
        {
            var currentUserName = Config.CurrentUserName;
            var userOptions = Config.UserOptions;
            var currentUserKey = Config.CurrentUserKey;
            var session = Config.Session;

            var identity = (ClaimsIdentity)User.Identity;
            var usr_code = identity.Claims.FirstOrDefault(claim => claim.Type == "usr_code")?.Value;

            if (string.IsNullOrEmpty(usr_code))
            {
                return BadRequest("Cannot find usr_code");
            }

            var results = new JArray();

            using (var connection = DataUtils.GetConnection())
            using (var transaction = connection.BeginTransaction())
            using (var cmd = new NfDbCommand("SELECT usr_cst_key FROM fw_user WHERE usr_code = @usr_code AND usr_delete_flag = 0", connection, transaction))
            {
                cmd.Parameters.AddWithValue("@usr_code", usr_code);

                var result = cmd.ExecuteScalar();

                if (result is Guid cst_key)
                {
                    using (var facade = FacadeObjectFactory.CreateIndividual())
                    {
                        facade.CurrentKey = cst_key.ToString();
                        facade.SelectByKey(connection, transaction);

                        // Suggest using a facade-to-model mapping solution here instead
                        var fields = GetFields(facade);

                        results.Add(fields);
                    }
                }
            }

            return Json(results);
        }

        [HttpPut]
        [Authorize(Roles = "netFORUMAdmin")]
        public IHttpActionResult Put(Guid id, [FromBody] JObject fields)
        {
            JObject result;

            using (var connection = DataUtils.GetConnection())
            using (var transaction = connection.BeginTransaction())
            using (var facade = FacadeObjectFactory.CreateIndividual())
            {
                facade.CurrentKey = id.ToString();
                facade.SelectByKey(connection, transaction);

                facade.Merge(fields);

                facade.Update(connection, transaction);

                facade.LoadRelatedData(connection, transaction);
                facade.ProcessRoundTripEvents(connection, transaction);

                result = GetFields(facade);

                transaction.Commit();
            }

            return Json(result);
        }
    }
}
