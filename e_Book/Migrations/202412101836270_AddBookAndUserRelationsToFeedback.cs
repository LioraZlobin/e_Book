namespace e_Book.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBookAndUserRelationsToFeedback : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Feedbacks", "UserId");
            CreateIndex("dbo.Feedbacks", "BookId");
            AddForeignKey("dbo.Feedbacks", "BookId", "dbo.Books", "BookId");
            AddForeignKey("dbo.Feedbacks", "UserId", "dbo.Users", "UserId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Feedbacks", "UserId", "dbo.Users");
            DropForeignKey("dbo.Feedbacks", "BookId", "dbo.Books");
            DropIndex("dbo.Feedbacks", new[] { "BookId" });
            DropIndex("dbo.Feedbacks", new[] { "UserId" });
        }
    }
}
