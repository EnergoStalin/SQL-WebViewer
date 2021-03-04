using System;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Backend.SQL
{
    class SQLConnection
    {
        private readonly string ConnectionString;
        private MySqlConnection Connection = null;
        private MySqlCommand Command = null;

        public SQLConnection(string connection)
        {
            ConnectionString = connection;

            try
            {
                Connection = new MySqlConnection(ConnectionString);
                Connection.Open();
                Command = new MySqlCommand();
                Command.Connection = Connection;
            }
            catch (MySqlException e)
            {
                Console.WriteLine("Exception while connecting to database.\n" + e.Message);
                throw new Exception();
            }
        }
        public string Database => Array.Find(ConnectionString.Split(';'), (string v) => { return v.Split('=')[0] == "database"; }).Split('=')[1];
        public MySqlDataReader Query(string q)
        {
            Command.CommandText = q;
            return Command.ExecuteReader();
        }
        public int Statement(string q)
        {
            Command.CommandText = q;
            return Command.ExecuteNonQuery();
        }
        public void Dispose()
        {
            Command?.Dispose();
            Connection?.Close();

            Command = null;
            Connection = null;
        }
        ~SQLConnection()
        {
            Dispose();
        }
    }
}
