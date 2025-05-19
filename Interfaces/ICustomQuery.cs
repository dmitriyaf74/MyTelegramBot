using MyTelegramBot.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTelegramBot.Interfaces
{
    internal interface ICustomQuery
    {
        public List<CustomRole> SelectRoles(long? Auser_id);
        public long InsertUser(CustomUser Auser);
        public CustomUser? SelectUserByIdent(long? AUser_Ident);
        public CustomUser? SelectUserById(long? AUser_Id);
        public List<CustomUser> SelectNewUsers();
        public List<CustomRole> GetAllRoles();
        public void SetUserRole(long? Auser_ident, RolesEnum? Aroles_id);
        public void SetUserQueryId(long? Auser_ident, long? Atopic_id);
        public List<CustomUserTopic> GetTopics();
        public void AddMessage(long? Auser_id, string? AMessageStr, long? Atopic_id);
        public long GetOldestMessageUserId();
        public List<CustomUserMessage> GetUserMessages(long? AUserId);
    }
}
