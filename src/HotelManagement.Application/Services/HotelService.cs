using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common;
using HotelManagement.Application.DTOs;
using HotelManagement.Application.Interfaces;
using HotelManagement.Application.Mapping;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;
using HotelManagement.Domain.Rules;
namespace HotelManagement.Application.Services;

public sealed class HotelService(
    IRepository<RoomType> types, IRepository<Room> rooms, IRepository<Reservation> reservations,
    IRepository<Review> reviews, IRepository<RoomJob> jobs, IRepository<StayPackage> packages,
    IRepository<ReservationEvent> reservationEvents, IUnitOfWork unit, IHotelClock clock) : IHotelService
{
    // PostgreSQL transaction lock: all hotel writes use the same lock, even across server instances.
    // Simple correctness-first design for one hotel. Split into room-level locks when scaling.
    private static readonly Guid HotelLock = Guid.Parse("64246550-0a5e-4f2f-ae24-44ab30a68330");
    private Task<T> Write<T>(Func<Task<T>> action) => unit.LockedAsync(HotelLock, action);
    private static void Validate(object input)
    {
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(input, new ValidationContext(input), results, true))
            throw new AppException(string.Join(" ", results.Select(x => x.ErrorMessage)));
    }
    private void ValidateDates(DateOnly start, DateOnly end, int guests)
    {
        if (start < clock.Today || end <= start || end.DayNumber-start.DayNumber > 30 || start > clock.Today.AddYears(1))
            throw new AppException("Giriş bugünden önce olamaz. 1–30 gece ve en fazla 1 yıl sonrası için seçim yapın.");
        if (guests is < 1 or > 20) throw new AppException("Misafir sayısı 1–20 olmalı.");
    }
    public async Task<List<RoomTypeDto>> TypesAsync(bool includeInactive = false)
        => (await types.ListAsync(x => includeInactive || x.IsActive)).OrderBy(x=>x.BasePrice).Select(x=>x.ToDto()).ToList();

    public Task<Guid> SaveTypeAsync(RoomTypeInput input) => Write(async () =>
    {
        Validate(input);
        var item = input.Id == Guid.Empty ? new RoomType() : await types.GetAsync(input.Id) ?? throw new AppException("Oda tipi bulunamadı.");
        if (await reservations.AnyAsync(r => r.RoomTypeId == item.Id && (r.CheckOutDate > clock.Today || r.Status==ReservationStatus.CheckedIn)
            && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.CheckedIn)
            && (!input.IsActive || r.GuestCount > input.Capacity)))
            throw new AppException("Aktif rezervasyon var; oda tipini kapatamaz veya kapasiteyi bu kadar düşüremezsiniz.");
        item.Name=input.Name.Trim(); item.Description=input.Description.Trim(); item.BasePrice=input.BasePrice;
        item.Capacity=input.Capacity; item.BedCount=input.BedCount; item.SizeInSquareMeters=input.SizeInSquareMeters;
        item.Amenities=(input.Amenities ?? "").Trim(); item.IsActive=input.IsActive;
        item.IsFeatured=input.IsFeatured; item.FeaturedOrder=input.FeaturedOrder;
        if (input.Id == Guid.Empty) types.Add(item);
        return item.Id;
    });

    public async Task<List<RoomDto>> RoomsAsync(bool includeInactive=false)
        => (await rooms.ListAsync(x=>includeInactive || (x.IsActive && x.RoomType.IsActive))).OrderBy(x=>x.Number).Select(x=>x.ToDto()).ToList();

    public Task SaveRoomAsync(RoomInput input) => Write(async () =>
    {
        Validate(input);
        var type=await types.GetAsync(input.RoomTypeId) ?? throw new AppException("Oda tipi bulunamadı.");
        if(!type.IsActive) throw new AppException("Pasif oda tipi seçilemez.");
        if(await rooms.AnyAsync(x=>x.Number==input.Number.Trim() && x.Id!=input.Id)) throw new AppException("Bu oda numarası zaten var.");
        var room=input.Id==Guid.Empty ? new Room() : await rooms.GetAsync(input.Id) ?? throw new AppException("Oda bulunamadı.");
        if(input.Id!=Guid.Empty && (input.RoomTypeId!=room.RoomTypeId || !input.IsActive) && await HasFutureAsync(room.Id))
            throw new AppException("Odanın aktif rezervasyonu var. Tipini değiştiremez veya pasife alamazsınız.");
        room.Number=input.Number.Trim(); room.Floor=input.Floor; room.RoomTypeId=input.RoomTypeId; room.IsActive=input.IsActive;
        if(input.Id==Guid.Empty) rooms.Add(room);
        return true;
    });

    public Task<int> CreateRoomsAsync(BulkRoomInput input) => Write(async () =>
    {
        Validate(input);
        var type=await types.GetAsync(input.RoomTypeId) ?? throw new AppException("Oda tipi bulunamadı.");
        if(!type.IsActive) throw new AppException("Pasif oda tipi için oda oluşturulamaz.");

        var lastNumber=(long)input.StartNumber+input.Count-1;
        if(lastNumber>999999) throw new AppException("Son oda numarası 999999 değerini geçemez.");
        var numbers=Enumerable.Range(input.StartNumber,input.Count).Select(x=>x.ToString()).ToArray();
        var duplicates=(await rooms.ListAsync(x=>numbers.Contains(x.Number))).Select(x=>x.Number).OrderBy(x=>x).ToArray();
        if(duplicates.Length>0)
            throw new AppException($"Şu oda numaraları zaten var: {string.Join(", ",duplicates)}. Hiçbir oda eklenmedi.");

        foreach(var number in numbers)
            rooms.Add(new Room {Number=number,Floor=input.Floor,RoomTypeId=input.RoomTypeId});
        return numbers.Length;
    });
    private Task<bool> HasFutureAsync(Guid id) => reservations.AnyAsync(r=>r.RoomId==id &&
        (r.Status==ReservationStatus.CheckedIn || ((r.Status==ReservationStatus.Pending || r.Status==ReservationStatus.Confirmed) && r.CheckOutDate>clock.Today)));

    public Task ArchiveRoomAsync(Guid id) => Write(async () =>
    {
        var room=await rooms.GetAsync(id) ?? throw new AppException("Oda bulunamadı.");
        if(await HasFutureAsync(id)) throw new AppException("Aktif rezervasyonu olan oda kaldırılamaz.");
        room.IsActive=false; return true;
    });

    private async Task EnsureBookable(Room room, DateOnly start, DateOnly end, int guests, Guid? exclude=null)
    {
        if(!room.IsActive || !room.RoomType.IsActive || guests>room.RoomType.Capacity)
            throw new AppException("Oda aktif değil veya kapasitesi yetersiz.");
        if(room.Status is RoomStatus.Cleaning or RoomStatus.Maintenance or RoomStatus.OutOfService ||
            await jobs.AnyAsync(j=>j.RoomId==room.Id && j.CompletedAtUtc==null))
            throw new AppException("Oda temizlik/bakım işlemleri tamamlanana kadar rezervasyona kapalı.");
        if(await reservations.AnyAsync(r=>r.RoomId==room.Id && r.Id!=exclude &&
            (r.Status==ReservationStatus.Confirmed || r.Status==ReservationStatus.CheckedIn) && r.CheckInDate<end && r.CheckOutDate>start))
            throw new AppException("Bu oda seçilen tarihlerde dolu. Başka oda veya tarih seçin.");
    }
    public async Task<List<RoomDto>> AvailableAsync(DateOnly start, DateOnly end, int guests)
    {
        ValidateDates(start,end,guests);
        var blocked=(await reservations.ListAsync(r=>(r.Status==ReservationStatus.Confirmed || r.Status==ReservationStatus.CheckedIn)
            && r.CheckInDate<end && r.CheckOutDate>start)).Select(x=>x.RoomId).ToHashSet();
        blocked.UnionWith((await jobs.ListAsync(j=>j.CompletedAtUtc==null)).Select(x=>x.RoomId));
        return (await rooms.ListAsync(r=>r.IsActive && r.RoomType.IsActive && r.RoomType.Capacity>=guests
            && (r.Status==RoomStatus.Available || r.Status==RoomStatus.Occupied)))
            .Where(r=>!blocked.Contains(r.Id)).OrderBy(r=>r.RoomType.BasePrice).Select(r=>r.ToDto()).ToList();
    }
    public async Task<List<BusyPeriod>> BusyAsync(Guid roomId)
        => (await reservations.ListAsync(r=>r.RoomId==roomId && r.CheckOutDate>clock.Today &&
            (r.Status==ReservationStatus.Confirmed || r.Status==ReservationStatus.CheckedIn)))
            .OrderBy(r=>r.CheckInDate).Select(r=>new BusyPeriod(r.CheckInDate,r.CheckOutDate)).ToList();

    public async Task<List<StayPackageDto>> PackagesAsync(bool includeInactive=false)
        => (await packages.ListAsync(x=>includeInactive || x.IsActive)).OrderBy(x=>x.SortOrder).ThenBy(x=>x.PricePerNight).Select(x=>x.ToDto()).ToList();

    public Task<Guid> SavePackageAsync(StayPackageInput input) => Write(async () =>
    {
        Validate(input);
        var normalized=input.Name.Trim();
        if((await packages.ListAsync()).Any(x=>x.Id!=input.Id && x.Name.Equals(normalized,StringComparison.OrdinalIgnoreCase)))
            throw new AppException("Bu paket adı zaten kullanılıyor.");
        var item=input.Id==Guid.Empty ? new StayPackage() : await packages.GetAsync(input.Id) ?? throw new AppException("Paket bulunamadı.");
        if(input.Id!=Guid.Empty && !input.IsActive && await reservations.AnyAsync(r=>r.StayPackageId==input.Id && r.CheckOutDate>clock.Today &&
            (r.Status==ReservationStatus.Pending || r.Status==ReservationStatus.Confirmed || r.Status==ReservationStatus.CheckedIn)))
            throw new AppException("Aktif rezervasyonlarda kullanılan paket kapatılamaz.");
        item.Name=normalized; item.Description=input.Description.Trim(); item.Benefits=input.Benefits.Trim();
        item.PricePerNight=input.PricePerNight; item.SortOrder=input.SortOrder; item.IsActive=input.IsActive;
        if(input.Id==Guid.Empty) packages.Add(item);
        return item.Id;
    });

    public Task<Guid> BookAsync(Guid customerId, BookingInput input, string actor="Müşteri") => Write(async () =>
    {
        Validate(input); ValidateDates(input.CheckInDate,input.CheckOutDate,input.GuestCount);
        var room=await rooms.GetAsync(input.RoomId) ?? throw new AppException("Oda bulunamadı.");
        var package=await packages.GetAsync(input.StayPackageId) ?? throw new AppException("Konaklama paketi seçin.");
        if(!package.IsActive) throw new AppException("Seçilen paket artık kullanılamıyor.");
        await EnsureBookable(room,input.CheckInDate,input.CheckOutDate,input.GuestCount);
        if(await reservations.AnyAsync(r=>r.CustomerId==customerId && r.RoomId==input.RoomId && r.Status==ReservationStatus.Pending
            && r.CheckInDate==input.CheckInDate && r.CheckOutDate==input.CheckOutDate))
            throw new AppException("Bu tarihler için zaten bekleyen talebiniz var.");
        var roomSubtotal=BookingRules.Total(input.CheckInDate,input.CheckOutDate,room.RoomType.BasePrice);
        var packageSubtotal=BookingRules.Total(input.CheckInDate,input.CheckOutDate,package.PricePerNight);
        var booking=new Reservation { CustomerId=customerId,RoomId=input.RoomId,RoomTypeId=room.RoomTypeId,RoomTypeName=room.RoomType.Name,RoomNumber=room.Number,CheckInDate=input.CheckInDate,CheckOutDate=input.CheckOutDate,
            GuestCount=input.GuestCount,GuestName=input.GuestName.Trim(),GuestPhone=input.GuestPhone.Trim(),NightlyPrice=room.RoomType.BasePrice,
            StayPackageId=package.Id,PackageName=package.Name,PackageDescription=package.Description,PackageBenefits=package.Benefits,PackagePricePerNight=package.PricePerNight,
            RoomSubtotal=roomSubtotal,PackageSubtotal=packageSubtotal,TotalPrice=roomSubtotal+packageSubtotal };
        reservations.Add(booking);
        reservationEvents.Add(new ReservationEvent {ReservationId=booking.Id,Status=booking.Status,Title="Rezervasyon talebi oluşturuldu",
            Description=$"Oda {booking.RoomNumber} ve {booking.PackageName} paketi seçildi.",Actor=CleanActor(actor)});
        return booking.Id;
    });
    public async Task<List<ReservationDto>> BookingsAsync(Guid? customerId=null)
    {
        var reviewed=(await reviews.ListAsync(r=>customerId==null || r.Reservation.CustomerId==customerId)).Select(x=>x.ReservationId).ToHashSet();
        return (await reservations.ListAsync(r=>customerId==null || r.CustomerId==customerId)).OrderByDescending(r=>r.CreatedAtUtc)
            .Select(r=>r.ToDto(reviewed.Contains(r.Id))).ToList();
    }
    public async Task<ReservationDetailsDto> BookingDetailsAsync(Guid id,Guid? customerId=null)
    {
        var booking=await reservations.GetAsync(id) ?? throw new AppException("Rezervasyon bulunamadı.");
        if(customerId!=null && booking.CustomerId!=customerId) throw new AppException("Bu rezervasyonu görüntüleme yetkiniz yok.");
        var reviewed=await reviews.AnyAsync(r=>r.ReservationId==id);
        var history=(await reservationEvents.ListAsync(x=>x.ReservationId==id)).OrderBy(x=>x.CreatedAtUtc).Select(x=>x.ToDto()).ToList();
        return new(booking.ToDto(reviewed),history);
    }
    public Task ChangeBookingAsync(Guid id, ReservationStatus target, Guid? customerId=null,string actor="Sistem") => Write(async () =>
    {
        var booking=await reservations.GetAsync(id) ?? throw new AppException("Rezervasyon bulunamadı.");
        if(customerId!=null && (booking.CustomerId!=customerId || target!=ReservationStatus.Cancelled)) throw new AppException("Bu işlem için yetkiniz yok.");
        var room=await rooms.GetAsync(booking.RoomId) ?? throw new AppException("Oda bulunamadı.");
        switch(target)
        {
            case ReservationStatus.Confirmed:
                if(booking.Status!=ReservationStatus.Pending) throw new AppException("Sadece bekleyen talep onaylanabilir.");
                ValidateDates(booking.CheckInDate,booking.CheckOutDate,booking.GuestCount);
                await EnsureBookable(room,booking.CheckInDate,booking.CheckOutDate,booking.GuestCount,booking.Id);
                break;
            case ReservationStatus.Rejected:
                if(booking.Status!=ReservationStatus.Pending) throw new AppException("Sadece bekleyen talep reddedilebilir.");
                break;
            case ReservationStatus.Cancelled:
                if(booking.Status is not (ReservationStatus.Pending or ReservationStatus.Confirmed)) throw new AppException("Bu rezervasyon iptal edilemez.");
                if(customerId!=null && booking.CheckInDate<=clock.Today) throw new AppException("Giriş günü iptal için resepsiyona başvurun.");
                break;
            case ReservationStatus.CheckedIn:
                if(booking.Status!=ReservationStatus.Confirmed || clock.Today<booking.CheckInDate || clock.Today>=booking.CheckOutDate)
                    throw new AppException("Giriş yalnızca onaylı rezervasyonun konaklama tarihlerinde yapılabilir.");
                if(room.Status!=RoomStatus.Available) throw new AppException("Oda henüz girişe hazır değil.");
                await EnsureBookable(room,booking.CheckInDate,booking.CheckOutDate,booking.GuestCount,booking.Id);
                room.Status=RoomStatus.Occupied; booking.ActualCheckInUtc=clock.UtcNow;
                break;
            case ReservationStatus.CheckedOut:
                if(booking.Status!=ReservationStatus.CheckedIn) throw new AppException("Önce giriş yapılmalı.");
                booking.ActualCheckOutUtc=clock.UtcNow;
                jobs.Add(new RoomJob {RoomId=room.Id,Kind=JobKind.Cleaning,Description="Misafir çıkışı sonrası oda temizliği"});
                room.Status=await jobs.AnyAsync(j=>j.RoomId==room.Id && j.Kind==JobKind.Maintenance && j.CompletedAtUtc==null)
                    ? RoomStatus.Maintenance : RoomStatus.Cleaning;
                break;
            default: throw new AppException("Geçersiz durum geçişi.");
        }
        booking.Status=target;
        reservationEvents.Add(new ReservationEvent {ReservationId=booking.Id,Status=target,Title=EventTitle(target),
            Description=EventDescription(target),Actor=CleanActor(actor)});
        return true;
    });

    private static string CleanActor(string actor)=>string.IsNullOrWhiteSpace(actor)?"Sistem":actor.Trim()[..Math.Min(actor.Trim().Length,120)];
    private static string EventTitle(ReservationStatus status)=>status switch {
        ReservationStatus.Confirmed=>"Rezervasyon onaylandı",ReservationStatus.Rejected=>"Rezervasyon reddedildi",
        ReservationStatus.Cancelled=>"Rezervasyon iptal edildi",ReservationStatus.CheckedIn=>"Otele giriş yapıldı",
        ReservationStatus.CheckedOut=>"Konaklama tamamlandı",_=>"Rezervasyon güncellendi"};
    private static string EventDescription(ReservationStatus status)=>status switch {
        ReservationStatus.Confirmed=>"Talep resepsiyon tarafından onaylandı ve oda kesinleştirildi.",
        ReservationStatus.Rejected=>"Rezervasyon talebi resepsiyon tarafından reddedildi.",
        ReservationStatus.Cancelled=>"Rezervasyon iptal edilerek oda yeniden müsait hale getirildi.",
        ReservationStatus.CheckedIn=>"Misafirin giriş işlemi tamamlandı ve oda dolu durumuna alındı.",
        ReservationStatus.CheckedOut=>"Çıkış işlemi tamamlandı ve oda temizlik sırasına alındı.",
        _=>"Rezervasyon durumu güncellendi."};

    public Task ReviewAsync(Guid customerId, ReviewInput input) => Write(async () =>
    {
        Validate(input);
        var booking=await reservations.GetAsync(input.ReservationId) ?? throw new AppException("Rezervasyon bulunamadı.");
        if(booking.CustomerId!=customerId || booking.Status!=ReservationStatus.CheckedOut)
            throw new AppException("Yalnızca kendi tamamlanan konaklamanıza yorum yazabilirsiniz.");
        if(await reviews.AnyAsync(r=>r.ReservationId==booking.Id)) throw new AppException("Bu konaklama için zaten yorum yazdınız.");
        reviews.Add(new Review {ReservationId=booking.Id,Rating=input.Rating,Comment=input.Comment.Trim()}); return true;
    });
    public async Task<List<ReviewDto>> ReviewsAsync(bool includeUnapproved=false)
        => (await reviews.ListAsync(r=>includeUnapproved || r.IsApproved)).OrderByDescending(x=>x.CreatedAtUtc).Select(x=>x.ToDto()).ToList();
    public Task ModerateAsync(Guid id,bool approve) => Write(async () =>
    {
        var review=await reviews.GetAsync(id) ?? throw new AppException("Yorum bulunamadı.");
        review.IsApproved=approve; return true;
    });
    public async Task<List<JobDto>> JobsAsync(JobKind? kind=null)
        => (await jobs.ListAsync(j=>kind==null || j.Kind==kind)).OrderBy(j=>j.CompletedAtUtc!=null).ThenByDescending(j=>j.CreatedAtUtc).Select(j=>j.ToDto()).ToList();

    public Task ReportJobAsync(JobInput input) => Write(async () =>
    {
        Validate(input);
        if(!Enum.IsDefined(input.Kind)) throw new AppException("Geçersiz görev türü.");
        var room=await rooms.GetAsync(input.RoomId) ?? throw new AppException("Oda bulunamadı.");
        if(!room.IsActive) throw new AppException("Pasif odaya görev açılamaz.");
        jobs.Add(new RoomJob {RoomId=room.Id,Kind=input.Kind,Description=input.Description.Trim()});
        if(room.Status!=RoomStatus.Occupied)
            room.Status=input.Kind==JobKind.Maintenance || await jobs.AnyAsync(j=>j.RoomId==room.Id && j.Kind==JobKind.Maintenance && j.CompletedAtUtc==null)
                ? RoomStatus.Maintenance : RoomStatus.Cleaning;
        return true;
    });
    public Task CompleteJobAsync(Guid id,JobKind? allowedKind,string actor) => Write(async () =>
    {
        var job=await jobs.GetAsync(id) ?? throw new AppException("Görev bulunamadı.");
        if(allowedKind!=null && job.Kind!=allowedKind) throw new AppException("Bu görev rolünüze ait değil.");
        if(job.CompletedAtUtc!=null) throw new AppException("Görev zaten tamamlandı.");
        var room=await rooms.GetAsync(job.RoomId) ?? throw new AppException("Oda bulunamadı.");
        job.CompletedAtUtc=clock.UtcNow; job.CompletedBy=actor;
        if(await reservations.AnyAsync(r=>r.RoomId==room.Id && r.Status==ReservationStatus.CheckedIn)) room.Status=RoomStatus.Occupied;
        else if(await jobs.AnyAsync(j=>j.Id!=id && j.RoomId==room.Id && j.CompletedAtUtc==null && j.Kind==JobKind.Maintenance)) room.Status=RoomStatus.Maintenance;
        else if(await jobs.AnyAsync(j=>j.Id!=id && j.RoomId==room.Id && j.CompletedAtUtc==null)) room.Status=RoomStatus.Cleaning;
        else room.Status=RoomStatus.Available;
        return true;
    });
    public async Task<DashboardDto> DashboardAsync()
    {
        var allRooms=await rooms.ListAsync(r=>r.IsActive); var allBookings=await reservations.ListAsync(); var openJobs=await jobs.ListAsync(j=>j.CompletedAtUtc==null);
        return new(allRooms.Count,allRooms.Count(r=>r.Status==RoomStatus.Occupied),allBookings.Count(r=>r.Status==ReservationStatus.Pending),
            openJobs.Count,allBookings.Where(r=>r.Status==ReservationStatus.CheckedOut).Sum(r=>r.TotalPrice));
    }
}
