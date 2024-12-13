namespace e_Book.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIdentityCardToUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "IdentityCard", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "IdentityCard");
        }
    }
}
