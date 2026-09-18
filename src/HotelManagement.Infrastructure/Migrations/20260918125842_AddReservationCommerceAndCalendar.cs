using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationCommerceAndCalendar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Reservation_Values",
                schema: "hotel",
                table: "Reservations");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                schema: "hotel",
                table: "Reservations",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PromotionCode",
                schema: "hotel",
                table: "Reservations",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "PromotionId",
                schema: "hotel",
                table: "Reservations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ServicesSubtotal",
                schema: "hotel",
                table: "Reservations",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ExtraServices",
                schema: "hotel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtraServices", x => x.Id);
                    table.CheckConstraint("CK_ExtraService_Values", "\"Price\" >= 0 AND \"SortOrder\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Promotions",
                schema: "hotel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    MinimumNights = table.Column<int>(type: "integer", nullable: false),
                    UsageLimit = table.Column<int>(type: "integer", nullable: true),
                    TimesUsed = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotions", x => x.Id);
                    table.CheckConstraint("CK_Promotion_Values", "\"Kind\" IN (1,2) AND \"Value\" > 0 AND (\"Kind\" <> 1 OR \"Value\" <= 100) AND \"EndDate\" >= \"StartDate\" AND \"MinimumNights\" > 0 AND \"TimesUsed\" >= 0 AND (\"UsageLimit\" IS NULL OR \"UsageLimit\" > 0)");
                });

            migrationBuilder.CreateTable(
                name: "ReservationExtras",
                schema: "hotel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReservationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtraServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationExtras", x => x.Id);
                    table.CheckConstraint("CK_ReservationExtra_Values", "\"UnitPrice\" >= 0 AND \"Quantity\" > 0 AND \"TotalPrice\" = \"UnitPrice\" * \"Quantity\"");
                    table.ForeignKey(
                        name: "FK_ReservationExtras_ExtraServices_ExtraServiceId",
                        column: x => x.ExtraServiceId,
                        principalSchema: "hotel",
                        principalTable: "ExtraServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReservationExtras_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalSchema: "hotel",
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_PromotionId",
                schema: "hotel",
                table: "Reservations",
                column: "PromotionId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Reservation_Values",
                schema: "hotel",
                table: "Reservations",
                sql: "\"CheckOutDate\" > \"CheckInDate\" AND \"GuestCount\" > 0 AND \"NightlyPrice\" >= 0 AND \"PackagePricePerNight\" >= 0 AND \"RoomSubtotal\" >= 0 AND \"PackageSubtotal\" >= 0 AND \"ServicesSubtotal\" >= 0 AND \"DiscountAmount\" >= 0 AND \"DiscountAmount\" <= \"RoomSubtotal\" + \"PackageSubtotal\" + \"ServicesSubtotal\" AND \"TotalPrice\" = \"RoomSubtotal\" + \"PackageSubtotal\" + \"ServicesSubtotal\" - \"DiscountAmount\"");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraServices_IsActive_SortOrder",
                schema: "hotel",
                table: "ExtraServices",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ExtraServices_Name",
                schema: "hotel",
                table: "ExtraServices",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_Code",
                schema: "hotel",
                table: "Promotions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_IsActive_StartDate_EndDate",
                schema: "hotel",
                table: "Promotions",
                columns: new[] { "IsActive", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ReservationExtras_ExtraServiceId",
                schema: "hotel",
                table: "ReservationExtras",
                column: "ExtraServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationExtras_ReservationId_ExtraServiceId",
                schema: "hotel",
                table: "ReservationExtras",
                columns: new[] { "ReservationId", "ExtraServiceId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Promotions_PromotionId",
                schema: "hotel",
                table: "Reservations",
                column: "PromotionId",
                principalSchema: "hotel",
                principalTable: "Promotions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            var seededAt = new DateTime(2026, 9, 18, 0, 0, 0, DateTimeKind.Utc);
            migrationBuilder.InsertData(
                schema: "hotel", table: "ExtraServices",
                columns: new[] { "Id", "Name", "Description", "Price", "IsActive", "SortOrder", "CreatedAtUtc", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { Guid.Parse("b0010000-0000-4000-8000-000000000001"), "Havalimanı Transferi", "Tek yön özel araçla havalimanı transferi.", 1250m, true, 10, seededAt, null! },
                    { Guid.Parse("b0010000-0000-4000-8000-000000000002"), "Oda Servisi", "Odaya özel servis ve ikram paketi.", 450m, true, 20, seededAt, null! },
                    { Guid.Parse("b0010000-0000-4000-8000-000000000003"), "Minibar Paketi", "Konaklama boyunca yenilenen minibar seçkisi.", 600m, true, 30, seededAt, null! },
                    { Guid.Parse("b0010000-0000-4000-8000-000000000004"), "Spa Deneyimi", "Bir kişilik spa ve masaj deneyimi.", 1500m, true, 40, seededAt, null! },
                    { Guid.Parse("b0010000-0000-4000-8000-000000000005"), "Ek Yatak", "Konaklama süresince odaya ek yatak kurulumu.", 900m, true, 50, seededAt, null! }
                });
            migrationBuilder.InsertData(
                schema: "hotel", table: "Promotions",
                columns: new[] { "Id", "Code", "Name", "Description", "Kind", "Value", "StartDate", "EndDate", "MinimumNights", "UsageLimit", "TimesUsed", "IsActive", "CreatedAtUtc", "UpdatedAtUtc" },
                values: new object[] { Guid.Parse("c0010000-0000-4000-8000-000000000001"), "MERIDIAN10", "Meridian'a Hoş Geldiniz", "İki gece ve üzeri konaklamalarda yüzde 10 indirim.", 1, 10m, new DateOnly(2026,1,1), new DateOnly(2030,12,31), 2, 100, 0, true, seededAt, null! });

            migrationBuilder.Sql("""
                REVOKE ALL ON TABLE hotel."ExtraServices", hotel."Promotions", hotel."ReservationExtras" FROM PUBLIC, anon, authenticated;
                ALTER TABLE hotel."ExtraServices" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE hotel."Promotions" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE hotel."ReservationExtras" ENABLE ROW LEVEL SECURITY;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Promotions_PromotionId",
                schema: "hotel",
                table: "Reservations");

            migrationBuilder.DropTable(
                name: "Promotions",
                schema: "hotel");

            migrationBuilder.DropTable(
                name: "ReservationExtras",
                schema: "hotel");

            migrationBuilder.DropTable(
                name: "ExtraServices",
                schema: "hotel");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_PromotionId",
                schema: "hotel",
                table: "Reservations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Reservation_Values",
                schema: "hotel",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                schema: "hotel",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PromotionCode",
                schema: "hotel",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PromotionId",
                schema: "hotel",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ServicesSubtotal",
                schema: "hotel",
                table: "Reservations");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Reservation_Values",
                schema: "hotel",
                table: "Reservations",
                sql: "\"CheckOutDate\" > \"CheckInDate\" AND \"GuestCount\" > 0 AND \"NightlyPrice\" >= 0 AND \"PackagePricePerNight\" >= 0 AND \"RoomSubtotal\" >= 0 AND \"PackageSubtotal\" >= 0 AND \"TotalPrice\" = \"RoomSubtotal\" + \"PackageSubtotal\"");
        }
    }
}
