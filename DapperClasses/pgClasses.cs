using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTelegramBot.DapperClasses
{
    internal class pgQuery : CustomQuery
    {
        public override List<CustomRole> SelectRoles(string AconnectionString, long Auser_id)
        {
            string sql = @"select r.id, r.name from userroles ur join roles r on r.id = ur.role_id where ur.user_id = @user_id and ur.enabled = true";
            DatabaseHelper dbHelper = new DatabaseHelper(AconnectionString);
            object parameters = new { user_id = Auser_id };
            List<CustomRole> items = dbHelper.GetList<CustomRole>(sql, parameters);

            return items;
        }

        public override long InsertUser(string AconnectionString, CustomUser Auser)
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
        public override CustomUser SelectUser(string AconnectionString, long AUser_Ident)
        {
            string sql = @"SELECT id, username, user_ident, firstname, lastname, roles_id 
                FROM userlist WHERE user_ident = @user_ident";

            using (var connection = new NpgsqlConnection(AconnectionString))
            {
                connection.Open();
                CustomUser vuser = connection.QueryFirstOrDefault<CustomUser>(sql, new { user_ident = AUser_Ident });

                return vuser;
            }
        }
    }
    
}
