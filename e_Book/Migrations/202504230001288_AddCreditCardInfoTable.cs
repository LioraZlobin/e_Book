namespace e_Book.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCreditCardInfoTable : DbMigration
    {
        public override void Up()
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
            
        }
        
        public override void Down()
        {
            DropTable("dbo.CreditCardInfoes");
        }
    }
}
