namespace e_Book.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBookAndUserToPurchase : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Purchases", "UserId");
            CreateIndex("dbo.Purchases", "BookId");
            AddForeignKey("dbo.Purchases", "BookId", "dbo.Books", "BookId", cascadeDelete: true);
            AddForeignKey("dbo.Purchases", "UserId", "dbo.Users", "UserId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Purchases", "UserId", "dbo.Users");
            DropForeignKey("dbo.Purchases", "BookId", "dbo.Books");
            DropIndex("dbo.Purchases", new[] { "BookId" });
            DropIndex("dbo.Purchases", new[] { "UserId" });
        }
    }
}
