using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTelegramBot.DapperClasses
{
    internal class pgQueryUser : CustomQuery
    {
        private string ConnectionString;
        public pgQueryUser(string AconnectionString)
        {
            ConnectionString = AconnectionString;
        }

        #region UserInfo
        public List<CustomRole> SelectRoles(long? Auser_id)
        {
            string sql = @"select r.id, r.name from userroles ur join roles r on r.id = ur.role_id where ur.user_id = @user_id and ur.enabled = true";
            DatabaseHelper dbHelper = new DatabaseHelper(ConnectionString);
            object parameters = new { user_id = Auser_id };
            return dbHelper.GetList<CustomRole>(sql, parameters);
        }
                        
        public long InsertUser(CustomUser Auser)
        {
            string sql = @"INSERT INTO userlist(username, user_ident, firstname, lastname, roles_id)
                VALUES (@username, @user_ident, @firstname, @lastname, @roles_id) RETURNING id;";

            long vUserId;
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                vUserId = connection.QueryFirstOrDefaultAsync<int>(sql, Auser).Result;
                Auser.Id = vUserId;
            }
            return vUserId;
        }
        public CustomUser? SelectUser(long? AUser_Ident)
        {
            string sql = @"SELECT id, username, user_ident, firstname, lastname, roles_id, userlist.user_query_tree_id 
                FROM userlist WHERE user_ident = @user_ident";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                return connection.QueryFirstOrDefault<CustomUser>(sql, new { user_ident = AUser_Ident });
            }
        }
        public bool HasRole(long Auser_id, long Arole_id)
        {
            string sql = @"select 1 
                from userroles ur 
                where ur.user_id = @user_id 
                    and ur.enabled = true
                    and ur.role_id = @role_id";
            DatabaseHelper dbHelper = new DatabaseHelper(ConnectionString);
            object parameters = new { user_id = Auser_id, role_id = Arole_id };
            return dbHelper.GetList<CustomRole>(sql, parameters).Count > 0;
        }

        public UserParam? ReadParam(long Auser_id, string AparamName)
        {
            string sql = @"select user_id,param_name,param_str,param_int  
                from userparams up 
                where up.user_id = @user_id 
                    and up.param_name = @param_name";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                return connection.QueryFirstOrDefault<UserParam>(sql, new { user_id = Auser_id, param_name = AparamName });
            }
        }

        public void WriteParam(long Auser_id, string AparamName, string Aparam_value)
        {
            string sql = @"INSERT INTO userparams ( user_id,param_name,param_str,param_int )
                VALUES (@user_id, @param_name, @param_value, null)
                ON CONFLICT (user_id,param_name)
                DO UPDATE SET 
                    param_str = EXCLUDED.param_str, 
                    param_int = EXCLUDED.param_int;";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                connection.QueryFirstOrDefault<UserParam>(sql, new { user_id = Auser_id, param_name = AparamName, param_value = Aparam_value });

            }
        }

        public void WriteParam(long Auser_id, string AparamName, int Aparam_value)
        {
            string sql = @"INSERT INTO userparams ( user_id,param_name,param_str,param_int )
                VALUES (@user_id, @param_name, null, @param_value)
                ON CONFLICT (user_id,param_name)
                DO UPDATE SET 
                    param_str = EXCLUDED.param_str, 
                    param_int = EXCLUDED.param_int;";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                connection.QueryFirstOrDefault<UserParam>(sql, new { user_id = Auser_id, param_name = AparamName, param_value = Aparam_value });

            }
        }

        public List<CustomUserTree> SelectUserTree(long AParent_Id)
        {
            string sql = @"select id,name,parent_id
                from userquerys_tree ut 
                where up.parent_id = @parent_id";

            DatabaseHelper dbHelper = new DatabaseHelper(ConnectionString);
            object parameters = new { parent_id = AParent_Id };
            return dbHelper.GetList<CustomUserTree>(sql, parameters);
        }


        /*User_Messages*/
        public void WriteMessageToDB(long Auser_id, long Achat_id, long Aanswerer_id, string AMessageStr)
        {
            string sql = @"INSERT INTO user_messages ( user_id,answerer_id,chat_id,message_str,datetime )
                VALUES (@user_id, @answerer_id, @chat_id, @message_str, datetime);";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var vParam = new { user_id = Auser_id, answerer_id = Aanswerer_id, chat_id = Achat_id, message_str = AMessageStr, datetime = DateTime.Now };
                connection.QueryFirstOrDefault<UserParam>(sql, vParam);

            }
        }
        #endregion UserInfo

        #region Roles
        public List<CustomRole> GetAllRoles()
        {
            string sql = @"select r.id, r.name from roles r";
            DatabaseHelper dbHelper = new DatabaseHelper(ConnectionString);
            return dbHelper.GetList<CustomRole>(sql);
        }

        public void SetUserRole(long? Auser_ident, long? Aroles_id)
        {
            string sql = @"UPDATE userlist set roles_id=@roles_id where user_ident = @user_ident;";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var vParam = new { user_id = Auser_ident, roles_id = Aroles_id };
                connection.QueryFirstOrDefault<UserParam>(sql, vParam);

            }
        }

        public void SetUserQueryId(long? Auser_ident, long? Aquery_id)
        {
            string sql = @"UPDATE userlist set user_query_tree_id=@query_id where user_ident = @user_ident;";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var vParam = new { user_ident = Auser_ident, query_id = Aquery_id };
                connection.QueryFirstOrDefault<UserParam>(sql, vParam);

            }
        }

        #endregion Roles

        #region UserQuery
        public List<UserQuerysTree> GetUserQuerysTree()
        {
            string sql = @"select q.id, q.name, q.parent_id from userquerys_tree q";
            DatabaseHelper dbHelper = new DatabaseHelper(ConnectionString);
            return dbHelper.GetList<UserQuerysTree>(sql);
        }

        public void AddMessage(long? Auser_id, string? AMessageStr)
        {
            string sql = @"insert into user_messages(user_id, message_str) values (@user_id, @message_str);";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var vParam = new { user_id = Auser_id, message_str = AMessageStr };
                connection.QueryFirstOrDefault<UserParam>(sql, vParam);

            }
        }
        #endregion UserQuery
    }

}
