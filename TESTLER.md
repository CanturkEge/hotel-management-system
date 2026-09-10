# Doğrulama planı

## Teslim anındaki durum

- Kaynak dosyaları, proje referansları, yapılandırma JSON/XML, controller/view bağlantıları statik incelendi.
- JavaScript için `node --check` çalıştırıldı.
- ZIP bütünlüğü kontrol edildi.
- .NET SDK, PostgreSQL ve PowerShell bu üretim ortamında yok: **C# / Razor derlemesi, 23 iş kuralı testi,
  DB migration, Identity ve tarayıcı testleri burada çalıştırılmadı.**
- Aşağıdaki adımları geçti demeden uygulamayı gerçek müşteriye yayınlamayın.

## 1. Derleme + servis testleri

```powershell
.\scripts\Verify.ps1
```

Betik başarısız olursa durur. Başarı için restore/build ve tüm iş kuralları başarılı olmalı.
Test projesi ekstra test paketi gerektirmeyen bir konsol programıdır:

```powershell
dotnet run --project tests/HotelManagement.Tests
```

`dotnet test` bu programı çalıştırmaz. GitHub Actions da `dotnet run` kullanır.
Testler bellek içi repository ile servis davranışlarını kontrol eder; PostgreSQL transaction,
RLS, gerçek concurrency, Identity cookie, CSRF veya MVC model binding yerine geçmez.

Kapsanan 23 senaryo: bitişik tarih aralığı, kesişen tarih, decimal toplam, pending kapasite,
fiyat snapshot, geçmiş tarih, sıfır gece, kapasite aşımı, temizlik kilidi, açık bakım kilidi,
tekrar pending istek, çifte onay kontrolü, onaysız check-in, checkout-temizlik,
başkasının rezervasyonunu iptal, konaklamadan yorum, tek yorum, yorum sahipliği,
yanlış görev rolü, bakım sürerken temizliğin odayı açmaması, tüm görevlerin bitmesi,
aktif rezervasyonlu odayı arşivleme ve tarihsel oda etiketlerinin korunması.

## 2. Gerçek veritabanı kontrolü

Yalnız ayrı geliştirme projesinde Setup.ps1 çalıştırın. Supabase SQL Editor'da bu salt-okunur sorgularla bakın:

```sql
select tablename, rowsecurity
from pg_tables
where schemaname = 'hotel'
order by tablename;

select count(*) as rooms from hotel."Rooms";
select count(*) as users from hotel."AspNetUsers";
select "MigrationId" from hotel."__EFMigrationsHistory";

select
  has_schema_privilege('anon', 'hotel', 'USAGE') as anon_access,
  has_schema_privilege('authenticated', 'hotel', 'USAGE') as authenticated_access;
```

İlk kurulumda 2 oda, 2 yönetici beklenir. RLS sütunu true; anon/authenticated şema erişimi false olmalı.
hotel şemasını Supabase Data API exposed schemas listesine EKLEMEYİN.
Supabase Auth tablolarında uygulama müşterilerinin görünmemesi normal; ASP.NET Identity kullanılıyor.

Setup.ps1 -SkipSecrets yeniden çalıştırıldığında oda/kullanıcı sayıları kendiliğinden artmamalı,
mevcut veriler silinmemeli. Uygulamayı kapatıp açınca kayıtlar kalmalı.

## 3. Tarayıcı iş akışı

