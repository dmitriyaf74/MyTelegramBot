namespace MyTelegramBot.Classes
{
    public enum RolesEnum
    {
        reUnknown,
        reUser,   
        reAdmin,  
        reOperator, 
    }

    internal class CustomRole
    {
        public long Id { get; set; }
        public string? Name { get; set; }

    }

    internal class CustomUserMessage
    {
        public long User_Id { get; set; }
        public string? MessageStr { get; set; }
        public DateTime Date_Time { get; set; }
        public bool Delivered { get; set; }
        public long Answerer_Id { get; set; }
        public long Topic_Id { get; set; }
        public bool IsNew { get; set; }
    }

    internal class CustomUser
    {
        public long? Id { get; set; }
        public string? UserName { get; set; }
        public long User_Ident { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public RolesEnum Roles_id { get; set; }
        public long Topic_id {  get; set; }

    }
    
    internal class CustomUserTopic
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public long? Parent_Id { get; set; }        

    }

    internal class CustomUserRole
    {
        public long User_Id { get; set; }
        public long Role_Id { get; set; }
        public bool Enabled { get; set; }

    }

}
