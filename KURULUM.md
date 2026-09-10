# Kurulum — sırasıyla uygula

Bu rehber Windows + Visual Studio + .NET 10 içindir. Docker gerekmez.
İlk kez bu paketle çalışırken eski HotelPractice projenizi, veritabanını veya migration klasörünü silmeyin.

## 1. ZIP'i aç ve doğru projeyi seç

ZIP'e sağ tık → Tümünü ayıkla. Örneğin `C:\Projects\HotelManagement-Package` gibi ayrı bir klasör kullanın.
İçeride HotelManagement.sln, Directory.Build.props, src, tests ve scripts olmalı.

Visual Studio → Dosya → Aç → Proje/Çözüm → **HotelManagement.sln**.
Klasör Aç seçmeyin. Çözüm Gezgini'nde dört uygulama katmanı ve bir test projesi görünür.
HotelManagement.Web'e sağ tık → Başlangıç Projesi Olarak Ayarla.

Eski projede açtığınız Class1.cs, Room.cs gibi dosyaları buraya ayrıca kopyalamayın; tam hâlleri pakette var.

## 2. SDK ve terminal

Visual Studio'da Görünüm → Terminal açın. Terminal konumu `.sln` dosyasının olduğu klasör olmalı.

```powershell
dir
dotnet --version
```

