using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyTelegramBot.Classes;

namespace MyTelegramBot.VirtDBClasses
{
    internal class VirtDB
    {
        public static void FillDB() 
        {
            FillRoles();
            FillUserMessages();
            FillUsers();
            FillUserTopics();
            FillUserRoles();
        }

        public static List<CustomRole> tRoles { get; } = new(); 
        public static List<CustomUserMessage> tUserMessages { get; } = new();
        public static List<CustomUser> tUsers { get; } = new();
        public static List<CustomUserTopic> tUserTopics { get; } = new();
        public static List<CustomUserRole> tUserRoles { get; } = new();
        public static List<DelayedChats> tDelayedChats { get; } = new();

        private static void FillRoles()
        {
            tRoles.Clear();
            tRoles.Add(new CustomRole() { Id = RolesEnum.reUser, Name = "Пользователь" });
            tRoles.Add(new CustomRole() { Id = RolesEnum.reAdmin, Name = "Администратор" });
            tRoles.Add(new CustomRole() { Id = RolesEnum.reOperator, Name = "Оператор" });
        }
        private static void FillUserMessages()
        {
            tUserMessages.Clear();
        }
        private static void FillUsers()
        {
            tUsers.Clear();
            tUsers.Add(new CustomUser() { Id = 1, UserName = "Dmitry", Roles_id = RolesEnum.reOperator, User_Ident = 311068358, Is_New = true });
        }
        private static void FillUserTopics()
        {
            tUserTopics.Clear();
            tUserTopics.Add(new CustomUserTopic() { Id = 0, Name = "Общий раздел", Parent_Id = null });
            tUserTopics.Add(new CustomUserTopic() { Id = 1, Name = "Десктопное ПО", Parent_Id = 0 });
            tUserTopics.Add(new CustomUserTopic() { Id = 2, Name = "ВЭБ-интерфейс", Parent_Id = 0 });
            tUserTopics.Add(new CustomUserTopic() { Id = 3, Name = "Сетевое администрирование", Parent_Id = 0 });
            tUserTopics.Add(new CustomUserTopic() { Id = 4, Name = "Ремонт компьютеров", Parent_Id = 0 });
            tUserTopics.Add(new CustomUserTopic() { Id = 5, Name = "Ремонт принтеров", Parent_Id = 0 });
            tUserTopics.Add(new CustomUserTopic() { Id = 6, Name = "MS Office", Parent_Id = 1 });
            tUserTopics.Add(new CustomUserTopic() { Id = 7, Name = "1с", Parent_Id = 1 });
            tUserTopics.Add(new CustomUserTopic() { Id = 8, Name = "Интернет-банк", Parent_Id = 1 });
            tUserTopics.Add(new CustomUserTopic() { Id = 9, Name = "Сайт компании", Parent_Id = 2 });
            tUserTopics.Add(new CustomUserTopic() { Id = 10, Name = "Хранилище файлов", Parent_Id = 2 });
            tUserTopics.Add(new CustomUserTopic() { Id = 11, Name = "Обмен данными", Parent_Id = 2 });
            tUserTopics.Add(new CustomUserTopic() { Id = 12, Name = "Настройка сети", Parent_Id = 3 });
            tUserTopics.Add(new CustomUserTopic() { Id = 13, Name = "Настройка почты", Parent_Id = 3 });
            tUserTopics.Add(new CustomUserTopic() { Id = 14, Name = "Настройка интернета", Parent_Id = 3 });
            tUserTopics.Add(new CustomUserTopic() { Id = 15, Name = "Монитор", Parent_Id = 4 });
            tUserTopics.Add(new CustomUserTopic() { Id = 16, Name = "Системный блок", Parent_Id = 4 });
            tUserTopics.Add(new CustomUserTopic() { Id = 17, Name = "Периферия", Parent_Id = 4 });
            tUserTopics.Add(new CustomUserTopic() { Id = 18, Name = "Замена картриджа", Parent_Id = 5 });
            tUserTopics.Add(new CustomUserTopic() { Id = 19, Name = "Загрузка бумаги", Parent_Id = 5 });
            tUserTopics.Add(new CustomUserTopic() { Id = 20, Name = "Проблемы с печатью", Parent_Id = 5 });
        }
        private static void FillUserRoles()
        {
            tUserRoles.Clear();
            tUserRoles.Add(new CustomUserRole() { User_Id = 1, Role_Id = RolesEnum.reUser, Enabled = true });
            tUserRoles.Add(new CustomUserRole() { User_Id = 1, Role_Id = RolesEnum.reAdmin, Enabled = true });
            tUserRoles.Add(new CustomUserRole() { User_Id = 1, Role_Id = RolesEnum.reOperator, Enabled = true });
        }
        
    }
}
