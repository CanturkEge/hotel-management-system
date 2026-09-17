using HotelManagement.Application.Common;
using HotelManagement.Domain.Entities;
using HotelManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
namespace HotelManagement.Infrastructure.Data;

public static class DatabaseSetup
{
    public static async Task InitializeAsync(HotelDbContext db,UserManager<AppUser> users,RoleManager<IdentityRole<Guid>> roles,IConfiguration config)
    {
        if(!db.Database.GetMigrations().Any()) throw new InvalidOperationException("Migration yok. Once scripts/Setup.ps1 calistirin.");
        await db.Database.MigrateAsync();
        await using var transaction=await db.Database.BeginTransactionAsync();
        await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(482948239)");
        foreach(var role in Roles.All)
            if(!await roles.RoleExistsAsync(role)) Ensure(await roles.CreateAsync(new IdentityRole<Guid>(role)));
        await SeedUser("SuperAdmin",Roles.SuperAdmin,"Genel Yönetici");
        await SeedUser("Admin",Roles.Admin,"Oda Yöneticisi");
        var roomTypes=await db.RoomTypes.ToListAsync();
        var createdTypes=new HashSet<Guid>();
        foreach(var preset in DefaultRoomTypes())
        {
            var existing=roomTypes.FirstOrDefault(x=>x.Name.Equals(preset.Name,StringComparison.OrdinalIgnoreCase));
            if(existing!=null)
            {
                if(string.IsNullOrWhiteSpace(existing.ImageUrls)) existing.ImageUrls=preset.ImageUrls;
                continue;
            }
            db.RoomTypes.Add(preset); roomTypes.Add(preset); createdTypes.Add(preset.Id);
        }
        if(!roomTypes.Any(x=>x.IsFeatured))
            foreach(var pair in roomTypes.OrderBy(x=>x.BasePrice).Take(4).Select((item,index)=>(item,index)))
            { pair.item.IsFeatured=true; pair.item.FeaturedOrder=pair.index; }
        var existingRoomNumbers=(await db.Rooms.Select(x=>x.Number).ToListAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var sampleRooms=new[]
        {
            ("101",1,"Deluxe Oda"),("102",1,"Deluxe Oda"),("201",2,"Standard Oda"),
            ("202",2,"Standard Oda"),("301",3,"Aile Süiti"),("401",4,"Executive Boğaz Süiti")
        };
        foreach(var (number,floor,typeName) in sampleRooms)
        {
            var type=roomTypes.First(x=>x.Name.Equals(typeName,StringComparison.OrdinalIgnoreCase));
            if(createdTypes.Contains(type.Id) && existingRoomNumbers.Add(number))
                db.Rooms.Add(new Room {Number=number,Floor=floor,RoomTypeId=type.Id});
        }
        if(!await db.HomePageContents.AnyAsync()) db.HomePageContents.Add(DefaultHome());
        if(!await db.NewsArticles.AnyAsync()) db.NewsArticles.AddRange(DefaultNews());
        if(!await db.StayPackages.AnyAsync()) db.StayPackages.AddRange(DefaultPackages());
        await db.SaveChangesAsync();
        // Private schema: no Data API grants. Enable RLS as defense in depth.
        // Runtime connects through the configured database owner; authorization is enforced by C#.
        await db.Database.ExecuteSqlRawAsync("""
            REVOKE ALL ON SCHEMA hotel FROM PUBLIC, anon, authenticated;
            REVOKE ALL ON ALL TABLES IN SCHEMA hotel FROM PUBLIC, anon, authenticated;
            DO $$ DECLARE t record; BEGIN
                FOR t IN SELECT tablename FROM pg_tables WHERE schemaname='hotel' LOOP
                    EXECUTE format('ALTER TABLE hotel.%I ENABLE ROW LEVEL SECURITY',t.tablename);
                END LOOP;
            END $$;
            """);
        await transaction.CommitAsync();

        async Task SeedUser(string section,string role,string name)
        {
            if((await users.GetUsersInRoleAsync(role)).Count>0) return;
            var email=config[$"Seed:{section}:Email"];
            var password=config[$"Seed:{section}:Password"];
            if(string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException($"Seed:{section}:Email ve Password User Secrets icinde gerekli.");
            if(await users.FindByEmailAsync(email)!=null) throw new InvalidOperationException("Baslangic yoneticisi e-postasi baska hesapta kullaniliyor.");
            var user=new AppUser{Id=Guid.NewGuid(),UserName=email,Email=email,FullName=name};
            Ensure(await users.CreateAsync(user,password)); Ensure(await users.AddToRoleAsync(user,role));
        }
    }
    private static void Ensure(IdentityResult result)
    {
        if(!result.Succeeded) throw new InvalidOperationException(string.Join(" ",result.Errors.Select(x=>x.Description)));
    }

    private static RoomType[] DefaultRoomTypes()=>
    [
        new() {Name="Standard Oda",Description="Şehir molaları ve iş seyahatleri için sade, ferah ve işlevsel bir oda.",IsFeatured=true,FeaturedOrder=0,
            BasePrice=2750,Capacity=2,BedCount=1,SizeInSquareMeters=24,Amenities="Wi-Fi, Klima, Akıllı TV, Çalışma masası, Duş",
            ImageUrls="/images/room-standard.webp"},
        new() {Name="Deluxe Oda",Description="Aydınlık yaşam alanı, geniş çift kişilik yatak ve dinlenme köşesi.",IsFeatured=true,FeaturedOrder=1,
            BasePrice=3500,Capacity=2,BedCount=1,SizeInSquareMeters=32,Amenities="Wi-Fi, Klima, Çalışma masası, Duş, Minibar",
            ImageUrls="/images/room-deluxe.webp"},
        new() {Name="Aile Süiti",Description="Ayrı uyku alanları ve geniş yaşam bölümüyle aileler için rahat bir konaklama.",IsFeatured=true,FeaturedOrder=2,
            BasePrice=5250,Capacity=4,BedCount=3,SizeInSquareMeters=52,Amenities="Wi-Fi, Klima, Akıllı TV, Oturma alanı, Minibar, Küvet",
            ImageUrls="/images/room-family-suite.webp"},
        new() {Name="Executive Boğaz Süiti",Description="Panoramik manzara, ayrı oturma alanı ve seçkin detaylarla Meridian'ın en özel süiti.",IsFeatured=true,FeaturedOrder=3,
            BasePrice=7900,Capacity=3,BedCount=1,SizeInSquareMeters=68,Amenities="Boğaz manzarası, Wi-Fi, Klima, Salon, Nespresso, Minibar, Küvet",
            ImageUrls="/images/room-executive-bosphorus.webp"}
    ];

    private static HomePageContent DefaultHome()=>new() {Id=HomePageContent.SingletonId,
        HeroEyebrow="MERIDIAN HOTEL · İSTANBUL",HeroTitle="Şehrin içinde. Telaşın dışında.",
        HeroText="İyi tasarlanmış odalar, sade bir rezervasyon deneyimi ve şehri kendi ritminizde yaşamanız için size ait bir alan.",HeroCaption="Meridian Deluxe Oda",
        RoomsEyebrow="ODALAR & SUİTLER",RoomsTitle="Her yolculuk için bir alan.",RoomsText="İş seyahatinden hafta sonu kaçamağına, ihtiyacınız kadar sade ve konforlu.",
        ExperienceEyebrow="MERIDIAN DENEYİMİ",ExperienceTitle="Odanızdan daha fazlası.",ExperienceText="Şehrin enerjisine yakın, kalabalığın gürültüsünden uzakta. Günün her anı için düşünülmüş küçük detaylar.",
        ExperienceOneTitle="Yerel kahvaltı",ExperienceOneText="Mevsim ürünleri ve mutfağımızdan sıcak tabaklarla telaşsız sabahlar.",
        ExperienceTwoTitle="Mahalle rotaları",ExperienceTwoText="Ekibimizin seçtiği yürüyüş, lezzet ve kültür durakları.",
        ExperienceThreeTitle="Dijital misafir alanı",ExperienceThreeText="Rezervasyonunuzu, durumunu ve geçmiş konaklamalarınızı tek yerde görün.",
        NewsEyebrow="MERIDIAN JOURNAL",NewsTitle="Şehirden notlar.",ContactEyebrow="MİSAFİR İLİŞKİLERİ",
        ContactTitle="Konaklamanızı birlikte planlayalım.",ContactText="Özel bir isteğiniz veya aklınıza takılan bir şey varsa ekibimiz burada.",
        ContactPhone="+90 216 000 00 00",ContactEmail="hello@meridianhotel.example"};

    private static NewsArticle[] DefaultNews()=>
    [
        new() {Slug="sehirde-yavas-bir-hafta-sonu",Category="Şehir Rehberi",Title="Şehirde yavaş bir hafta sonu",IsFeatured=true,SortOrder=0,
            Summary="Koşturmayı bırakıp İstanbul'un sakin köşelerini keşfetmek için küçük bir Meridian rotası.",
            Body="Güne telaşsız bir kahvaltıyla başlayın. Moda sahilinde kısa bir yürüyüşten sonra Yeldeğirmeni'nin ara sokaklarına geçin; küçük dükkânlar, kahve molaları ve tarihi apartmanlar günün temposunu kendiliğinden düşürür.\n\nAkşamüstünü gün batımına ayırın. Otelinize döndüğünüzde resepsiyon ekibimiz yakın çevredeki güncel önerileri paylaşabilir. En iyi şehir planı bazen en az duraklı olandır.",PublishedAt=new(2026,9,12),ReadingMinutes=3},
        new() {Slug="meridian-kahvalti-ritueli",Category="Lezzet",Title="Meridian kahvaltı ritüeli",IsFeatured=true,SortOrder=1,
            Summary="Yerel ürünler, mevsim tatları ve uzun sohbetlere yakışan sade bir sabah masası.",
            Body="Kahvaltımızda gösterişten çok iyi ürüne yer açıyoruz. Mevsim meyveleri, günlük ekmekler, yerel peynirler ve mutfağımızdan çıkan sıcak tabaklar küçük porsiyonlarla masaya geliyor.\n\nAlerjen veya özel beslenme tercihiniz varsa varıştan önce bize ulaşmanız yeterli. Ekibimiz uygun seçenekleri önceden planlar.",PublishedAt=new(2026,9,8),ReadingMinutes=2},
        new() {Slug="konaklamanizi-kolaylastiran-yenilikler",Category="Meridian'dan",Title="Konaklamanızı kolaylaştıran yenilikler",IsFeatured=true,SortOrder=2,
            Summary="Rezervasyondan çıkışa kadar daha açık, hızlı ve sakin bir dijital misafir deneyimi.",
            Body="Yeni misafir alanımız ile rezervasyon taleplerinizi tek ekrandan takip edebilir, yaklaşan konaklamanızı görebilir ve tamamlanan ziyaretinizi değerlendirebilirsiniz.\n\nMüsaitlik ekranı tarih ve kişi sayısına göre uygun odaları karşılaştırır. Her adımda toplam fiyatı görürsünüz; sürpriz ücret yoktur.",PublishedAt=new(2026,9,1),ReadingMinutes=2}
    ];

    private static StayPackage[] DefaultPackages()=>
    [
        new() {Name="Standart",Description="Sade ve esnek konaklama deneyimi.",Benefits="Oda konaklaması\nÜcretsiz Wi-Fi\n7/24 resepsiyon",PricePerNight=0,SortOrder=0},
        new() {Name="Gold",Description="Konforunu artırmak isteyen Meridian üyelerine özel paket.",Benefits="Standart paket ayrıcalıkları\nGünlük kahvaltı\n14.00'e kadar geç çıkış\nKarşılama ikramı",PricePerNight=750,SortOrder=1},
        new() {Name="Premium",Description="Konaklamayı baştan sona ayrıcalıklı hale getiren üst seviye paket.",Benefits="Gold paket ayrıcalıkları\nHavalimanı transferi\nOda önceliği\nMinibar başlangıç seti",PricePerNight=1500,SortOrder=2}
    ];
}
