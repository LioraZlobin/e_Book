namespace e_Book.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPositionToWaitingList : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WaitingLists", "Position", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.WaitingLists", "Position");
        }
    }
}
