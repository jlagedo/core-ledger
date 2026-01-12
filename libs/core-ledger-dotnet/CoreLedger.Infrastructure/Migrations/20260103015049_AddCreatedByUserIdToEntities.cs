using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreLedger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByUserIdToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add columns as nullable
            migrationBuilder.AddColumn<string>(
                name: "created_by_user_id",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_user_id",
                table: "transactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_user_id",
                table: "todos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_user_id",
                table: "securities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_user_id",
                table: "funds",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_user_id",
                table: "accounts",
                type: "text",
                nullable: true);

            // Step 2: Update all existing rows with default value
            migrationBuilder.Sql("UPDATE users SET created_by_user_id = 'mock|admin-001' WHERE created_by_user_id IS NULL;");
            migrationBuilder.Sql("UPDATE transactions SET created_by_user_id = 'mock|admin-001' WHERE created_by_user_id IS NULL;");
            migrationBuilder.Sql("UPDATE todos SET created_by_user_id = 'mock|admin-001' WHERE created_by_user_id IS NULL;");
            migrationBuilder.Sql("UPDATE securities SET created_by_user_id = 'mock|admin-001' WHERE created_by_user_id IS NULL;");
            migrationBuilder.Sql("UPDATE funds SET created_by_user_id = 'mock|admin-001' WHERE created_by_user_id IS NULL;");
            migrationBuilder.Sql("UPDATE accounts SET created_by_user_id = 'mock|admin-001' WHERE created_by_user_id IS NULL;");

            // Step 3: Alter columns to NOT NULL
            migrationBuilder.AlterColumn<string>(
                name: "created_by_user_id",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "created_by_user_id",
                table: "transactions",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "created_by_user_id",
                table: "todos",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "created_by_user_id",
                table: "securities",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "created_by_user_id",
                table: "funds",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "created_by_user_id",
                table: "accounts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "todos");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "securities");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "funds");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "accounts");
        }
    }
}
