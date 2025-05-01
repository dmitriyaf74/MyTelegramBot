using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace MyTelegramBot.DapperClasses
{
    public class DatabaseHelper
    {
        private string _connectionString;

        public DatabaseHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<T> GetList<T>(string sql, object parameters = null)
        {
            using (NpgsqlConnection connection = new (_connectionString))
            {
                connection.Open();
                IEnumerable<T> result = connection.Query<T>(sql, parameters);
                return result.AsList();
            }
        }
    }

}
