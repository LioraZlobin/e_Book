namespace e_Book.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Books",
                c => new
                    {
                        BookId = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Author = c.String(),
                        Publisher = c.String(),
                        PriceBuy = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PriceBorrow = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AvailableCopies = c.Int(nullable: false),
                        Format = c.String(),
                        YearPublished = c.Int(nullable: false),
                        IsBorrowable = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.BookId);
            
            CreateTable(
                "dbo.Borrows",
                c => new
                    {
                        BorrowId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        BookId = c.Int(nullable: false),
                        BorrowDate = c.DateTime(nullable: false),
                        DueDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.BorrowId);
            
            CreateTable(
                "dbo.Feedbacks",
                c => new
                    {
                        FeedbackId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        BookId = c.Int(),
                        Content = c.String(),
                        Rating = c.Int(nullable: false),
                        FeedbackDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.FeedbackId);
            
            CreateTable(
                "dbo.Purchases",
                c => new
                    {
                        PurchaseId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        BookId = c.Int(nullable: false),
                        PurchasePrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PurchaseDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.PurchaseId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Email = c.String(),
                        Password = c.String(),
                        Role = c.String(),
                    })
                .PrimaryKey(t => t.UserId);
            
            CreateTable(
                "dbo.WaitingLists",
                c => new
                    {
                        WaitingListId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        BookId = c.Int(nullable: false),
                        AddedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.WaitingListId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.WaitingLists");
            DropTable("dbo.Users");
            DropTable("dbo.Purchases");
            DropTable("dbo.Feedbacks");
            DropTable("dbo.Borrows");
            DropTable("dbo.Books");
        }
    }
}
