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
            string sql = @"SELECT id, username, user_ident, firstname, lastname, roles_id, topic_id, sender_id
                FROM userlist WHERE user_ident = @user_ident";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                return connection.QueryFirstOrDefault<CustomUser>(sql, new { user_ident = AUser_Ident });
            }
        }

        public CustomUser? SelectUserById(long? AUser_Id)
        {
            string sql = @"SELECT id, username, user_ident, firstname, lastname, roles_id, topic_id, sender_id
                FROM userlist WHERE id = @user_id";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                return connection.QueryFirstOrDefault<CustomUser>(sql, new { user_id = AUser_Id });
            }
        }

        public List<CustomUser> SelectNewUsers()
        {
            string sql = @"SELECT id, username, user_ident, firstname, lastname, roles_id, topic_id, sender_id
                FROM userlist WHERE is_new = true";
            DatabaseHelper dbHelper = new DatabaseHelper(ConnectionString);
            return dbHelper.GetList<CustomUser>(sql);
        }

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
                var vParam = new { user_ident = Auser_ident, roles_id = Aroles_id };
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

        public List<CustomUserTopic> GetTopics()
        {
            string sql = @"select ut.id, ut.name, ut.parent_id from user_topics ut";
            DatabaseHelper dbHelper = new DatabaseHelper(ConnectionString);
            return dbHelper.GetList<CustomUserTopic>(sql);
        }

        public bool HasOpenedMessages(long? Auser_id)
        {
            string sql = @"select 1 from user_messages um where um.is_new = true and um.user_id = @user_id limit 1";
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var vParam = new { user_id = Auser_id };
                return connection.ExecuteScalar<int>(sql, vParam) == 1;
            }
        }

        public void AddMessage(long? Auser_id, string? AMessageStr, long? Atopic_id, long? AAnswerer_id)
        {
            string sql = @"insert into user_messages(user_id, message_str, topic_id, answerer_id) 
                values (@user_id, @message_str, @topic_id, @answerer_id); 
                SELECT 1;";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var vParam = new { user_id = Auser_id, message_str = AMessageStr, topic_id = Atopic_id, answerer_id = AAnswerer_id };
                connection.QuerySingle<int>(sql, vParam);

            }
        }

        public (long?, long?) GetOldestMessageUserId()
        {
            string sql = @"select um.user_id, um.topic_id from user_messages um where um.is_new = true and um.answerer_id is null order by um.datetime limit 1";
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                    if (reader.Read())
                    {
                        //var vParam = new { };
                        //return connection.ExecuteScalar < int; int> (sql, vParam);
                        return (reader.GetInt64(0), reader.GetInt64(0));
                    }
            }
            return (null, null);
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

        public void SetActiveSenderId(long? AUser_Id, long? ASender_id)
        {
            string sql = @"UPDATE userlist set sender_id = @sender_id where id = @user_id;
                SELECT 1;";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var vParam = new { user_id = AUser_Id, sender_id = ASender_id };
                connection.QuerySingle<int>(sql, vParam);

            }
        }
        public void CloseActiveMessages(long? ASender_id)
        {
            string sql = @"UPDATE user_messages set is_new = false where user_id = @user_id;
                SELECT 1;";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var vParam = new { user_id = ASender_id };
                connection.QuerySingle<int>(sql, vParam);

            }
        }

    }

}
