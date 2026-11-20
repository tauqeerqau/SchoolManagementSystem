using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.Infrastructure.Persistence.Migrations;

public partial class AddSchoolClassStudentRelationship : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Classes",
            columns: table => new
            {
                ClassId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Classes", x => x.ClassId);
            });

        migrationBuilder.AddColumn<int>(
            name: "ClassId",
            table: "Students",
            type: "int",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Students_ClassId",
            table: "Students",
            column: "ClassId");

        migrationBuilder.AddForeignKey(
            name: "FK_Students_Classes_ClassId",
            table: "Students",
            column: "ClassId",
            principalTable: "Classes",
            principalColumn: "ClassId",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Students_Classes_ClassId",
            table: "Students");

        migrationBuilder.DropIndex(
            name: "IX_Students_ClassId",
            table: "Students");

        migrationBuilder.DropColumn(
            name: "ClassId",
            table: "Students");

        migrationBuilder.DropTable(
            name: "Classes");
    }
}
