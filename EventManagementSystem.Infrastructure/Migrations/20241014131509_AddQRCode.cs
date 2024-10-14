using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQRCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditCardDetails_AspNetUsers_UserId",
                table: "CreditCardDetails");

            migrationBuilder.DropIndex(
                name: "IX_CreditCardDetails_UserId",
                table: "CreditCardDetails");

            migrationBuilder.DropColumn(
                name: "CardHolderName",
                table: "CreditCardDetails");

            migrationBuilder.RenameColumn(
                name: "CVV",
                table: "CreditCardDetails",
                newName: "PaymentMethodId");

            migrationBuilder.AddColumn<string>(
                name: "QRCodeSvg",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "CreditCardDetails",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ExpirationYear",
                table: "CreditCardDetails",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ExpirationMonth",
                table: "CreditCardDetails",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CardNumber",
                table: "CreditCardDetails",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(16)",
                oldMaxLength: 16);

            migrationBuilder.AddColumn<string>(
                name: "CardBrand",
                table: "CreditCardDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QRCodeSvg",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CardBrand",
                table: "CreditCardDetails");

            migrationBuilder.RenameColumn(
                name: "PaymentMethodId",
                table: "CreditCardDetails",
                newName: "CVV");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "CreditCardDetails",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "ExpirationYear",
                table: "CreditCardDetails",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "ExpirationMonth",
                table: "CreditCardDetails",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CardNumber",
                table: "CreditCardDetails",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "CardHolderName",
                table: "CreditCardDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CreditCardDetails_UserId",
                table: "CreditCardDetails",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditCardDetails_AspNetUsers_UserId",
                table: "CreditCardDetails",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
