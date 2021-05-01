using Microsoft.EntityFrameworkCore.Migrations;

namespace KnowledgeSpace.BackendServer.Models.Migrations
{
    public partial class Add_Status_Into_Votes_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "Votes",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Votes");
        }
    }
}
