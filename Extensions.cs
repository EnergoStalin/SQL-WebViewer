using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace Backend.Extensions
{
    public static class EXT
    {
        public static Dictionary<string, string> ParseQuery(this Uri uri)
        {
            if (uri.Query == string.Empty) return new Dictionary<string, string>();
            var a = uri.ToString().Substring(uri.ToString().LastIndexOf('?')).Remove(0,1).Split('&');
            var dict = new Dictionary<string, string>();
            foreach (var item in a)
            {
                var str = item.Split('=');
                dict.Add(str[0], str[1]);
            }

            return dict;
        }

        public static Dictionary<string, string> DictionaryFromSQLDataReader(MySqlDataReader reader)
        {
            var dc = new Dictionary<string, string>();
            for(int i = 0; i < reader.FieldCount; i++)
            {
                dc[reader.GetName(i)] = reader.GetString(i);
            }

            return dc;
        }
    }
}
