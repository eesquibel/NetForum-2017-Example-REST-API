using Avectra.netForum.Common;
using Avectra.netForum.Data;
using NetForumRestExample.Utility;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Http;

namespace NetForumRestExample.Controllers
{
    [Authorize(Roles = "iWeb"), Authorize(Roles = "netFORUMUser")]
    public class mb_membershipController : ApiController
    {
        [NonAction]
        private JObject GetFields(FacadeClass membership)
        {
            var fields = new JObject();
            foreach (var column in new string[] { "mbr_key", "mbr_asn_code", "mbt_key", "mbt_code", "mbr_cst_key", "mbr_auto_pay", "mbr_cpi_key" })
            {
                var field = membership.GetField(column);

                if (field != null)
                {
                    fields[column] = field.ValueNative;
                }
            }

            return fields;
        }

        // GET: api/mb_membership
        /// <summary>
        /// Get a list of memberships, filtered by a where condition
        /// </summary>
        /// <param name="where">JSON encoded list of fields to search the database for</param>
        /// <remarks>
        /// ?where={ "field1": "value", "field2": { "eq": "value }, "field3": { "gt": 5 }, "field4": "IS NULL" }
        /// </remarks>
        /// <returns>List of memberships</returns>
        public IHttpActionResult Get([FromUri] string where)
        {
            JArray results = new JArray();
            JObject whereFields = null;
            Dictionary<string, object> searchFields = new Dictionary<string, object>();

            try
            {
                whereFields = JObject.Parse(where);
            }
            catch (Exception)
            {

            }

            if (whereFields != null)
            {
                using (var facade = FacadeObjectFactory.CreateMembership())
                {
                    // Quick and dirty field validation
                    var verify = new Regex(@"^[a-z0-9_]+$");

                    foreach (var pair in whereFields)
                    {
                        if (!verify.IsMatch(pair.Key))
                        {
                            continue;
                        }

                        if (pair.Value == null)
                        {
                            continue;
                        }

                        var field = facade.GetField(pair.Key);
                        var parts = pair.Key.Split('_');

                        if (field == null)
                        {
                            continue;
                        }

                        if (parts[0] != "mbr" && parts[0] != "mbt")
                        {
                            continue;
                        }

                        searchFields.Add(pair.Key, pair.Value);
                    }
                }
            }

            // Add top just for sanity reasons
            var SQL = @"SELECT TOP 100 mbr_key FROM mb_membership JOIN mb_member_type ON mbt_key = mbr_mbt_key AND mbt_delete_flag = 0 WHERE mbr_delete_flag = 0";

            using (var connection = DataUtils.GetConnection())
            using (var transaction = connection.BeginTransaction())
            using (var cmd = new NfDbCommand(SQL, connection, transaction))
            {
                foreach (var field in searchFields.Keys)
                {
                    cmd.CommandText += Environment.NewLine + $" AND [{field}]";

                    var search = searchFields[field];

                    if (search is JObject searchOperation)
                    {
                        if (searchOperation.Count != 1)
                        {
                            continue;
                        }

                        var op = (JProperty)searchOperation.First;

                        switch (op.Name)
                        {
                            case "eq":
                                SearchUtils.JTokenParse(cmd, op.Value, field, "=");
                                break;
                            case "ne":
                                SearchUtils.JTokenParse(cmd, op.Value, field, "!=");
                                break;
                            case "gt":
                                SearchUtils.JTokenParse(cmd, op.Value, field, ">");
                                break;
                            case "ge":
                                SearchUtils.JTokenParse(cmd, op.Value, field, ">=");
                                break;
                            case "lt":
                                SearchUtils.JTokenParse(cmd, op.Value, field, "<");
                                break;
                            case "le":
                                SearchUtils.JTokenParse(cmd, op.Value, field, "<=");
                                break;
                        }
                    }
                    else if (search is JToken searchValue)
                    {
                        SearchUtils.JTokenParse(cmd, searchValue, field, "=");
                    }
                }

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Guid mbr_key = reader.GetGuid(0);

                        using (var facade = FacadeObjectFactory.CreateMembership())
                        {
                            facade.CurrentKey = mbr_key.ToString();
                            facade.SelectByKey(connection, transaction);

                            // Suggest using a facade-to-model mapping solution here instead
                            var fields = GetFields(facade);

                            results.Add(fields);
                        }
                    }
                }
            }

            return Json(results);
        }

        // GET: api/mb_membership/5
        public IHttpActionResult Get(Guid id)
        {
            using (var connection = DataUtils.GetConnection())
            using (var transaction = connection.BeginTransaction())
            using (var facade = FacadeObjectFactory.CreateMembership())
            {
                facade.CurrentKey = id.ToString();
                facade.SelectByKey(connection, transaction);

                var fields = GetFields(facade);

                return Json(fields);
            }
        }

        // POST: api/mb_membership
        [HttpPost]
        [Authorize(Roles = "netFORUMAdmin")]
        public IHttpActionResult Post([FromBody] JObject value)
        {
            throw new NotImplementedException();
        }

        // PUT: api/mb_membership/5
        [HttpPut]
        [Authorize(Roles = "netFORUMAdmin")]
        public IHttpActionResult Put(Guid id, [FromBody] JObject fields)
        {
            JObject result;

            using (var connection = DataUtils.GetConnection())
            using (var transaction = connection.BeginTransaction())
            using (var facade = FacadeObjectFactory.CreateMembership())
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

        // DELETE: api/mb_membership/5
        [HttpDelete]
        [Authorize(Roles = "netFORUMAdmin")]
        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
