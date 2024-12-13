namespace e_Book.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAgeRestrictionToBooks : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Books", "AgeRestriction", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Books", "AgeRestriction");
        }
    }
}
