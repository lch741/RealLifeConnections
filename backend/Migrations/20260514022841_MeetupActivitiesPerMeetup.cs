using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class MeetupActivitiesPerMeetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeetupEvents_Activities_ActivityId",
                table: "MeetupEvents");

            migrationBuilder.DropIndex(
                name: "IX_MeetupEvents_ActivityId",
                table: "MeetupEvents");

            migrationBuilder.DeleteData(
                table: "Activities",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Activities",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Activities",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Activities",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Activities",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Activities",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "ActivityId",
                table: "MeetupEvents");

            migrationBuilder.AddColumn<int>(
                name: "MeetupEventId",
                table: "Activities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Activities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Activities_MeetupEventId_Order",
                table: "Activities",
                columns: new[] { "MeetupEventId", "Order" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_MeetupEvents_MeetupEventId",
                table: "Activities",
                column: "MeetupEventId",
                principalTable: "MeetupEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_MeetupEvents_MeetupEventId",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_MeetupEventId_Order",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "MeetupEventId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Activities");

            migrationBuilder.AddColumn<int>(
                name: "ActivityId",
                table: "MeetupEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "Activities",
                columns: new[] { "Id", "Description", "Name", "Type" },
                values: new object[,]
                {
                    { 1, "Coffee meetups", "Coffee", 0 },
                    { 2, "Walking and exploring", "Walk", 1 },
                    { 3, "Fitness and workouts", "Gym", 3 },
                    { 4, "Dining and meals", "Food", 2 },
                    { 5, "Drinks and hangouts", "Drinks", 4 },
                    { 6, "Explore somewhere new", "Explore", 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeetupEvents_ActivityId",
                table: "MeetupEvents",
                column: "ActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_MeetupEvents_Activities_ActivityId",
                table: "MeetupEvents",
                column: "ActivityId",
                principalTable: "Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
