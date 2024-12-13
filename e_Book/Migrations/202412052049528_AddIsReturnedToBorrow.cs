namespace e_Book.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsReturnedToBorrow : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Borrows", "IsReturned", c => c.Boolean(nullable: false));
            CreateIndex("dbo.Borrows", "UserId");
            CreateIndex("dbo.Borrows", "BookId");
            AddForeignKey("dbo.Borrows", "BookId", "dbo.Books", "BookId", cascadeDelete: true);
            AddForeignKey("dbo.Borrows", "UserId", "dbo.Users", "UserId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Borrows", "UserId", "dbo.Users");
            DropForeignKey("dbo.Borrows", "BookId", "dbo.Books");
            DropIndex("dbo.Borrows", new[] { "BookId" });
            DropIndex("dbo.Borrows", new[] { "UserId" });
            DropColumn("dbo.Borrows", "IsReturned");
        }
    }
}
