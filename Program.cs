using System;
using Backend.SQL;
using Backend.Http;
using Backend.API;
using System.IO;
using System.Collections.Generic;

namespace Backend
{
    static class Programm
    {
        static SQLConnection connection = null;
        static HttpServer<Api> server = null;

        static string ConfigPath = "config.cfg";
        static string loadSQLConfig()
        {
            var dc = new Dictionary<string, string>();
            using (var f = File.OpenText(ConfigPath))
            {
                while(!f.EndOfStream)
                {
                    string line = f.ReadLine();
                    if(line == "[SQL]")
                    {
                        line = f.ReadLine();
                        do
                        {
                            if (line.Length == 0) continue;
                            var arr = line.Split('=');
                            dc.Add(arr[0], arr[1]);
                            line = f.ReadLine();
                        }
                        while (!f.EndOfStream && !line.StartsWith("["));
                        break;
                    }
                }
            }
            return $"server={dc["ip"]};uid={dc["user"]};pwd={dc["password"]};database={dc["database"]}";
        }
        static string loadHTTPConfig()
        {
            var dc = new Dictionary<string, string>();
            using (var f = File.OpenText(ConfigPath))
            {
                while (!f.EndOfStream)
                {
                    string line = f.ReadLine();
                    if (line == "[HTTP]")
                    {
                        line = f.ReadLine();
                        do
                        {
                            if (line.Length == 0) continue;
                            var arr = line.Split('=');
                            dc.Add(arr[0], arr[1]);
                            line = f.ReadLine();
                        }
                        while (!f.EndOfStream && !line.StartsWith("["));
                        break;
                    }
                }
            }
            return dc["prefix"];
        }
        static void Main(string[] args)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                connection = new SQLConnection(loadSQLConfig());
                Console.WriteLine("SQL connected sucsessfuly.");

                Api.Sql = connection;
                server = new HttpServer<Api>(loadHTTPConfig());
                Console.WriteLine("Http server up.");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Shutdown...\n" + e.Message);
                Environment.Exit(1);
            }

            Console.ResetColor();
            Console.ReadKey();

            connection?.Dispose();
            server?.Dispose();
        }
    }
}
