using Dapper;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using MyTelegramBot.Interfaces;
using MyTelegramBot.Classes;

namespace MyTelegramBot.DapperClasses
{

    internal class pgQuery : ICustomQuery
    {
        private string ConnectionString;
        public pgQuery(string AconnectionString)
        {
            ConnectionString = AconnectionString;
        }

        #region UserInfo
        public List<CustomRole> SelectRoles(long? Auser_id)
        {
            string sql = @"select r.id, r.name 
                from userroles ur 
                join roles r on r.id = ur.role_id 
                where ur.user_id = @user_id and ur.enabled = true
                order by 1";
            DatabaseHelper dbHelper = new DatabaseHelper(ConnectionString);
            object parameters = new { user_id = Auser_id };
            return dbHelper.GetList<CustomRole>(sql, parameters);
        }
                        
        public long InsertUser(CustomUser Auser)
        {
            string sql = @"INSERT INTO userlist(username, user_ident, firstname, lastname, roles_id, topic_id)
                VALUES (@username, @user_ident, @firstname, @lastname, @roles_id, @topic_id) RETURNING id;";

            long vUserId;
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                vUserId = connection.QueryFirstOrDefaultAsync<int>(sql, Auser).Result;
                Auser.Id = vUserId;
            }
            return vUserId;
        }
        public CustomUser? SelectUserByIdent(long? AUser_Ident)
        {
            string sql = @"SELECT id, username, user_ident, firstname, lastname, roles_id, topic_id
                FROM userlist WHERE user_ident = @user_ident";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                return connection.QueryFirstOrDefault<CustomUser>(sql, new { user_ident = AUser_Ident });
            }
        }

        public CustomUser? SelectUserById(long? AUser_Id)
        {
            string sql = @"SELECT id, username, user_ident, firstname, lastname, roles_id, topic_id
                FROM userlist WHERE id = @user_id";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                return connection.QueryFirstOrDefault<CustomUser>(sql, new { user_id = AUser_Id });
            }
        }

        public List<CustomUser> SelectNewUsers()
        {
            string sql = @"SELECT id, username, user_ident, firstname, lastname, roles_id, topic_id
                FROM userlist WHERE is_new = true";
            DatabaseHelper dbHelper = new DatabaseHelper(ConnectionString);
            return dbHelper.GetList<CustomUser>(sql);
        }
                
        #endregion UserInfo

        #region Roles
        public List<CustomRole> GetAllRoles()
        {
            string sql = @"select r.id, r.name from roles r";
            DatabaseHelper dbHelper = new DatabaseHelper(ConnectionString);
            return dbHelper.GetList<CustomRole>(sql);
        }

        public void SetUserRole(long? Auser_ident, RolesEnum? Aroles_id)
        {
            string sql = @"UPDATE userlist set roles_id=@roles_id where user_ident = @user_ident;
                SELECT 1;";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var vParam = new { user_id = Auser_ident, roles_id = Aroles_id };
                connection.QuerySingle<int>(sql, vParam);

            }
        }

        public void SetUserTopicId(long? Auser_ident, long? Atopic_id)
        {
            string sql = @"UPDATE userlist set topic_id = @topic_id where user_ident = @user_ident;
                SELECT 1;";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var vParam = new { user_ident = Auser_ident, topic_id = Atopic_id };
                connection.QuerySingle<int>(sql, vParam);

            }
        }

        #endregion Roles

        #region UserQuery
        public List<CustomUserTopic> GetTopics()
        {
            string sql = @"select ut.id, ut.name, ut.parent_id from user_topics ut";
            DatabaseHelper dbHelper = new DatabaseHelper(ConnectionString);
            return dbHelper.GetList<CustomUserTopic>(sql);
        }

        public void AddMessage(long? Auser_id, string? AMessageStr, long? Atopic_id)
        {
            string sql = @"insert into user_messages(user_id, message_str, topic_id) 
                values (@user_id, @message_str, @topic_id); 
                SELECT 1;";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var vParam = new { user_id = Auser_id, message_str = AMessageStr, topic_id = Atopic_id };
                connection.QuerySingle<int>(sql, vParam);

            }
        }
        #endregion UserQuery
        public long GetOldestMessageUserId()
        {
            string sql = @"select um.user_id from user_messages um where um.is_new = true order by um.datetime limit 1";
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var vParam = new {};
                return connection.ExecuteScalar<int>(sql, vParam);
            }
        }

        public List<CustomUserMessage> GetUserMessages(long? AUserId)
        {
            string sql = @"select um.user_id User_Id, um.message_str MessageStr, um.datetime Date_Time, 
                um.delivered Delivered, um.answerer_id Answerer_Id, um.topic_id Topic_Id, um.is_new IsNew
                from user_messages um 
                where um.user_id = @user_id 
                order by um.datetime";
            DatabaseHelper dbHelper = new DatabaseHelper(ConnectionString);
            var vParam = new { user_id = AUserId };
            return dbHelper.GetList<CustomUserMessage>(sql, vParam);
        }

    }

}