1. Ana sayfa ve müsaitlik sayfası girişsiz açılsın. Dar mobil ekranda alanlar okunabilsin.
2. Süper admin → personel girişi → resepsiyon, temizlik, teknik servis hesapları oluşturun.
3. Admin ile oda tipi oluşturun (8'den çok veya HTTPS olmayan görsel adresi reddedilmeli).
4. Fiziksel oda ekleyin; aynı numara ikinci kez eklenmemeli.
5. Müşteri kaydı oluşturun. Personel girişinden müşteri hesabına giriş reddedilmeli ve tersi de geçerli.
6. Giriş bugün, çıkış yarın olacak şekilde talep açın; fiyat backend'deki oda tipi fiyatından gelsin.
7. Başka müşteri aynı oda için pending talep açabilir; bu talepler henüz kesinleşmiş değildir.
8. Resepsiyon ilkini onaylasın; çakışan diğerini onaylamak reddedilmeli.
9. Onaylı rezervasyona check-in yapın. Oda dolu görünsün; ikinci giriş yapılamasın.
10. Check-out yapın. Temizlik görevi oluşsun; odaya yeni rezervasyon reddedilsin.
11. Temizlik görevlisi temizlendi desin. Başka açık görev yoksa oda açılsın.
12. Teknik arıza ve temizlik görevini birlikte açın. Sadece temizliği bitirmek odayı açmamalı.
13. Teknik servis bakım görevini bitirsin. Tüm görevler bittiyse ve konaklayan yoksa oda açılsın.
14. Çıkış yapan müşteri yorum yazsın; ikinci yorum ve konaklamayan kişinin yorumu reddedilsin.
15. Yönetici yorumu yayınlayınca yalnız ilgili oda tipi sayfasında ve ana yorumlarda görünsün.
16. Tamamlanan konaklama ücretini, oda numarasını veya tipi sonradan değiştirmek eski rezervasyon
    ekranındaki ücret/etiketleri değiştirmemeli. Review oda tipi ilişkisi de korunmalı.
17. Müşteri adına personel rezervasyon oluşturabilsin; yalnız mevcut müşteri hesabı seçilebilsin.
18. Tüm rollerde çıkış butonu çalışsın. Geri tuşundan sonra korunan sayfayı yenilemek tekrar giriş istesin.

## 4. Yetki ve güvenlik

- Misafir olarak /Management/Rooms, /Users/Index, /Staff/Reservations açmak giriş ister.
- Müşteri /Users/Index veya /Jobs/Index açamaz (403 / yetkisiz sayfası).
- Cleaner /Management/Rooms açamaz; Maintenance temizlik görevini Complete ile tamamlayamaz.
- Reception görev açabilir ama Complete isteğiyle bitiremez.
- Müşteri A'nın oturumuyla B'nin rezervasyon GUID'sini göndererek Cancel/Review denemek değişiklik yapmaz.
- Sahte CustomerId, TotalPrice, Status alanları müşteri Bookings/Create POST'una eklenirse sunucu bunları kullanmaz.
- Çerez olsa da anti-forgery token'sız POST /Account/Logout veya /Management/ArchiveRoom 400 döner.
- GET /Account/Logout çıkış yapmamalı; GET /Management/ArchiveRoom kayıt değiştirmemeli.
- Üst üste yanlış şifre denemelerinde Identity kilidi; dakikada yoğun isteklerde 429 beklenir.
- Süper admin rolü üçüncü hesaba atanamaz; tek Admin/SuperAdmin rolü panelden düşürülemez.
- Personel yetkisi değişince eski oturum bir sonraki istekte geçersiz olmalı.
- Yorum/metin alanında `<script>alert(1)</script>` çalışmamalı; Razor tarafından metin olarak kodlanmalı.

## 5. Gerçek eşzamanlılık kontrolü

Ayrı PostgreSQL test verisiyle iki resepsiyon oturumunda aynı oda/tarih için iki pending talebi
eşzamanlı onaylayın. Yalnız biri Confirmed olmalı. İstekler farklı sunucu süreçlerine gelse de
transaction advisory lock aynı sabiti kullanır. Bu test burada çalıştırılmadı.
SQL Editor'dan elle rezervasyon yazmak bu kilidi atlar; normal kullanım yöntemi değildir.

## Yayın öncesi ayrıca

HTTPS, güvenli kalıcı DataProtection anahtarları, gerçek domain AllowedHosts, güvenilen reverse proxy,
ayrı migration/runtime DB yetkileri, e-posta doğrulama/şifre kurtarma, yedekleme-geri yükleme testi,
log/izleme, sayfalama, API/işlem limitleri ve gerçek kişisel verilerin saklanma politikası tamamlanmalı.
Ödeme alınacaksa ödeme sağlayıcısı ayrı tasarlanmalı; bu proje ödeme almaz.
