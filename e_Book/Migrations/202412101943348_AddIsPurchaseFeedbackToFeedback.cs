namespace e_Book.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsPurchaseFeedbackToFeedback : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Feedbacks", "IsPurchaseFeedback", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Feedbacks", "IsPurchaseFeedback");
        }
    }
}
