using Dapper;
using MyTelegramBot.Classes;
using MyTelegramBot.DapperClasses;
using MyTelegramBot.Interfaces;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace MyTelegramBot.VirtDBClasses
{
    internal class lstQuery : ICustomQuery
    {        
        public lstQuery(string AconnectionString)
        {            
        }

        public List<CustomRole> SelectRoles(long? Auser_id)
        {
            var filteredUserRoles = VirtDB.tUserRoles.
                Where(x => x.User_Id == Auser_id).ToList();

            var query = (from сustomRole in VirtDB.tRoles
                        join сustomUserRole in filteredUserRoles on сustomRole.Id equals сustomUserRole.Role_Id
                        select сustomRole).ToList();

            return query;
        }

        public long InsertUser(CustomUser Auser)
        {
            Auser.Id = (VirtDB.tUsers.Max(i => i.Id) ?? 0)+1;
            VirtDB.tUsers.Add(Auser);
            return Auser.Id ?? 0;
        }
        public CustomUser? SelectUserByIdent(long? AUser_Ident)
        {
            return VirtDB.tUsers.
                Where(x => x.User_Ident == AUser_Ident).FirstOrDefault();
        }

        public CustomUser? SelectUserById(long? AUser_Id)
        {
            return VirtDB.tUsers.
                Where(x => x.Id == AUser_Id).FirstOrDefault();
        }

        public List<CustomUser> SelectNewUsers()
        {
            return VirtDB.tUsers.
                Where(x => x.Is_New == true).ToList();
        }

        public List<CustomRole> GetAllRoles()
        {
            return VirtDB.tRoles;
        }

        public void SetUserRole(long? Auser_ident, RolesEnum? Aroles_id)
        {
            var vUser = SelectUserByIdent(Auser_ident);
            if ((vUser != null) && (Aroles_id != null))
            {
                vUser.Roles_id = Aroles_id; 
            }
        }

        public void SetUserTopicId(long? Auser_ident, long? Atopic_id)
        {
            var vUser = SelectUserByIdent(Auser_ident);
            if ((vUser != null) && (Atopic_id != null))
            {
                vUser.Topic_id = Atopic_id; 
            }
        }

        public List<CustomUserTopic> GetTopics()
        {
            return VirtDB.tUserTopics;
        }
        public bool HasOpenedMessages(long? Auser_id)
        {
            return VirtDB.tUserMessages.Where(x => x.IsNew == true && x.User_Id == Auser_id).Any();
        }

        public void AddMessage(long? Auser_id, string? AMessageStr, long? Atopic_id, long? AAnswerer_id)
        {
            CustomUserMessage mes = new() 
            { 
                User_Id = Auser_id ?? 0, 
                MessageStr = AMessageStr, 
                Topic_Id = Atopic_id, 
                Date_Time = DateTime.Now,
                Delivered = false,
                IsNew = true,
                Answerer_Id = AAnswerer_id,
            };
            VirtDB.tUserMessages.Add(mes);
        }

        public (long?, long?) GetOldestMessageUserId()
        {
            return VirtDB.tUserMessages.
                Where(x => x.IsNew == true).
                OrderBy(x => x.Date_Time).
                Select(x => (x.User_Id,x.Topic_Id)).FirstOrDefault();
        }

        public List<CustomUserMessage> GetUserMessages(long? AUserId)
        {
            return VirtDB.tUserMessages.
                Where(x => x.User_Id == AUserId).ToList();
        }

        public void SetActiveSenderId(long? AUser_Id, long? ASender_Id)
        {
            var vUser = SelectUserById(AUser_Id);
            if (vUser is not null)
                vUser.Sender_Id = ASender_Id; 
        }

        public void CloseActiveMessages(long? ASender_id)
        {
            var vMes = GetUserMessages(ASender_id);
            foreach (var item in vMes)
                item.IsNew = false;
        }
        public void DelayChat(long? AUser_id, long? AChat_id)
        {
            DelayedChats? item = VirtDB.tDelayedChats.
                Where(x => x.Chat_Id == AChat_id).FirstOrDefault();
            if (item == null)
            {
                item = new DelayedChats();
                VirtDB.tDelayedChats.Add(item);
            }
            item.User_Id = AUser_id ?? 0;
            item.Chat_Id = AChat_id ?? 0;
            item.Enabled = true;
        }
        public void ResumeChat(long? AChat_id)
        {
            DelayedChats? t = VirtDB.tDelayedChats.
                Where(x => x.Chat_Id == AChat_id).FirstOrDefault();
            if (t != null)
                t.Enabled = false;
        }
        public List<DelayedChats> GetDelayedChats(long? AUser_id)
        {
            return VirtDB.tDelayedChats;
        }
        public long? GetAnswererIdent(long? AUser_id)
        {
            return VirtDB.tUsers.
                Where(x => x.Sender_Id == AUser_id).
                Select(x => x.User_Ident).FirstOrDefault();
        }

        public void AddRole(long? AUser_id, RolesEnum? ARole_id)
        {
            CustomUserRole? vRole = VirtDB.tUserRoles.Where(x => x.User_Id == AUser_id && x.Role_Id == ARole_id).FirstOrDefault();
            if (vRole == null)
            {
                vRole = new CustomUserRole();
                VirtDB.tUserRoles.Add(vRole);
            }
            vRole.User_Id = AUser_id ?? 0;
            vRole.Role_Id = ARole_id ?? 0;
            vRole.Enabled = true;
        }

        public void DropRole(long? AUser_id, RolesEnum? ARole_id)
        {
            CustomUserRole? vRole = VirtDB.tUserRoles.Where(x => x.User_Id == AUser_id && x.Role_Id == ARole_id).FirstOrDefault();
            if (vRole != null)
                vRole.Enabled = false;
        }

        public void HideFromNewUser(long? AUser_id)
        {
            var vUser = SelectUserById(AUser_id);
            if (vUser != null)
                vUser.Is_New = false;
        }
    }
}
