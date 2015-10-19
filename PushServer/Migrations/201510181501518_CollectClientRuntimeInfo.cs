namespace PushServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CollectClientRuntimeInfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SignalRConnections", "ConnectAt", c => c.DateTime(nullable: false));
            AddColumn("dbo.SignalRConnections", "ReconnectAt", c => c.DateTime());
            AddColumn("dbo.SignalRConnections", "DisconnectAt", c => c.DateTime());
            AddColumn("dbo.SignalRConnections", "OSVersion", c => c.String());
            AddColumn("dbo.SignalRConnections", "IEVersion", c => c.String());
            AddColumn("dbo.SignalRConnections", "RuntimeVersion", c => c.String());
            AddColumn("dbo.SignalRConnections", "ReconnectCount", c => c.Int(nullable: false));
            DropColumn("dbo.SignalRConnections", "LastConnectDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SignalRConnections", "LastConnectDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.SignalRConnections", "ReconnectCount");
            DropColumn("dbo.SignalRConnections", "RuntimeVersion");
            DropColumn("dbo.SignalRConnections", "IEVersion");
            DropColumn("dbo.SignalRConnections", "OSVersion");
            DropColumn("dbo.SignalRConnections", "DisconnectAt");
            DropColumn("dbo.SignalRConnections", "ReconnectAt");
            DropColumn("dbo.SignalRConnections", "ConnectAt");
        }
    }
}
