namespace e_Book.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCreditCardFieldsToUsers : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "CreditCardNumber", c => c.String(nullable: false));
            AddColumn("dbo.Users", "ExpiryDate", c => c.String(nullable: false));
            AddColumn("dbo.Users", "CVC", c => c.String(nullable: false));
            AlterColumn("dbo.Users", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.Users", "Email", c => c.String(nullable: false));
            AlterColumn("dbo.Users", "Password", c => c.String(nullable: false));
            AlterColumn("dbo.Users", "Role", c => c.String(nullable: false));
            DropColumn("dbo.Users", "IdentityCard");
            DropTable("dbo.CreditCardInfoes");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.CreditCardInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(nullable: false),
                        LastName = c.String(nullable: false),
                        PersonalId = c.String(nullable: false),
                        CardNumber = c.String(nullable: false),
                        ExpiryDate = c.String(nullable: false),
                        CVC = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
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
