namespace e_Book.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class YourMigrationName : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Users", "CreditCardNumber", c => c.String());
            AlterColumn("dbo.Users", "ExpiryDate", c => c.String());
            AlterColumn("dbo.Users", "CVC", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Users", "CVC", c => c.String(nullable: false));
            AlterColumn("dbo.Users", "ExpiryDate", c => c.String(nullable: false));
            AlterColumn("dbo.Users", "CreditCardNumber", c => c.String(nullable: false));
        }
    }
}
