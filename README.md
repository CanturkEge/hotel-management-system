# HotelManagement — Meridian

ASP.NET Core MVC, .NET 10, ASP.NET Core Identity, EF Core ve PostgreSQL ile otel yönetimi.
Türkçe müşteri sitesi ve personel panelleri aynı Razor/CSS tasarımını kullanır.

## Önce okuyun

Bu paket yeni bir başlangıç sürümüdür. Eski HotelPractice/SQLite veritabanına uygulanmaz.
Mevcut klasörünüzü silmeyin; ZIP'i ayrı bir klasöre çıkarın. İlk kurulum için KURULUM.md'yi izleyin.

**Doğrulama durumu (09.09.2026):** proje Windows/.NET 10 ortamında başarıyla derlendi ve
23 iş kuralı testi geçti. İlk EF Core migration'ı üretildi ve Supabase PostgreSQL üzerinde uygulandı.
Gerçek giriş, yetki ve tarayıcı akışları `TESTLER.md` listesinden ayrıca doğrulanmalıdır.

## Dahil

- Müşteri kayıt/giriş, ayrı personel girişi, çalışan POST çıkış formu.
- Tek başlangıç Admin ve SuperAdmin; Reception, Cleaner, Maintenance personel hesapları.
- Süper admin tarafından personel oluşturma ve personel rol değiştirme.
- Oda tipi: açıklama, fiyat, kapasite, yatak sayısı, alan, olanaklar, 8 adrese kadar görsel galerisi.
- Fiziksel oda ekleme/düzenleme/satıştan kaldırma. Geçmiş kayıtlar korunur.
- Tarih/kişi ile müsaitlik arama; seçilen oda için dolu tarih aralıkları.
- Müşteri talebi veya resepsiyonun müşteri adına talebi; onay/ret, iptal, check-in/check-out.
- Sunucuda fiyat hesaplama, fiyat/oda tipi/oda numarası geçmiş anlık görüntüleri.
- Konaklama geçmişi; tamamlanan konaklamaya tek yorum; yönetici yayın onayı.
- Çıkış sonrası temizlik görevi; bakım bildirimi; rol bazlı görev tamamlama.
- Açık temizlik/bakım görevleri varken oda rezervasyona kapalı.
- Genel bakış: oda, konaklama, bekleyen talep, görev sayıları ve tamamlanan konaklama toplamı.
- Mobil uyumlu Razor ekranları, klavye odağı, formlarda sunucu doğrulaması, başarı/hata durumları.
- Kurulum betiği, bağımlılıksız çalıştırılabilir iş kuralı testleri, GitHub Actions build iş akışı.

## Bu sürümün kapsamı dışında

Ödeme/tahsilat, fatura, e-posta doğrulama/şifre sıfırlama, çok faktörlü giriş, stok/tedarikçi,
sezonluk fiyat tarifesi, çoklu otel, fotoğrafın uygulama içinden yüklenmesi, canlı yayın ve eski veriyi taşıma yoktur.
Görseller bu sürümde yönetici tarafından HTTPS adresleriyle tanımlanır; Supabase Storage panelinden
yükleyip URL eklemek mümkündür. Bu ayrım bilinçlidir: uygulama içinde upload varmış gibi davranılmaz.
Gerçek misafirlerle kullanılmadan önce bu eksikler, veri saklama/mahremiyet süreci, yedekleme ve
operasyonel güvenlik mentorla tamamlanmalı. Frontend tasarımı referans otelin kodunu/fotoğraflarını kopyalamaz.

## Katmanlar

| Proje | Sorumluluk | Referans |
|---|---|---|
| Domain | Entity, enum, temel tarih/fiyat kuralları | Yok |
| Application | DTO, interface, servis, açık mapping | Domain |
| Infrastructure | EF repository, DbContext, Identity, transaction | Application |
| Web | MVC controller, Razor View, CSS, az miktarda JS, DI kaydı | Application + Infrastructure |
| Tests | Servis kurallarını bellek içi test çiftleriyle doğrulama | Application |

Mapping burada ayrı bir `HotelMapping` sınıfında açık C# ile yapılır; AutoMapper bağımlılığı yoktur.
`DateOnly` konaklama günlerini; UTC `DateTime` kayıt/işlem zamanlarını temsil eder.
Otel günü Europe/Istanbul saat dilimine göre belirlenir.

## Hızlı başlangıç

1. Visual Studio'da HotelManagement.sln açın. Başlangıç projesi HotelManagement.Web olsun.
2. Yeni Supabase projesini hazırlayın (KURULUM.md).
3. Solution kökünde PowerShell: `./scripts/Setup.ps1`
4. Kurulum başarılıysa: `dotnet run --project src/HotelManagement.Web --launch-profile http`
5. http://localhost:5080 adresini açın. Yönetici bilgileri kurulumda sizin belirlediğiniz e-posta/şifrelerdir.

Hazır, herkesin bildiği admin şifresi yoktur. Gerçek kişisel bilgileri test verisi olarak girmeyin.

## Teknik sınırlar

- İlk EF migration ve model snapshot repoda bulunur. Sonraki model değişikliklerinde yeni migration
  oluşturun; `EnsureCreated` ile migration karıştırılmaz ve migration geçmişine elle kayıt basılmaz.
- Hotel işlemleri PostgreSQL transaction advisory lock altında sıralanır. Birden çok uygulama örneğinde
  aynı kilit kullanılır. Bu tek otel MVP'sinde doğruluk odaklıdır; yüksek trafikte oda bazında ölçeklenmeli.
- Veritabanına uygulama dışından doğrudan veri yazmak servis kilidini atlar. Üretimde DB seviyesinde
  exclusion constraint, least-privilege runtime rolü, ayrı migration rolü ve operasyon politikası eklenmeli.
- `hotel` şeması Data API'den saklanır; RLS ve rol grant kısıtları kurulumda uygulanır.
  ASP.NET Identity, Supabase Auth değildir. auth.uid() ile bu müşteri kimlikleri eşleştirilmez.
- Yönetici listeleri küçük eğitim verisi için tam liste döndürür. Üretimde sayfalama/limit ve sorgu
  ölçümü gerekir. Ham exception ayrıntıları kullanıcıya verilmez.
- Kayıt/girişte IP başına limit ve Identity lockout vardır. Reverse proxy kurulumunda yalnız güvenilen
  proxy için ForwardedHeaders yapılandırılmalı. DataProtection anahtarları üretimde kalıcı/korumalı tutulmalı.

Kaynaklar ve görsel bilgisi: KURULUM.md ve ASSETS.md.
