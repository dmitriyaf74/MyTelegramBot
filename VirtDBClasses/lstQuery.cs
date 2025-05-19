using MyTelegramBot.Classes;
using MyTelegramBot.DapperClasses;
using MyTelegramBot.Interfaces;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTelegramBot.VirtDBClasses
{
    internal class lstQuery : ICustomQuery
    {        
        public lstQuery(string AconnectionString)
        {            
        }

        #region UserInfo
        public List<CustomRole> SelectRoles(long? Auser_id)
        {
            return new();
        }

        public long InsertUser(CustomUser Auser)
        {
            return 0;
        }
        public CustomUser? SelectUserByIdent(long? AUser_Ident)
        {
            return new();
        }

        public CustomUser? SelectUserById(long? AUser_Id)
        {
            return new();
        }

        public List<CustomUser> SelectNewUsers()
        {
            return new();
        }

        #endregion UserInfo

        #region Roles
        public List<CustomRole> GetAllRoles()
        {
            return new();
        }

        public void SetUserRole(long? Auser_ident, RolesEnum? Aroles_id)
        {
            
        }

        public void SetUserQueryId(long? Auser_ident, long? Atopic_id)
        {
           
        }

        #endregion Roles

        #region UserQuery
        public List<CustomUserTopic> GetTopics()
        {
            return new();
        }

        public void AddMessage(long? Auser_id, string? AMessageStr, long? Atopic_id)
        {
            
        }
        #endregion UserQuery
        public long GetOldestMessageUserId()
        {
            return 0;
        }

        public List<CustomUserMessage> GetUserMessages(long? AUserId)
        {
            return new();
        }
    }
}
