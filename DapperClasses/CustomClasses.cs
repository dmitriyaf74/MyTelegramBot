namespace MyTelegramBot.DapperClasses
{
    abstract class CustomQuery
    {
        public abstract long InsertUser(string AconnectionString, CustomUser Auser);
        public abstract CustomUser SelectUser(string AconnectionString, long AUser_Ident);
        public abstract List<CustomRole> SelectRoles(string AconnectionString, long Auser_id);
    }
    internal class CustomUser
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public long User_Ident { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long Roles_id { get; set; }

    }

    internal class CustomRole
    {
        public long Id { get; set; }
        public string Name { get; set; }

    }
}
