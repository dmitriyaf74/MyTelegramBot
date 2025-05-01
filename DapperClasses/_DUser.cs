using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Dapper;

namespace MyTelegramBot.DapperClasses
{
    internal class _DUser
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public long User_Ident { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long Roles_id { get; set; }

        public static long InsertUser(string AconnectionString, _DUser Auser)
        {
            string sql = @"INSERT INTO public.userlist(username, user_ident, firstname, lastname)
                VALUES (@username, @user_ident, @firstname, @lastname) RETURNING id;";

            long vUserId;
            using (var connection = new NpgsqlConnection(AconnectionString))
            {
                connection.Open();
                vUserId = connection.QueryFirstOrDefaultAsync<int>(sql, Auser).Result;
                Auser.Id = vUserId;

                Console.WriteLine($"Inserted User with ID: {vUserId}");
            }
            return vUserId;
        }
        public static _DUser SelectUser(string AconnectionString, long AUser_Ident)
        {
            string sql = @"SELECT id, username, user_ident, firstname, lastname, roles_id 
                FROM userlist WHERE user_ident = @user_ident";

            using (var connection = new NpgsqlConnection(AconnectionString))
            {
                connection.Open();
                _DUser vuser = connection.QueryFirstOrDefault<_DUser>(sql, new { user_ident = AUser_Ident });

                return vuser;
            }
        }
    }
}
