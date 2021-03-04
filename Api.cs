using System;
using System.Net;
using Backend.SQL;
using Newtonsoft.Json;
using System.Text;
using Backend.Extensions;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Backend.API
{
    sealed class Api
    {
        private static string TryGetString(Dictionary<string,string> dc, string prop, string def = "0")
        {
            string val;
            dc.TryGetValue(prop, out val);
            return val ?? def;
        }
        private Api() { }
        public static SQLConnection Sql { get; set; }
        public static void GetData(HttpListenerResponse res, HttpListenerRequest req)
        {
            var dc = req.Url.ParseQuery();
            if(!dc.ContainsKey("tb"))
            {
                res.StatusCode = 302;
                res.Close();
                return;
            }

            int lim;
            int.TryParse(TryGetString(dc,"lm"), out lim);

            string query = $"select * from {dc["tb"]} where ID>'{TryGetString(dc, "st")}' limit {lim}";
            Console.WriteLine("Excuting query:\n" + query);

            MySqlDataReader read;
            try
            {
                read = Sql.Query(query);
            }
            catch (Exception e)
            {
                res.StatusCode = 302;
                res.Close();
                Console.WriteLine("Error excuting query\n" + e.Message);
                return;
            }

            var students = new Dictionary<string,string>[int.Parse(dc["lm"])];
            int i = 0;
            for(;read.Read(); i++)
            {
                students[i] = EXT.DictionaryFromSQLDataReader(read);
            }

            byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new ArraySegment<Dictionary<string, string>>(students, 0, i)));
            res.ContentLength64 = bytes.Length;
            res.Headers.Add("Content-Type: text/json; charset=utf-8");
            res.StatusCode = 200;
            res.OutputStream.Write(bytes, 0, bytes.Length);

            read.Close();
            res.Close();
        }
        public static void PutStudent(HttpListenerResponse res, HttpListenerRequest req)
        {
            var dc = req.Url.ParseQuery();
            if (!dc.ContainsKey("tb"))
            {
                res.StatusCode = 302;
                res.Close();
                return;
            }

            string query = $"insert into {dc["tb"]} (";
            foreach (var key in dc.Keys)
            {
                query += key + ",";
            } query.Remove(query.Length - 1, ',');
            query += ") values (";
            foreach (var val in dc.Values)
            {
                query += $"'{val}',";
            } query.Remove(query.Length - 1, ',');
            query += ");";
            Console.WriteLine("Excuting query:\n" + query);

            try
            {
                if (Sql.Statement(query) > 0)
                {
                    res.StatusCode = 200;
                }
                else
                {
                    res.StatusCode = 402;
                }
            }
            catch (Exception e)
            {
                res.StatusCode = 302;
                res.Close();
                Console.WriteLine("Error excuting query:\n" + e.Message);
                return;
            }
            
            res.StatusCode = 200;
            res.Close();
        }
        public static void UpdateStudent(HttpListenerResponse res, HttpListenerRequest req)
        {
            var dc = req.Url.ParseQuery();
            if (!dc.ContainsKey("tb"))
            {
                res.StatusCode = 302;
                res.Close();
                return;
            }
            string reqTB = dc["tb"];

            string query = $"update {reqTB} set ";

            foreach (var v in dc)
            {
                query += v.Key + $"='{v.Value}',";
            } query.Remove(query.Length-1,',');

            query += $" where {dc.Keys.GetEnumerator().Current}='{dc.Values.GetEnumerator().Current}'";
            Console.WriteLine("Excuting statement:\n"+query);

            try
            {
                if (Sql.Statement(query) > 0)
                {
                    res.StatusCode = 200;
                }
                else
                {
                    res.StatusCode = 402;
                }
            }
            catch (Exception e)
            {
                res.StatusCode = 302;
                res.Close();
                Console.WriteLine("Error excuting statement:\n" + e.Message);
                return;
            }

            res.Close();
        }
        public static void DeleteStudent(HttpListenerResponse res, HttpListenerRequest req)
        {
            var dc = req.Url.ParseQuery();
            if (!dc.ContainsKey("tb"))
            {
                res.StatusCode = 302;
                res.Close();
                return;
            }
            string query = $"delete from {dc["tb"]} where {dc.Keys.GetEnumerator().Current}='{dc.Values.GetEnumerator().Current}'";
            Console.WriteLine("Excuting statement\n" + query);

            try
            {
                if (Sql.Statement(query) > 0)
                {
                    res.StatusCode = 200;
                }
                else
                {
                    res.StatusCode = 402;
                }
            }
            catch (Exception e)
            {
                res.StatusCode = 302;
                res.Close();
                Console.WriteLine("Error excuting statement:\n" + e.Message);
                return;
            }

            Console.WriteLine("Excuting query:\n" + query);

            res.StatusCode = 200;
            res.Close();
        }
        public static void GetAvalibleTables(HttpListenerResponse res, HttpListenerRequest req)
        {
            string query = $"select TABLE_NAME from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA='{Sql.Database}'";
            Console.WriteLine("Excuting query:\n" + query);

            MySqlDataReader rd;
            try
            {
                rd = Sql.Query(query);
            }
            catch (Exception e)
            {
                res.StatusCode = 302;
                res.Close();
                Console.WriteLine("Error excuting query:\n" + e.Message);
                return;
            }

            var dc = new List<string>();
            while (rd.Read())
            {
                dc.Add(EXT.DictionaryFromSQLDataReader(rd)["TABLE_NAME"]);
            }
            byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dc));

            res.ContentLength64 = bytes.Length;
            res.ContentEncoding = Encoding.UTF8;
            res.StatusCode = 200;
            res.OutputStream.Write(bytes, 0, bytes.Length);

            rd.Close();
            res.Close();
        }
        public static void GetTableColumns(HttpListenerResponse res, HttpListenerRequest req)
        {
            var dc = req.Url.ParseQuery();
            if (!dc.ContainsKey("tb"))
            {
                res.StatusCode = 302;
                res.Close();
                return;
            }
            string query = $"SELECT COLUMN_NAME, ORDINAL_POSITION, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'{dc["tb"]}'";

            MySqlDataReader read;
            try
            {
                read = Sql.Query(query);
            }
            catch (Exception e)
            {
                res.StatusCode = 302;
                res.Close();
                Console.WriteLine("Error excuting query\n" + e.Message);
                return;
            }

            var dca = new List<Dictionary<string, string>>();
            while (read.Read())
            {
                dca.Add(EXT.DictionaryFromSQLDataReader(read));
            }
            Console.WriteLine(JsonConvert.SerializeObject(dca));
            byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dca));
            res.StatusCode = 200;
            res.ContentLength64 = bytes.Length;
            res.ContentEncoding = Encoding.UTF8;
            res.OutputStream.Write(bytes, 0, bytes.Length);

            read.Close();
            res.Close();
        }
    }
}
