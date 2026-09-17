using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MembershipPackagesAndReservationHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Reservation_Values",
                schema: "hotel",
                table: "Reservations");

            migrationBuilder.AddColumn<string>(
                name: "PackageBenefits",
                schema: "hotel",
                table: "Reservations",
                type: "character varying(1200)",
                maxLength: 1200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PackageDescription",
                schema: "hotel",
                table: "Reservations",
                type: "character varying(600)",
                maxLength: 600,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PackageName",
                schema: "hotel",
                table: "Reservations",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PackagePricePerNight",
                schema: "hotel",
                table: "Reservations",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PackageSubtotal",
                schema: "hotel",
                table: "Reservations",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RoomSubtotal",
                schema: "hotel",
                table: "Reservations",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "StayPackageId",
                schema: "hotel",
                table: "Reservations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReservationEvents",
                schema: "hotel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReservationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    Actor = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservationEvents_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalSchema: "hotel",
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StayPackages",
                schema: "hotel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    Benefits = table.Column<string>(type: "character varying(1200)", maxLength: 1200, nullable: false),
                    PricePerNight = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StayPackages", x => x.Id);
                    table.CheckConstraint("CK_StayPackage_Values", "\"PricePerNight\" >= 0 AND \"SortOrder\" >= 0");
                });

            migrationBuilder.Sql(
                """
                INSERT INTO hotel."StayPackages"
                    ("Id", "Name", "Description", "Benefits", "PricePerNight", "IsActive", "SortOrder", "CreatedAtUtc")
                VALUES
                    ('11111111-1111-1111-1111-111111111111', 'Standart',
                     'Sade ve esnek konaklama deneyimi.',
                     E'Oda konaklaması\nÜcretsiz Wi-Fi\n7/24 resepsiyon',
                     0, TRUE, 0, NOW()),
                    ('22222222-2222-2222-2222-222222222222', 'Gold',
                     'Konforunu artırmak isteyen Meridian üyelerine özel paket.',
                     E'Standart paket ayrıcalıkları\nGünlük kahvaltı\n14.00\'e kadar geç çıkış\nKarşılama ikramı',
                     750, TRUE, 1, NOW()),
                    ('33333333-3333-3333-3333-333333333333', 'Premium',
                     'Konaklamayı baştan sona ayrıcalıklı hale getiren üst seviye paket.',
                     E'Gold paket ayrıcalıkları\nHavalimanı transferi\nOda önceliği\nMinibar başlangıç seti',
                     1500, TRUE, 2, NOW());

                UPDATE hotel."Reservations"
                SET "StayPackageId" = '11111111-1111-1111-1111-111111111111',
                    "PackageName" = 'Standart',
                    "PackageDescription" = 'Sade ve esnek konaklama deneyimi.',
                    "PackageBenefits" = E'Oda konaklaması\nÜcretsiz Wi-Fi\n7/24 resepsiyon',
                    "PackagePricePerNight" = 0,
                    "RoomSubtotal" = "TotalPrice",
                    "PackageSubtotal" = 0;

                INSERT INTO hotel."ReservationEvents"
                    ("Id", "ReservationId", "Status", "Title", "Description", "Actor", "CreatedAtUtc")
                SELECT gen_random_uuid(), "Id", "Status",
                       CASE "Status"
                           WHEN 1 THEN 'Rezervasyon talebi oluşturuldu'
                           WHEN 2 THEN 'Rezervasyon onaylandı'
                           WHEN 3 THEN 'Otele giriş yapıldı'
                           WHEN 4 THEN 'Konaklama tamamlandı'
                           WHEN 5 THEN 'Rezervasyon iptal edildi'
                           WHEN 6 THEN 'Rezervasyon reddedildi'
                           ELSE 'Rezervasyon güncellendi'
                       END,
                       'Mevcut rezervasyon kaydı geçmiş görünümü için aktarıldı.',
                       'Sistem', "CreatedAtUtc"
                FROM hotel."Reservations";

                REVOKE ALL ON hotel."StayPackages", hotel."ReservationEvents"
                    FROM PUBLIC, anon, authenticated;
                ALTER TABLE hotel."StayPackages" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE hotel."ReservationEvents" ENABLE ROW LEVEL SECURITY;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_StayPackageId",
                schema: "hotel",
                table: "Reservations",
                column: "StayPackageId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Reservation_Values",
                schema: "hotel",
                table: "Reservations",
                sql: "\"CheckOutDate\" > \"CheckInDate\" AND \"GuestCount\" > 0 AND \"NightlyPrice\" >= 0 AND \"PackagePricePerNight\" >= 0 AND \"RoomSubtotal\" >= 0 AND \"PackageSubtotal\" >= 0 AND \"TotalPrice\" = \"RoomSubtotal\" + \"PackageSubtotal\"");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationEvents_ReservationId_CreatedAtUtc",
                schema: "hotel",
                table: "ReservationEvents",
                columns: new[] { "ReservationId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StayPackages_IsActive_SortOrder",
                schema: "hotel",
                table: "StayPackages",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_StayPackages_Name",
                schema: "hotel",
                table: "StayPackages",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_StayPackages_StayPackageId",
                schema: "hotel",
                table: "Reservations",
                column: "StayPackageId",
                principalSchema: "hotel",
                principalTable: "StayPackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_StayPackages_StayPackageId",
                schema: "hotel",
                table: "Reservations");

            migrationBuilder.DropTable(
                name: "ReservationEvents",
                schema: "hotel");

            migrationBuilder.DropTable(
                name: "StayPackages",
                schema: "hotel");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_StayPackageId",
                schema: "hotel",
                table: "Reservations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Reservation_Values",
                schema: "hotel",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PackageBenefits",
                schema: "hotel",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PackageDescription",
                schema: "hotel",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PackageName",
                schema: "hotel",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PackagePricePerNight",
                schema: "hotel",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PackageSubtotal",
                schema: "hotel",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "RoomSubtotal",
                schema: "hotel",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "StayPackageId",
                schema: "hotel",
                table: "Reservations");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Reservation_Values",
                schema: "hotel",
                table: "Reservations",
                sql: "\"CheckOutDate\" > \"CheckInDate\" AND \"GuestCount\" > 0 AND \"TotalPrice\" >= 0");
        }
    }
}
