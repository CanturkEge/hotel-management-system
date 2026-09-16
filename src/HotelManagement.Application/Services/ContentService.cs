using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using HotelManagement.Application.Common;
using HotelManagement.Application.DTOs;
using HotelManagement.Application.Interfaces;
using HotelManagement.Domain.Entities;
namespace HotelManagement.Application.Services;

public sealed partial class ContentService(
    IRepository<HomePageContent> homePages, IRepository<NewsArticle> articles,
    IRepository<MediaAsset> media, IRepository<RoomTypeImage> roomImages,
    IRepository<RoomType> roomTypes, IUnitOfWork unit, IHotelClock clock) : IContentService
{
    private static readonly Guid ContentLock = Guid.Parse("a629bd52-02c4-48f2-b3a5-d3c9452f16f7");
    private const int MaxImageBytes = 5 * 1024 * 1024;
    private Task<T> Write<T>(Func<Task<T>> action) => unit.LockedAsync(ContentLock, action);

    public async Task<HomePageDto> HomeAsync()
    {
        var item=await homePages.GetAsync(HomePageContent.SingletonId);
        return item==null ? new(HomePageContent.SingletonId,Defaults(),null) : ToDto(item);
    }

    public Task SaveHomeAsync(HomePageInput input,MediaUploadInput? heroImage,bool removeHeroImage)=>Write(async()=>
    {
        Validate(input);
        var item=await homePages.GetAsync(HomePageContent.SingletonId) ?? new HomePageContent {Id=HomePageContent.SingletonId};
        if(item.CreatedAtUtc==default) item.CreatedAtUtc=clock.UtcNow;
        if(await homePages.AnyAsync(x=>x.Id==item.Id)==false) homePages.Add(item);
        Map(input,item);
        if(removeHeroImage && item.HeroImageId is Guid oldId)
        {
            var old=await media.GetAsync(oldId); if(old!=null) media.Remove(old);
            item.HeroImageId=null;
        }
        if(heroImage!=null)
        {
            var asset=CreateMedia(heroImage); media.Add(asset);
            if(item.HeroImageId is Guid previousId)
            {
                var previous=await media.GetAsync(previousId); if(previous!=null) media.Remove(previous);
            }
            item.HeroImageId=asset.Id;
        }
        return true;
    });

    public async Task<List<NewsArticleDto>> NewsAsync(bool includeUnpublished=false)
        => (await articles.ListAsync(x=>includeUnpublished || x.IsPublished))
            .OrderByDescending(x=>x.IsFeatured).ThenBy(x=>x.SortOrder).ThenByDescending(x=>x.PublishedAt)
            .Select(ToDto).ToList();

    public async Task<NewsArticleDto?> NewsBySlugAsync(string slug,bool includeUnpublished=false)
    {
        var normalized=NormalizeSlug(slug);
        return (await articles.ListAsync(x=>x.Slug==normalized && (includeUnpublished || x.IsPublished))).Select(ToDto).FirstOrDefault();
    }

    public Task SaveNewsAsync(NewsArticleInput input,MediaUploadInput? coverImage,bool removeCoverImage)=>Write(async()=>
    {
        Validate(input);
        var slug=NormalizeSlug(input.Slug);
        if(string.IsNullOrWhiteSpace(slug)) throw new AppException("Haber adresi en az bir harf veya rakam içermeli.");
        if(await articles.AnyAsync(x=>x.Slug==slug && x.Id!=input.Id)) throw new AppException("Bu haber adresi zaten kullanılıyor.");
        var item=input.Id==Guid.Empty ? new NewsArticle() : await articles.GetAsync(input.Id) ?? throw new AppException("Haber bulunamadı.");
        item.Slug=slug; item.Category=input.Category.Trim(); item.Title=input.Title.Trim(); item.Summary=input.Summary.Trim();
        item.Body=input.Body.Trim(); item.PublishedAt=input.PublishedAt; item.ReadingMinutes=input.ReadingMinutes;
        item.IsPublished=input.IsPublished; item.IsFeatured=input.IsFeatured; item.SortOrder=input.SortOrder;
        if(input.Id==Guid.Empty) articles.Add(item);
        if(removeCoverImage && item.CoverImageId is Guid oldId)
        {
            var old=await media.GetAsync(oldId); if(old!=null) media.Remove(old);
            item.CoverImageId=null;
        }
        if(coverImage!=null)
        {
            var asset=CreateMedia(coverImage); media.Add(asset);
            if(item.CoverImageId is Guid previousId)
            {
                var previous=await media.GetAsync(previousId); if(previous!=null) media.Remove(previous);
            }
            item.CoverImageId=asset.Id;
        }
        return true;
    });

    public Task SetNewsPublishedAsync(Guid id,bool published)=>Write(async()=>
    {
        var item=await articles.GetAsync(id) ?? throw new AppException("Haber bulunamadı.");
        item.IsPublished=published; return true;
    });

    public Task UpdateRoomImagesAsync(Guid roomTypeId,IReadOnlyList<MediaUploadInput> uploads,IReadOnlyCollection<Guid> removeImageIds)=>Write(async()=>
    {
        if(uploads.Count>8) throw new AppException("Tek seferde en fazla 8 görsel yükleyebilirsiniz.");
        var roomType=await roomTypes.GetAsync(roomTypeId) ?? throw new AppException("Oda tipi bulunamadı.");
        var existing=await roomImages.ListAsync(x=>x.RoomTypeId==roomTypeId);
        var removeSet=removeImageIds.ToHashSet();
        if(removeSet.Any(id=>existing.All(x=>x.Id!=id))) throw new AppException("Silinecek görsellerden biri bu oda tipine ait değil.");
        if(existing.Count-removeSet.Count+uploads.Count>8) throw new AppException("Bir oda tipinde en fazla 8 görsel olabilir.");
        foreach(var link in existing.Where(x=>removeSet.Contains(x.Id)))
        {
            var asset=await media.GetAsync(link.MediaAssetId);
            var tracked=await roomImages.GetAsync(link.Id); if(tracked!=null) roomImages.Remove(tracked);
            if(asset!=null) media.Remove(asset);
        }
        var nextOrder=existing.Where(x=>!removeSet.Contains(x.Id)).Select(x=>x.SortOrder).DefaultIfEmpty(-1).Max()+1;
        foreach(var upload in uploads)
        {
            var asset=CreateMedia(upload); media.Add(asset);
            roomImages.Add(new RoomTypeImage {RoomTypeId=roomTypeId,MediaAssetId=asset.Id,
                AltText=string.IsNullOrWhiteSpace(upload.AltText)?$"{roomType.Name} oda görseli":upload.AltText.Trim(),SortOrder=nextOrder++});
        }
        if(uploads.Count>0) roomType.ImageUrls="";
        return true;
    });

    public async Task<MediaFileDto?> MediaAsync(Guid id)
    {
        var item=await media.GetAsync(id);
        return item==null?null:new(item.Data,item.ContentType,item.FileName,item.Sha256,item.UpdatedAtUtc??item.CreatedAtUtc);
    }

    private static void Validate(object input)
    {
        var results=new List<ValidationResult>();
        if(!Validator.TryValidateObject(input,new ValidationContext(input),results,true))
            throw new AppException(string.Join(" ",results.Select(x=>x.ErrorMessage)));
    }

    private static MediaAsset CreateMedia(MediaUploadInput upload)
    {
        if(upload.Data.Length is 0 or > MaxImageBytes) throw new AppException("Görsel boş olamaz ve 5 MB sınırını geçemez.");
        var contentType=DetectImageType(upload.Data) ?? throw new AppException("Yalnızca gerçek JPEG, PNG, WebP veya AVIF görsel yükleyebilirsiniz.");
        var fileName=Path.GetFileName(upload.FileName.Trim());
        if(string.IsNullOrWhiteSpace(fileName)) fileName="image"+Extension(contentType);
        if(fileName.Length>180) fileName=fileName[..170]+Extension(contentType);
        return new MediaAsset {FileName=fileName,ContentType=contentType,Data=upload.Data,Length=upload.Data.Length,
            Sha256=Convert.ToHexString(SHA256.HashData(upload.Data)).ToLowerInvariant()};
    }

    private static string? DetectImageType(byte[] data)
    {
        if(data.Length>=8 && data.AsSpan(0,8).SequenceEqual(new byte[]{0x89,0x50,0x4e,0x47,0x0d,0x0a,0x1a,0x0a})) return "image/png";
        if(data.Length>=3 && data[0]==0xff && data[1]==0xd8 && data[2]==0xff) return "image/jpeg";
        if(data.Length>=12 && Encoding.ASCII.GetString(data,0,4)=="RIFF" && Encoding.ASCII.GetString(data,8,4)=="WEBP") return "image/webp";
        if(data.Length>=16 && Encoding.ASCII.GetString(data,4,4)=="ftyp")
        {
            var brand=Encoding.ASCII.GetString(data,8,4);
            if(brand is "avif" or "avis") return "image/avif";
        }
        return null;
    }
    private static string Extension(string contentType)=>contentType switch {"image/png"=>".png","image/jpeg"=>".jpg","image/webp"=>".webp",_=>".avif"};

    private static string NormalizeSlug(string value)
    {
        var lowered=(value??"").Trim().ToLowerInvariant()
            .Replace('ç','c').Replace('ğ','g').Replace('ı','i').Replace('ö','o').Replace('ş','s').Replace('ü','u');
        var normalized=SlugInvalid().Replace(lowered,"-").Trim('-');
        return normalized.Length<=120?normalized:normalized[..120].TrimEnd('-');
    }
    [GeneratedRegex("[^a-z0-9]+",RegexOptions.CultureInvariant)] private static partial Regex SlugInvalid();

    private static NewsArticleDto ToDto(NewsArticle x)=>new(x.Id,x.Slug,x.Category,x.Title,x.Summary,x.Body,x.PublishedAt,
        x.ReadingMinutes,x.IsPublished,x.IsFeatured,x.SortOrder,x.CoverImageId is Guid id?$"/media/{id}":null);
    private static HomePageDto ToDto(HomePageContent x)=>new(x.Id,new HomePageInput {
        HeroEyebrow=x.HeroEyebrow,HeroTitle=x.HeroTitle,HeroText=x.HeroText,HeroCaption=x.HeroCaption,
        RoomsEyebrow=x.RoomsEyebrow,RoomsTitle=x.RoomsTitle,RoomsText=x.RoomsText,
        ExperienceEyebrow=x.ExperienceEyebrow,ExperienceTitle=x.ExperienceTitle,ExperienceText=x.ExperienceText,
        ExperienceOneTitle=x.ExperienceOneTitle,ExperienceOneText=x.ExperienceOneText,
        ExperienceTwoTitle=x.ExperienceTwoTitle,ExperienceTwoText=x.ExperienceTwoText,
        ExperienceThreeTitle=x.ExperienceThreeTitle,ExperienceThreeText=x.ExperienceThreeText,
        NewsEyebrow=x.NewsEyebrow,NewsTitle=x.NewsTitle,ContactEyebrow=x.ContactEyebrow,
        ContactTitle=x.ContactTitle,ContactText=x.ContactText,ContactPhone=x.ContactPhone,ContactEmail=x.ContactEmail
    },x.HeroImageId is Guid id?$"/media/{id}":null);
    private static void Map(HomePageInput x,HomePageContent y)
    {
        y.HeroEyebrow=x.HeroEyebrow.Trim(); y.HeroTitle=x.HeroTitle.Trim(); y.HeroText=x.HeroText.Trim(); y.HeroCaption=x.HeroCaption.Trim();
        y.RoomsEyebrow=x.RoomsEyebrow.Trim(); y.RoomsTitle=x.RoomsTitle.Trim(); y.RoomsText=x.RoomsText.Trim();
        y.ExperienceEyebrow=x.ExperienceEyebrow.Trim(); y.ExperienceTitle=x.ExperienceTitle.Trim(); y.ExperienceText=x.ExperienceText.Trim();
        y.ExperienceOneTitle=x.ExperienceOneTitle.Trim(); y.ExperienceOneText=x.ExperienceOneText.Trim();
        y.ExperienceTwoTitle=x.ExperienceTwoTitle.Trim(); y.ExperienceTwoText=x.ExperienceTwoText.Trim();
        y.ExperienceThreeTitle=x.ExperienceThreeTitle.Trim(); y.ExperienceThreeText=x.ExperienceThreeText.Trim();
        y.NewsEyebrow=x.NewsEyebrow.Trim(); y.NewsTitle=x.NewsTitle.Trim(); y.ContactEyebrow=x.ContactEyebrow.Trim();
        y.ContactTitle=x.ContactTitle.Trim(); y.ContactText=x.ContactText.Trim(); y.ContactPhone=x.ContactPhone.Trim(); y.ContactEmail=x.ContactEmail.Trim();
    }
    public static HomePageInput Defaults()=>new() {
        HeroEyebrow="MERIDIAN HOTEL · İSTANBUL",HeroTitle="Şehrin içinde. Telaşın dışında.",
        HeroText="İyi tasarlanmış odalar, sade bir rezervasyon deneyimi ve şehri kendi ritminizde yaşamanız için size ait bir alan.",HeroCaption="Meridian Deluxe Oda",
        RoomsEyebrow="ODALAR & SUİTLER",RoomsTitle="Her yolculuk için bir alan.",RoomsText="İş seyahatinden hafta sonu kaçamağına, ihtiyacınız kadar sade ve konforlu.",
        ExperienceEyebrow="MERIDIAN DENEYİMİ",ExperienceTitle="Odanızdan daha fazlası.",ExperienceText="Şehrin enerjisine yakın, kalabalığın gürültüsünden uzakta. Günün her anı için düşünülmüş küçük detaylar.",
        ExperienceOneTitle="Yerel kahvaltı",ExperienceOneText="Mevsim ürünleri ve mutfağımızdan sıcak tabaklarla telaşsız sabahlar.",
        ExperienceTwoTitle="Mahalle rotaları",ExperienceTwoText="Ekibimizin seçtiği yürüyüş, lezzet ve kültür durakları.",
        ExperienceThreeTitle="Dijital misafir alanı",ExperienceThreeText="Rezervasyonunuzu, durumunu ve geçmiş konaklamalarınızı tek yerde görün.",
        NewsEyebrow="MERIDIAN JOURNAL",NewsTitle="Şehirden notlar.",ContactEyebrow="MİSAFİR İLİŞKİLERİ",
        ContactTitle="Konaklamanızı birlikte planlayalım.",ContactText="Özel bir isteğiniz veya aklınıza takılan bir şey varsa ekibimiz burada.",
        ContactPhone="+90 216 000 00 00",ContactEmail="hello@meridianhotel.example"
    };
}
