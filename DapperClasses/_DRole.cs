using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Dapper;

namespace MyTelegramBot.DapperClasses
{
    internal class _DRole
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public static List<_DRole> SelectRoles(string AconnectionString, long Auser_id)
        {
            string sql = @"select r.id, r.name from userroles ur join roles r on r.id = ur.role_id where ur.user_id = @user_id and ur.enabled = true";
            DatabaseHelper dbHelper = new DatabaseHelper(AconnectionString);
            object parameters = new { user_id = Auser_id };
            List<_DRole> items = dbHelper.GetList<_DRole>(sql, parameters);

            return items;
        }
    }
}
