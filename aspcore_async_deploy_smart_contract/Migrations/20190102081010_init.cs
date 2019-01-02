using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace aspcore_async_deploy_smart_contract.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TaskId = table.Column<int>(nullable: false),
                    OrganizationId = table.Column<string>(nullable: true),
                    ContractAddress = table.Column<string>(nullable: true),
                    DeployStart = table.Column<DateTime>(nullable: false),
                    DeployDone = table.Column<DateTime>(nullable: false),
                    QuerryDone = table.Column<DateTime>(nullable: false),
                    Status = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    Messasge = table.Column<string>(nullable: true),
                    TransactionId = table.Column<string>(nullable: true),
                    Hash = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Certificates");
        }
    }
}
