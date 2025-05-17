namespace MyTelegramBot.DapperClasses
{
    abstract class CustomQuery
    {
        /*public abstract long InsertUser(CustomUser Auser);
        public abstract CustomUser? SelectUser(long? AUser_Ident);
        public abstract List<CustomRole> SelectRoles(long? Auser_id);
        public abstract bool HasRole(long Auser_id, long Arole_id);
        public abstract UserParam? ReadParam(long Auser_id, string AparamName);
        public abstract void WriteParam(long Auser_id, string AparamName, string Aparam_value);
        public abstract void WriteParam(long Auser_id, string AparamName, int Aparam_value);
        public abstract List<CustomUserTree> SelectUserTree(long AParent_Id);

        public abstract void WriteMessageToDB(long Auser_id, long Aanswerer_id, long Achat_id, string AMessageStr);*/
    }
    internal class CustomUser
    {
        public long Id { get; set; }
        public string? UserName { get; set; }
        public long User_Ident { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public long Roles_id { get; set; }
        public string? UserQueryTreeId { get; set; }

    }

    internal class CustomRole
    {
        public long Id { get; set; }
        public string? Name { get; set; }

    }
    internal class CustomUserTree
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public long Parent_Id { get; set; }

    }
    internal class UserParam
    {
        public long User_Id { get; set; }
        public string? Param_Name { get; set; }
        public string? Param_Str { get; set; }
        public long Param_Int { get; set; }

    }
    internal class UserMessages
    {
        public long User_Id { get; set; }
        public string? MessageStr { get; set; }
        public DateTime Date_Time { get; set; }
        public bool Delivered { get; set; }
    }
    internal class UserQuerysTree
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public long? Parent_Id { get; set; }        

    }

}
