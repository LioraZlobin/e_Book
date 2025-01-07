namespace e_Book.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExpirationTimeToWaitingList : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WaitingLists", "ExpirationTime", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.WaitingLists", "ExpirationTime");
        }
    }
}
