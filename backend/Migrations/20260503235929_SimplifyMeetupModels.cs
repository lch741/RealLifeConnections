using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyMeetupModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "ChillToEnergetic",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntrovertToExtrovert",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlannerToSpontaneous",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredDaysOfWeek",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreferredDistanceKm",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredTimeOfDay",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Suburb",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TalkativeToQuiet",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                table: "Conversations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndsAt",
                table: "Conversations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsClosed",
                table: "Conversations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MeetupEventId",
                table: "Conversations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "Conversations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MeetupEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatorId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Region = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Suburb = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LocationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ActivityId = table.Column<int>(type: "integer", nullable: false),
                    EventDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    MaxParticipants = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetupEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetupEvents_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MeetupEvents_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeetupFeedbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MeetupEventId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Result = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetupFeedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetupFeedbacks_MeetupEvents_MeetupEventId",
                        column: x => x.MeetupEventId,
                        principalTable: "MeetupEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MeetupFeedbacks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeetupLocationSuggestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MeetupEventId = table.Column<int>(type: "integer", nullable: false),
                    SuggestedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    IsChosen = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetupLocationSuggestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetupLocationSuggestions_MeetupEvents_MeetupEventId",
                        column: x => x.MeetupEventId,
                        principalTable: "MeetupEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MeetupLocationSuggestions_Users_SuggestedByUserId",
                        column: x => x.SuggestedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserMeetups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    MeetupEventId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMeetups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMeetups_MeetupEvents_MeetupEventId",
                        column: x => x.MeetupEventId,
                        principalTable: "MeetupEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserMeetups_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Activities",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Coffee meetups", "Coffee" },
                    { 2, "Walking and exploring", "Walk" },
                    { 3, "Fitness and workouts", "Gym" },
                    { 4, "Dining and meals", "Food" },
                    { 5, "Drinks and hangouts", "Drinks" },
                    { 6, "Explore somewhere new", "Explore" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_User_Region_Suburb",
                table: "Users",
                columns: new[] { "Region", "Suburb" });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_MeetupEventId",
                table: "Conversations",
                column: "MeetupEventId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetupEvent_Location_Date",
                table: "MeetupEvents",
                columns: new[] { "Region", "Suburb", "EventDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MeetupEvents_ActivityId",
                table: "MeetupEvents",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetupEvents_CreatorId",
                table: "MeetupEvents",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetupFeedbacks_MeetupEventId",
                table: "MeetupFeedbacks",
                column: "MeetupEventId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetupFeedbacks_UserId_MeetupEventId",
                table: "MeetupFeedbacks",
                columns: new[] { "UserId", "MeetupEventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeetupLocationSuggestions_MeetupEventId",
                table: "MeetupLocationSuggestions",
                column: "MeetupEventId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetupLocationSuggestions_SuggestedByUserId",
                table: "MeetupLocationSuggestions",
                column: "SuggestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMeetups_MeetupEventId",
                table: "UserMeetups",
                column: "MeetupEventId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMeetups_UserId_MeetupEventId",
                table: "UserMeetups",
                columns: new[] { "UserId", "MeetupEventId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_MeetupEvents_MeetupEventId",
                table: "Conversations",
                column: "MeetupEventId",
                principalTable: "MeetupEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_MeetupEvents_MeetupEventId",
                table: "Conversations");

            migrationBuilder.DropTable(
                name: "MeetupFeedbacks");

            migrationBuilder.DropTable(
                name: "MeetupLocationSuggestions");

            migrationBuilder.DropTable(
                name: "UserMeetups");

            migrationBuilder.DropTable(
                name: "MeetupEvents");

            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_User_Region_Suburb",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_MeetupEventId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "ChillToEnergetic",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IntrovertToExtrovert",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PlannerToSpontaneous",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PreferredDaysOfWeek",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PreferredDistanceKm",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PreferredTimeOfDay",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Suburb",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TalkativeToQuiet",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "EndsAt",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "IsClosed",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "MeetupEventId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "Conversations");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
