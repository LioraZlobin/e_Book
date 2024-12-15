namespace e_Book.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsReminderSentToBorrows : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Borrows", "IsReminderSent", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Borrows", "IsReminderSent");
        }
    }
}
