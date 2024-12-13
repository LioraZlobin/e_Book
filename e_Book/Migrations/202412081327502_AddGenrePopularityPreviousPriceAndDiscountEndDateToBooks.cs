namespace e_Book.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGenrePopularityPreviousPriceAndDiscountEndDateToBooks : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Books", "Genre", c => c.String());
            AddColumn("dbo.Books", "Popularity", c => c.Int(nullable: false));
            AddColumn("dbo.Books", "PreviousPrice", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Books", "DiscountEndDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Books", "DiscountEndDate");
            DropColumn("dbo.Books", "PreviousPrice");
            DropColumn("dbo.Books", "Popularity");
            DropColumn("dbo.Books", "Genre");
        }
    }
}
