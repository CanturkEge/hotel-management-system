using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DynamicContentAndUploads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RoomType_Values",
                schema: "hotel",
                table: "RoomTypes");

            migrationBuilder.AddColumn<int>(
                name: "FeaturedOrder",
                schema: "hotel",
                table: "RoomTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                schema: "hotel",
                table: "RoomTypes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "MediaAssets",
                schema: "hotel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Data = table.Column<byte[]>(type: "bytea", nullable: false),
                    Length = table.Column<int>(type: "integer", nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaAssets", x => x.Id);
                    table.CheckConstraint("CK_MediaAsset_Length", "\"Length\" > 0 AND \"Length\" <= 5242880");
                });

            migrationBuilder.CreateTable(
                name: "HomePageContents",
                schema: "hotel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HeroEyebrow = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HeroTitle = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    HeroText = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    HeroCaption = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    HeroImageId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoomsEyebrow = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RoomsTitle = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    RoomsText = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    ExperienceEyebrow = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExperienceTitle = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    ExperienceText = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    ExperienceOneTitle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExperienceOneText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExperienceTwoTitle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExperienceTwoText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExperienceThreeTitle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExperienceThreeText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    NewsEyebrow = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NewsTitle = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    ContactEyebrow = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContactTitle = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    ContactText = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    ContactPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomePageContents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HomePageContents_MediaAssets_HeroImageId",
                        column: x => x.HeroImageId,
                        principalSchema: "hotel",
                        principalTable: "MediaAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NewsArticles",
                schema: "hotel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Category = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Body = table.Column<string>(type: "character varying(12000)", maxLength: 12000, nullable: false),
                    PublishedAt = table.Column<DateOnly>(type: "date", nullable: false),
                    ReadingMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CoverImageId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsArticles", x => x.Id);
                    table.CheckConstraint("CK_NewsArticle_Values", "\"ReadingMinutes\" BETWEEN 1 AND 60 AND \"SortOrder\" >= 0");
                    table.ForeignKey(
                        name: "FK_NewsArticles_MediaAssets_CoverImageId",
                        column: x => x.CoverImageId,
                        principalSchema: "hotel",
                        principalTable: "MediaAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoomTypeImages",
                schema: "hotel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    AltText = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomTypeImages", x => x.Id);
                    table.CheckConstraint("CK_RoomTypeImage_Order", "\"SortOrder\" >= 0");
                    table.ForeignKey(
                        name: "FK_RoomTypeImages_MediaAssets_MediaAssetId",
                        column: x => x.MediaAssetId,
                        principalSchema: "hotel",
                        principalTable: "MediaAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoomTypeImages_RoomTypes_RoomTypeId",
                        column: x => x.RoomTypeId,
                        principalSchema: "hotel",
                        principalTable: "RoomTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomTypes_IsFeatured_FeaturedOrder",
                schema: "hotel",
                table: "RoomTypes",
                columns: new[] { "IsFeatured", "FeaturedOrder" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_RoomType_Values",
                schema: "hotel",
                table: "RoomTypes",
                sql: "\"BasePrice\" > 0 AND \"Capacity\" > 0 AND \"BedCount\" > 0 AND \"SizeInSquareMeters\" > 0 AND \"FeaturedOrder\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_HomePageContents_HeroImageId",
                schema: "hotel",
                table: "HomePageContents",
                column: "HeroImageId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_Sha256",
                schema: "hotel",
                table: "MediaAssets",
                column: "Sha256");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_CoverImageId",
                schema: "hotel",
                table: "NewsArticles",
                column: "CoverImageId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_IsPublished_IsFeatured_SortOrder_PublishedAt",
                schema: "hotel",
                table: "NewsArticles",
                columns: new[] { "IsPublished", "IsFeatured", "SortOrder", "PublishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_Slug",
                schema: "hotel",
                table: "NewsArticles",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoomTypeImages_MediaAssetId",
                schema: "hotel",
                table: "RoomTypeImages",
                column: "MediaAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomTypeImages_RoomTypeId_SortOrder",
                schema: "hotel",
                table: "RoomTypeImages",
                columns: new[] { "RoomTypeId", "SortOrder" });

            migrationBuilder.Sql("""
                WITH ranked AS (
                    SELECT "Id", (row_number() OVER (ORDER BY "BasePrice", "Name") - 1)::integer AS display_order
                    FROM hotel."RoomTypes" WHERE "IsActive" = TRUE LIMIT 4
                )
                UPDATE hotel."RoomTypes" room_type
                SET "IsFeatured" = TRUE, "FeaturedOrder" = ranked.display_order
                FROM ranked WHERE room_type."Id" = ranked."Id";

                INSERT INTO hotel."HomePageContents" (
                    "Id","HeroEyebrow","HeroTitle","HeroText","HeroCaption","HeroImageId",
                    "RoomsEyebrow","RoomsTitle","RoomsText","ExperienceEyebrow","ExperienceTitle","ExperienceText",
                    "ExperienceOneTitle","ExperienceOneText","ExperienceTwoTitle","ExperienceTwoText",
                    "ExperienceThreeTitle","ExperienceThreeText","NewsEyebrow","NewsTitle","ContactEyebrow",
                    "ContactTitle","ContactText","ContactPhone","ContactEmail","CreatedAtUtc","UpdatedAtUtc")
                VALUES (
                    '03815fb6-77b7-44c2-a9c1-a632de206a4d','MERIDIAN HOTEL · İSTANBUL','Şehrin içinde. Telaşın dışında.',
                    'İyi tasarlanmış odalar, sade bir rezervasyon deneyimi ve şehri kendi ritminizde yaşamanız için size ait bir alan.',
                    'Meridian Deluxe Oda',NULL,'ODALAR & SUİTLER','Her yolculuk için bir alan.',
                    'İş seyahatinden hafta sonu kaçamağına, ihtiyacınız kadar sade ve konforlu.','MERIDIAN DENEYİMİ',
                    'Odanızdan daha fazlası.','Şehrin enerjisine yakın, kalabalığın gürültüsünden uzakta. Günün her anı için düşünülmüş küçük detaylar.',
                    'Yerel kahvaltı','Mevsim ürünleri ve mutfağımızdan sıcak tabaklarla telaşsız sabahlar.',
                    'Mahalle rotaları','Ekibimizin seçtiği yürüyüş, lezzet ve kültür durakları.',
                    'Dijital misafir alanı','Rezervasyonunuzu, durumunu ve geçmiş konaklamalarınızı tek yerde görün.',
                    'MERIDIAN JOURNAL','Şehirden notlar.','MİSAFİR İLİŞKİLERİ','Konaklamanızı birlikte planlayalım.',
                    'Özel bir isteğiniz veya aklınıza takılan bir şey varsa ekibimiz burada.','+90 216 000 00 00',
                    'hello@meridianhotel.example',CURRENT_TIMESTAMP,NULL)
                ON CONFLICT ("Id") DO NOTHING;

                INSERT INTO hotel."NewsArticles" ("Id","Slug","Category","Title","Summary","Body","PublishedAt",
                    "ReadingMinutes","IsPublished","IsFeatured","SortOrder","CoverImageId","CreatedAtUtc","UpdatedAtUtc") VALUES
                ('33df7614-e0f0-4cf1-ac42-0dad8fe58d14','sehirde-yavas-bir-hafta-sonu','Şehir Rehberi','Şehirde yavaş bir hafta sonu',
                    'Koşturmayı bırakıp İstanbul''un sakin köşelerini keşfetmek için küçük bir Meridian rotası.',
                    E'Güne telaşsız bir kahvaltıyla başlayın. Moda sahilinde kısa bir yürüyüşten sonra Yeldeğirmeni''nin ara sokaklarına geçin; küçük dükkânlar, kahve molaları ve tarihi apartmanlar günün temposunu kendiliğinden düşürür.\n\nAkşamüstünü gün batımına ayırın. Otelinize döndüğünüzde resepsiyon ekibimiz yakın çevredeki güncel önerileri paylaşabilir. En iyi şehir planı bazen en az duraklı olandır.',
                    DATE '2026-09-12',3,TRUE,TRUE,0,NULL,CURRENT_TIMESTAMP,NULL),
                ('ebd0d909-c130-4fb6-bdd1-b494d9493268','meridian-kahvalti-ritueli','Lezzet','Meridian kahvaltı ritüeli',
                    'Yerel ürünler, mevsim tatları ve uzun sohbetlere yakışan sade bir sabah masası.',
                    E'Kahvaltımızda gösterişten çok iyi ürüne yer açıyoruz. Mevsim meyveleri, günlük ekmekler, yerel peynirler ve mutfağımızdan çıkan sıcak tabaklar küçük porsiyonlarla masaya geliyor.\n\nAlerjen veya özel beslenme tercihiniz varsa varıştan önce bize ulaşmanız yeterli. Ekibimiz uygun seçenekleri önceden planlar.',
                    DATE '2026-09-08',2,TRUE,TRUE,1,NULL,CURRENT_TIMESTAMP,NULL),
                ('79cfc755-7594-4f89-8e82-1811a8f864b0','konaklamanizi-kolaylastiran-yenilikler','Meridian''dan','Konaklamanızı kolaylaştıran yenilikler',
                    'Rezervasyondan çıkışa kadar daha açık, hızlı ve sakin bir dijital misafir deneyimi.',
                    E'Yeni misafir alanımız ile rezervasyon taleplerinizi tek ekrandan takip edebilir, yaklaşan konaklamanızı görebilir ve tamamlanan ziyaretinizi değerlendirebilirsiniz.\n\nMüsaitlik ekranı tarih ve kişi sayısına göre uygun odaları karşılaştırır. Her adımda toplam fiyatı görürsünüz; sürpriz ücret yoktur.',
                    DATE '2026-09-01',2,TRUE,TRUE,2,NULL,CURRENT_TIMESTAMP,NULL)
                ON CONFLICT ("Slug") DO NOTHING;

                REVOKE ALL ON hotel."MediaAssets", hotel."RoomTypeImages", hotel."NewsArticles", hotel."HomePageContents"
                    FROM PUBLIC, anon, authenticated;
                ALTER TABLE hotel."MediaAssets" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE hotel."RoomTypeImages" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE hotel."NewsArticles" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE hotel."HomePageContents" ENABLE ROW LEVEL SECURITY;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HomePageContents",
                schema: "hotel");

            migrationBuilder.DropTable(
                name: "NewsArticles",
                schema: "hotel");

            migrationBuilder.DropTable(
                name: "RoomTypeImages",
                schema: "hotel");

            migrationBuilder.DropTable(
                name: "MediaAssets",
                schema: "hotel");

            migrationBuilder.DropIndex(
                name: "IX_RoomTypes_IsFeatured_FeaturedOrder",
                schema: "hotel",
                table: "RoomTypes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RoomType_Values",
                schema: "hotel",
                table: "RoomTypes");

            migrationBuilder.DropColumn(
                name: "FeaturedOrder",
                schema: "hotel",
                table: "RoomTypes");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                schema: "hotel",
                table: "RoomTypes");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RoomType_Values",
                schema: "hotel",
                table: "RoomTypes",
                sql: "\"BasePrice\" > 0 AND \"Capacity\" > 0 AND \"BedCount\" > 0 AND \"SizeInSquareMeters\" > 0");
        }
    }
}
