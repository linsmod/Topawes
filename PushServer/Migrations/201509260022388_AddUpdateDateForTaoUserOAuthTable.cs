namespace PushServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUpdateDateForTaoUserOAuthTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserTaoOAuths", "UpdateAt", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserTaoOAuths", "UpdateAt");
        }
    }
}
