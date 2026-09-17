using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CorrectReservationEventBackfillTitles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE hotel."ReservationEvents"
                SET "Title" = CASE "Status"
                    WHEN 1 THEN 'Rezervasyon talebi oluşturuldu'
                    WHEN 2 THEN 'Rezervasyon onaylandı'
                    WHEN 3 THEN 'Otele giriş yapıldı'
                    WHEN 4 THEN 'Konaklama tamamlandı'
                    WHEN 5 THEN 'Rezervasyon iptal edildi'
                    WHEN 6 THEN 'Rezervasyon reddedildi'
                    ELSE 'Rezervasyon güncellendi'
                END
                WHERE "Actor" = 'Sistem'
                  AND "Description" = 'Mevcut rezervasyon kaydı geçmiş görünümü için aktarıldı.';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE hotel."ReservationEvents"
                SET "Title" = 'Geçmiş rezervasyon kaydı'
                WHERE "Actor" = 'Sistem'
                  AND "Description" = 'Mevcut rezervasyon kaydı geçmiş görünümü için aktarıldı.';
                """);
        }
    }
}
