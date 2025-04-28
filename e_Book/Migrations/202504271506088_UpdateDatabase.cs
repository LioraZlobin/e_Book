namespace e_Book.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDatabase : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "CreditCardNumber", c => c.String());
            AddColumn("dbo.Users", "ExpiryDate", c => c.String());
            AddColumn("dbo.Users", "CVC", c => c.String());
            AlterColumn("dbo.Users", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.Users", "Email", c => c.String(nullable: false));
            AlterColumn("dbo.Users", "Password", c => c.String(nullable: false));
            AlterColumn("dbo.Users", "Role", c => c.String(nullable: false));
            DropColumn("dbo.Users", "IdentityCard");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "IdentityCard", c => c.String());
            AlterColumn("dbo.Users", "Role", c => c.String());
            AlterColumn("dbo.Users", "Password", c => c.String());
            AlterColumn("dbo.Users", "Email", c => c.String());
            AlterColumn("dbo.Users", "Name", c => c.String());
            DropColumn("dbo.Users", "CVC");
            DropColumn("dbo.Users", "ExpiryDate");
            DropColumn("dbo.Users", "CreditCardNumber");
        }
    }
}
