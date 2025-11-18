using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtemisBanking.Infraestructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationTypeAndOperatedByToTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LoanPaymentSchedules_LoanId",
                table: "LoanPaymentSchedules");

            migrationBuilder.AddColumn<string>(
                name: "OperatedByUserId",
                table: "Transactions",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OperationType",
                table: "Transactions",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EstadoPago",
                table: "Loans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "CreditCards",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "EsAvanceEfectivo",
                table: "CreditCardConsumptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Beneficiaries",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_OperatedByUserId_FechaTransaccion",
                table: "Transactions",
                columns: new[] { "OperatedByUserId", "FechaTransaccion" });

            migrationBuilder.CreateIndex(
                name: "IX_SavingsAccounts_NumeroCuenta",
                table: "SavingsAccounts",
                column: "NumeroCuenta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoanPaymentSchedules_LoanId_NumeroCuota",
                table: "LoanPaymentSchedules",
                columns: new[] { "LoanId", "NumeroCuota" });

            migrationBuilder.CreateIndex(
                name: "IX_CreditCards_NumeroTarjeta",
                table: "CreditCards",
                column: "NumeroTarjeta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditCards_UserId",
                table: "CreditCards",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_UserId_NumeroCuentaBeneficiario",
                table: "Beneficiaries",
                columns: new[] { "UserId", "NumeroCuentaBeneficiario" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_OperatedByUserId_FechaTransaccion",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_SavingsAccounts_NumeroCuenta",
                table: "SavingsAccounts");

            migrationBuilder.DropIndex(
                name: "IX_LoanPaymentSchedules_LoanId_NumeroCuota",
                table: "LoanPaymentSchedules");

            migrationBuilder.DropIndex(
                name: "IX_CreditCards_NumeroTarjeta",
                table: "CreditCards");

            migrationBuilder.DropIndex(
                name: "IX_CreditCards_UserId",
                table: "CreditCards");

            migrationBuilder.DropIndex(
                name: "IX_Beneficiaries_UserId_NumeroCuentaBeneficiario",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "OperatedByUserId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "OperationType",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "EstadoPago",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "EsAvanceEfectivo",
                table: "CreditCardConsumptions");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "CreditCards",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_LoanPaymentSchedules_LoanId",
                table: "LoanPaymentSchedules",
                column: "LoanId");
        }
    }
}
