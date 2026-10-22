using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Takwene.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Artists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DSPs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DSPs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tracks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ArtistId = table.Column<int>(type: "int", nullable: false),
                    Isrc = table.Column<string>(type: "varchar(12)", nullable: false),
                    ReleaseDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Genre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tracks", x => x.Id);
                    table.CheckConstraint("CK_Tracks_Status", "[Status] IN ('draft','submitted','distributed')");
                    table.ForeignKey(
                        name: "FK_Tracks_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TokenHash = table.Column<string>(type: "char(64)", maxLength: 64, nullable: false),
                    FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReplacedByTokenId = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.CheckConstraint("CK_RefreshTokens_Expiry", "[ExpiresAt] > [CreatedAt]");
                    table.ForeignKey(
                        name: "FK_RefreshTokens_RefreshTokens_ReplacedByTokenId",
                        column: x => x.ReplacedByTokenId,
                        principalTable: "RefreshTokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrackDistributions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackId = table.Column<int>(type: "int", nullable: false),
                    DspId = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackDistributions", x => x.Id);
                    table.CheckConstraint("CK_Distributions_Status", "[Status] IN ('pending','live','rejected')");
                    table.ForeignKey(
                        name: "FK_TrackDistributions_DSPs_DspId",
                        column: x => x.DspId,
                        principalTable: "DSPs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrackDistributions_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Artists",
                columns: new[] { "Id", "Country", "Email", "Name" },
                values: new object[,]
                {
                    { 1, "Egypt", "layla@example.com", "Layla Noor" },
                    { 2, "Morocco", "omar@example.com", "Omar Atlas" },
                    { 3, "Lebanon", "nour@example.com", "Nour Al Bahr" }
                });

            migrationBuilder.InsertData(
                table: "DSPs",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Spotify" },
                    { 2, "Apple Music" },
                    { 3, "YouTube" }
                });

            migrationBuilder.InsertData(
                table: "Tracks",
                columns: new[] { "Id", "ArtistId", "Genre", "Isrc", "ReleaseDate", "Status", "Title" },
                values: new object[,]
                {
                    { 1, 1, "Arabic Pop", "EGTKW2600001", new DateOnly(2026, 9, 1), "draft", "Cairo Nights" },
                    { 2, 1, "R&B", "EGTKW2600002", new DateOnly(2026, 9, 2), "submitted", "First Light" },
                    { 3, 1, "Electronic", "EGTKW2600003", new DateOnly(2026, 9, 3), "distributed", "Nile Breeze" },
                    { 4, 2, "Hip Hop", "EGTKW2600004", new DateOnly(2026, 9, 4), "draft", "Atlas Road" },
                    { 5, 2, "Jazz", "EGTKW2600005", new DateOnly(2026, 9, 5), "submitted", "Casablanca Rain" },
                    { 6, 2, "Electronic", "EGTKW2600006", new DateOnly(2026, 9, 6), "distributed", "Desert Echo" },
                    { 7, 3, "Indie", "EGTKW2600007", new DateOnly(2026, 9, 7), "draft", "Beirut Morning" },
                    { 8, 3, "Arabic Pop", "EGTKW2600008", new DateOnly(2026, 9, 8), "submitted", "Sea Letters" },
                    { 9, 3, "Acoustic", "EGTKW2600009", new DateOnly(2026, 9, 9), "distributed", "Home Again" }
                });

            migrationBuilder.InsertData(
                table: "TrackDistributions",
                columns: new[] { "Id", "DspId", "Status", "SubmittedAt", "TrackId" },
                values: new object[,]
                {
                    { 1, 1, "pending", new DateTime(2026, 8, 20, 12, 0, 0, 0, DateTimeKind.Utc), 2 },
                    { 2, 1, "live", new DateTime(2026, 8, 20, 12, 0, 0, 0, DateTimeKind.Utc), 3 },
                    { 3, 2, "live", new DateTime(2026, 8, 20, 12, 0, 0, 0, DateTimeKind.Utc), 3 },
                    { 4, 2, "rejected", new DateTime(2026, 8, 20, 12, 0, 0, 0, DateTimeKind.Utc), 5 },
                    { 5, 3, "live", new DateTime(2026, 8, 20, 12, 0, 0, 0, DateTimeKind.Utc), 6 },
                    { 6, 1, "pending", new DateTime(2026, 8, 20, 12, 0, 0, 0, DateTimeKind.Utc), 8 },
                    { 7, 2, "live", new DateTime(2026, 8, 20, 12, 0, 0, 0, DateTimeKind.Utc), 9 },
                    { 8, 3, "pending", new DateTime(2026, 8, 20, 12, 0, 0, 0, DateTimeKind.Utc), 9 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DSPs_Name",
                table: "DSPs",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_FamilyId",
                table: "RefreshTokens",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ReplacedByTokenId",
                table: "RefreshTokens",
                column: "ReplacedByTokenId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackDistributions_DspId",
                table: "TrackDistributions",
                column: "DspId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackDistributions_TrackId_DspId",
                table: "TrackDistributions",
                columns: new[] { "TrackId", "DspId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_ArtistId",
                table: "Tracks",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_Isrc",
                table: "Tracks",
                column: "Isrc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users",
                column: "NormalizedEmail",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "TrackDistributions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "DSPs");

            migrationBuilder.DropTable(
                name: "Tracks");

            migrationBuilder.DropTable(
                name: "Artists");
        }
    }
}
