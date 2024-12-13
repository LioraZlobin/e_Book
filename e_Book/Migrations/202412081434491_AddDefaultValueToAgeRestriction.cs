namespace e_Book.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDefaultValueToAgeRestriction : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Books", "AgeRestriction", c => c.String(defaultValue: "All Ages"));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Books", "AgeRestriction", c => c.String());
        }
    }
}