`dir` sonucunda HotelManagement.sln görünmeli. SDK sürümü `10.0.xxx` olmalı.
`global.json` bu proje için .NET 10'u seçer. Yalnızca runtime yeterli değildir; **SDK** gerekir.
Yüklü değilse [.NET 10 indirme sayfası](https://dotnet.microsoft.com/download/dotnet/10.0) üzerinden SDK'yı kurun.
Visual Studio Installer'da “ASP.NET ve web geliştirme” iş yükü bulunmalı.

Önce veritabanına dokunmadan derlemeyi kontrol edebilirsiniz:

```powershell
.\scripts\Verify.ps1
```

Betik restore, build, sonra iş kuralı testlerini çalıştırır. Derleme veya test başarısızsa durur.
**Bu paketin build'i üretildiği ortamda çalıştırılamadı. Bu aşama atlanmamalı.**
Kırmızı hata varsa son uzun yığın yerine ilk asıl hata kodu ve satırını paylaşın; şifre paylaşmayın.

### PowerShell betiği güvenlik nedeniyle engellenirse

Önce scripts/Setup.ps1 ve Verify.ps1 dosyalarını Visual Studio'da okuyabilirsiniz.
Windows dosyayı internetten indirildi diye işaretlediyse dosyanın Özellikler bölümündeki “Engellemeyi kaldır”
seçeneğini yalnız bu incelenmiş dosya için kullanın. Şirketin yürütme politikası engelliyorsa IT/mentor
izni olmadan politikayı değiştirmeyin. Alternatif olarak aşağıdaki “Elle kurulum” adımlarını kullanın.

## 3. Ayrı Supabase projesi

[Supabase Dashboard](https://supabase.com/dashboard) üzerinden kendi hesabınızla giriş yapın.
New project seçin; kişisel kuruluşunuzu, uygun ücretsiz planı ve yakın bölgeyi seçin.
Proje ismi örneğin `hotel-management-dev` olabilir. Yeni bir veritabanı şifresi belirleyip saklayın.
Hazır olana kadar bekleyin. Ücretsiz planın limitleri/uyku koşulları değişebilir; panelde gösterileni esas alın.

Projenin **Connect** düğmesine basın. Bağlantı türü olarak **Session pooler** seçin.
Şunları not edin:

- Host: panelde gösterilen tam sunucu adresi.
- Port: Session pooler için 5432.
- Database: postgres.
- Username: panelde verilen `postgres.PROJE_REFERANSI` benzeri tam kullanıcı adı.
- Password: bu projenin veritabanı şifresi; Supabase hesabınızın giriş şifresi değil.

Bilgisayar IPv4 ağındaysa Session pooler kullanımı doğrudan IPv6 bağlantı sorununu önler.
Transaction pooler 6543 ile karıştırmayın. [Supabase bağlantı rehberi](https://supabase.com/docs/guides/database/connecting-to-postgres)

Supabase Auth kurulumu yapmayın. Bu uygulamanın giriş sistemi **ASP.NET Core Identity**.
publishable/anon/service_role anahtarlarına bu kurulumda ihtiyacınız yok.

## 4. Otomatik kurulum

Web uygulaması çalışıyorsa durdurun. Solution kökündeki terminalde:

```powershell
.\scripts\Setup.ps1
```

Betik sırayla şunları yapar:

1. NuGet paketlerini ve projeye özel dotnet-ef aracını yükler.
2. Tüm solution'ı derler ve iş kuralı testlerini çalıştırır; hata varsa ilerlemez.
3. Sizden host, DB kullanıcı adı ve şifresini; iki yönetici için farklı e-posta/şifreleri ister.
4. Bu bilgileri projenin dışındaki User Secrets alanına kaydeder. Şifreler terminale yazılmaz.
5. Henüz yoksa `InitialCreate` migration'ını ve model snapshot'ını oluşturur.
6. Veritabanına yazmadan önce sizden `EVET` onayı ister.
7. Migration'ı uygular, rolleri ve tek Admin + tek SuperAdmin hesabını oluşturur.
8. İlk kurulumda örnek Deluxe tipi ve 101/102 odalarını ekler; şema erişimini kısıtlar.

Kurulum hedefinizin ayrı geliştirme projesi olduğundan emin olun. Bu işlem `hotel` şeması oluşturur;
uygulamanın bu şemasındaki mevcut tabloları/migration'ları değiştirebilir. Başka uygulamanın kullandığı
bir `hotel` şemasına uygulamayın. Eski SQLite dosyalarıyla hiçbir ilişkisi yoktur.

Yönetici şifrelerinde en az 12 karakter, büyük/küçük harf, rakam ve sembol olmalı.
E-posta adresleri bu sürümde kullanıcı adı olarak kullanılır; doğrulama e-postası gönderilmez.

Başarı mesajı: `Kurulum tamamlandi. Oda ornekleri ve yonetici hesaplari hazir.`

Bağlantı/şifre düzeltip yeniden denemek güvenlidir: mevcut migration yeniden oluşturulmaz,
mevcut yönetici şifreleri değiştirilmez ve mevcut oda kayıtları silinmez.
User Secrets zaten doğruysa soruları atlamak için:

```powershell
.\scripts\Setup.ps1 -SkipSecrets
```

Not: İlk migration uygulanıp hesap oluşturma başarısız olabilir. Bu, DB'yi silme gerekçesi değildir.
Hata nedenini düzeltin ve aynı komutu tekrar çalıştırın.

## 5. Çalıştır

```powershell
dotnet run --project src/HotelManagement.Web --launch-profile http
```

Tarayıcıda [yerel siteyi](http://localhost:5080) açın.
Terminal açık kalmalı. Durdurmak için Ctrl+C. Visual Studio'dan çalıştırırken Web başlangıç projesi olsun.

HTTP profili yalnızca yerel geliştirmedir. İnternete HTTP ile yayınlamayın.
HTTPS ile yerelde kullanmak isterseniz:

```powershell
dotnet dev-certs https --trust
dotnet run --project src/HotelManagement.Web --launch-profile https
```

Sertifika güven sorusunu inceleyip onayladıktan sonra https://localhost:7080 kullanın.

## 6. İlk kullanım

1. Sayfanın altındaki Personel girişi → kurulumda seçtiğiniz süper admin e-postası ve şifresi.
2. Hesaplar & yetkiler → resepsiyon, temizlik görevlisi ve teknik servis için test hesapları açın.
3. Oda tipleri → açıklama, fiyat, kapasite, görselleri değiştirin; Fiziksel odalar → oda ekleyin.
4. Çıkış yapın. Müşteri girişi → Kayıt ol → test müşteri hesabı oluşturun.
5. Müşteriyle tarih ve oda seçip talep gönderin.
6. Personel hesabıyla Rezervasyonlar'dan onaylayın. Konaklama gününde giriş yapın, sonra çıkış yapın.
7. Temizlik panelinde yeni görev görünür; tamamlanana kadar oda yeni rezervasyona kapalıdır.
8. Müşteri, tamamlanan konaklamasına yorum yazabilir; yönetici Yorumlar ekranından yayınlar.

Müşteri ve personeli aynı anda test etmek için normal pencere + gizli pencere kullanın.
Aynı tarayıcı profilindeki sekmeler aynı oturum çerezini paylaşır.

## 7. Görseller (isteğe bağlı)

Paket özgün üretilmiş temsili oda fotoğrafıyla açılır. Gerçek otele aitmiş gibi sunmayın.
Kendi görselleriniz için Supabase Dashboard → Storage bölümünde yalnız oda tanıtım görselleri için
örneğin `room-images` adlı **public** bucket oluşturabilirsiniz. Bu bucket'ın okunması herkese açıktır;
kimlik, evrak, misafir fotoğrafı veya özel dosya koymayın.
Görseli Supabase panelinden kendiniz yükleyin, public URL'sini alın, oda tipi düzenleme ekranındaki
Görseller alanına yapıştırın. Her satıra bir URL; en fazla 8. Anahtar veya imzalı özel URL koymayın.
Uygulamanın kendisi Storage'a yükleme yapmaz. Tarayıcıya hiçbir secret/service-role anahtarı gitmez.
[Supabase public bucket erişimi](https://supabase.com/docs/guides/storage/buckets/fundamentals)

## 8. GitHub'a kaydet

Önce derleme ve TESTLER.md adımlarını tamamlayın. Sonra paketi repo klasörünüze kontrollü taşıyın;
eski dosyalarla üst üste iki solution birleştirmeyin. Önce mevcut repo durumunu commit ederek yedekleyin.
GitHub Desktop değişiklik listesinde şifre/connection string, User Secrets dosyası ve gerçek müşteri verisi
bulunmadığını kontrol edin. `.gitignore` bin/obj/.vs/secrets gibi dosyaları dışarıda bırakır.
Commit açıklaması: `Add MVC hotel management MVP`. Commit → Push origin.
GitHub Actions otomatik restore/build ve iş kuralı testlerini çalıştırır.

Bu uygulama **GitHub Pages'te çalışmaz**; C# sunucu gerektirir. Repo GitHub'da durabilir ama
MVC uygulaması .NET destekleyen bir hostta yayınlanmalıdır. Bu pakette otomatik deployment yoktur.

## 9. Sonraki model değişiklikleri

Uygulamayı durdurun ve mevcut veritabanını yedekleyin. Modeli değiştirdikten sonra:

```powershell
dotnet tool restore
dotnet ef migrations add DescribeYourChange --project src/HotelManagement.Infrastructure --startup-project src/HotelManagement.Web --output-dir Migrations
```

Oluşan migration'ın Up/Down kodunu kontrol edin. İstenmeyen DropTable/DropColumn varsa uygulamayın.
Ardından geliştirme DB'sine uygulamak için `Setup.ps1 -SkipSecrets` kullanın.
`dotnet ef database update` komutunu bu pakette doğrudan kullanmayın: DesignTimeFactory sadece
migration üretimi için sahte localhost bağlantısı taşır. Gerçek güncelleme `Web --setup` üzerinden
User Secrets bağlantısıyla yapılır.

**EnsureCreated, EnsureDeleted, migration geçmişine elle INSERT ve DB dosyası silme yok.**

## Elle kurulum (betik çalıştıramıyorsanız)

```powershell
dotnet restore HotelManagement.sln
dotnet tool restore
dotnet build HotelManagement.sln
dotnet run --project tests/HotelManagement.Tests
```

Her komut başarılı olmadan sonraki adıma geçmeyin.
Visual Studio'da **HotelManagement.Web → sağ tık → Kullanıcı Gizli Dizilerini Yönet / Manage User Secrets**.
Açılan, proje dışındaki secrets.json dosyasına aşağıdaki şemayı yazın; yer tutucuları kendiniz doldurun:

```json
{
  "ConnectionStrings:HotelDatabase": "Host=PANELDEKI_HOST;Port=5432;Database=postgres;Username=PANELDEKI_USERNAME;Password=DB_SIFRESI;SSL Mode=VerifyFull;Maximum Pool Size=10;Timeout=15",
  "Seed:SuperAdmin:Email": "SUPER_ADMIN_EPOSTANIZ",
  "Seed:SuperAdmin:Password": "KENDINIZIN_BELIRLEDIGI_GUCLU_SIFRE",
  "Seed:Admin:Email": "FARKLI_ADMIN_EPOSTANIZ",
  "Seed:Admin:Password": "KENDINIZIN_BELIRLEDIGI_FARKLI_GUCLU_SIFRE"
}
```

Bu JSON'u appsettings.json veya GitHub'a koymayın. DB şifresinde `;` veya `"` gibi karakterler varsa
bağlantı dizesi ve JSON kaçışları gerekir; otomatik betik DbConnectionStringBuilder ile bunları yönetir.
User Secrets geliştirme aracıdır, şifreli kasa değildir. [Microsoft User Secrets rehberi](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)

Yalnızca migration klasörü/snapshot henüz yoksa:

```powershell
dotnet ef migrations add InitialCreate --project src/HotelManagement.Infrastructure --startup-project src/HotelManagement.Web --output-dir Migrations
```

Sonra gerçek DB'ye yazılacağını bilerek:

```powershell
dotnet run --project src/HotelManagement.Web --launch-profile http -- --setup
dotnet run --project src/HotelManagement.Web --launch-profile http
```

## Hata tablosu

| Hata | Kontrol |
|---|---|
| Proje bulunamadı | Terminalde dir; .sln görünen klasöre geçin veya --project yolunu düzeltin. |
| SDK bulunamadı | .NET 10 SDK kurulu mu? dotnet --list-sdks ile bakın. |
| NU1101/NU1301 | NuGet kaynağı ve internet erişimi; proxy/şirket ağı varsa mentorla kontrol. |
| Şifre doğrulaması başarısız | Supabase DB şifresi + Session pooler tam Username. |
| Sunucuya ulaşılamıyor | Proje uyku/duraklatılmış mı? Host doğru mu? IPv4 için Session pooler 5432 mi? |
| SSL sertifika hatası | Host/tarih/saat ve Supabase CA zincirini kontrol edin; sertifika doğrulamasını kapatmayın. |
| relation/table does not exist | Setup başarılı mı? Aynı bağlantı mı? Migration klasörünü silmeden ilk setup hatasını inceleyin. |
| table already exists | Başka proje/elle kurulmuş şemaya bağlanıyor olabilirsiniz. DB'yi silmeyin; hedef ve migration geçmişini inceleyin. |
| Oda listesi boş | Oda tipi ve fiziksel oda satışa açık mı? Temizlik/bakım görevi var mı? |
| Giriş olmuyor | Doğru müşteri/personel sayfası ve kurulumda belirlediğiniz e-posta/şifre. Hazır admin123 yok. |
| Rol değişikliği sonrası oturum kapandı | Beklenen davranış; eski yetkiyi kullanmaması için tekrar giriş gerekir. |
| Giriş işlemi reddedildi | Rezervasyon onaylı mı? Otelin bugünü konaklama aralığında mı? Oda temiz/hazır mı? |

Üretim öncesi: TESTLER.md ve README kapsam sınırlarını mutlaka okuyun.
