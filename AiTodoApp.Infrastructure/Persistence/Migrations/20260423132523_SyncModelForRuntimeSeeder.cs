using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AiTodoApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelForRuntimeSeeder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FullName", "GoogleAccountEmail", "GoogleCalendarId", "GoogleRefreshToken", "GoogleTokenExpiresAt", "IsGoogleConnected", "Password", "TimeZone" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "user1@meetingsplus.local", "Demo User One", "user1@gmail.com", "primary", "seed-refresh-token-user-1", new DateTime(2027, 4, 23, 0, 0, 0, 0, DateTimeKind.Utc), true, "Anubhab*1997", "Asia/Kolkata" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "user2@meetingsplus.local", "Demo User Two", null, null, null, null, false, "Meeting@123", "UTC" }
                });

            migrationBuilder.InsertData(
                table: "Appointments",
                columns: new[] { "Id", "CreatedAt", "Description", "EndTime", "Profile", "Source", "StartTime", "Status", "Title", "UserId" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 4, 23, 0, 0, 0, 0, DateTimeKind.Utc), "Engineering sync for sprint updates.", new DateTime(2026, 4, 25, 10, 30, 0, 0, DateTimeKind.Utc), "Engineering", 1, new DateTime(2026, 4, 25, 10, 0, 0, 0, DateTimeKind.Utc), 2, "Weekly Standup", new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 4, 23, 0, 0, 0, 0, DateTimeKind.Utc), "Discuss open action items from client notes.", new DateTime(2026, 4, 26, 15, 0, 0, 0, DateTimeKind.Utc), "Product", 3, new DateTime(2026, 4, 26, 14, 0, 0, 0, DateTimeKind.Utc), 1, "Client Requirement Review", new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb") }
                });
        }
    }
}
