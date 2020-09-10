using Avectra.netForum.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NetForumRestExample.Utility
{
    public static class SearchUtils
    {
        public static void JTokenParse(NfDbCommand cmd, JToken token, string field, string op = "=")
        {
            switch (token.Type)
            {
                case JTokenType.Boolean:
                    bool b = (bool)token;

                    cmd.CommandText += $" {op} @{field}";
                    cmd.Parameters.AddWithValue(field, b ? 1 : 0);
                    break;

                case JTokenType.Float:
                    float f = (float)token;

                    cmd.CommandText += $" {op} @{field}";
                    cmd.Parameters.AddWithValue(field, f);
                    break;

                case JTokenType.Integer:
                    int i = (int)token;

                    cmd.CommandText += $" {op} @{field}";
                    cmd.Parameters.AddWithValue(field, i);
                    break;

                case JTokenType.Null:
                    cmd.CommandText += $" IS NULL";
                    break;

                default:
                    string s = token.ToString();

                    if (s == "IS NULL")
                    {
                        cmd.CommandText += " IS NULL";
                    }
                    else if (s == "IS NOT NULL")
                    {
                        cmd.CommandText += " IS NOT NULL";
                    }
                    else
                    {
                        cmd.CommandText += $" {op} @{field}";
                        cmd.Parameters.AddWithValue(field, s);
                    }
                    break;
            }
        }

    }
}