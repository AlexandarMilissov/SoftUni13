using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class _2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Passengers_Reservations_ReservationId",
                table: "Passengers");

            migrationBuilder.DropIndex(
                name: "IX_Passengers_ReservationId",
                table: "Passengers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Passengers_ReservationId",
                table: "Passengers",
                column: "ReservationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Passengers_Reservations_ReservationId",
                table: "Passengers",
                column: "ReservationId",
                principalTable: "Reservations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
