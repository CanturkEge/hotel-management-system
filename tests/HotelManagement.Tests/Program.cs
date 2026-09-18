using System.Linq.Expressions;
using HotelManagement.Application.Common;
using HotelManagement.Application.DTOs;
using HotelManagement.Application.Interfaces;
using HotelManagement.Application.Mapping;
using HotelManagement.Application.Services;
using HotelManagement.Domain.Common;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;
using HotelManagement.Domain.Rules;

// Dependency-free executable business-rule tests. Run with dotnet run, not dotnet test.
int passed=0;
async Task Test(string name,Func<Task> test)
{
    await test(); passed++; Console.WriteLine("PASS: "+name);
}
void Assert(bool condition){if(!condition)throw new Exception("Assertion failed");}
async Task Reject(Func<Task> operation)
{
    try{await operation();}catch(AppException){return;}
    throw new Exception("Expected AppException");
}
var day=new DateOnly(2026,9,9);
await Test("Adjacent dates do not overlap",()=> {Assert(!BookingRules.Overlaps(day,day.AddDays(2),day.AddDays(2),day.AddDays(4)));return Task.CompletedTask;});
await Test("Contained dates overlap",()=> {Assert(BookingRules.Overlaps(day,day.AddDays(5),day.AddDays(1),day.AddDays(2)));return Task.CompletedTask;});
await Test("Decimal server-side total",()=> {Assert(BookingRules.Total(day,day.AddDays(3),1250.50m)==3751.50m);return Task.CompletedTask;});
await Test("Pending requests do not hold inventory",()=> {Assert(!BookingRules.BlocksDates(ReservationStatus.Pending));return Task.CompletedTask;});
await Test("Book stores price snapshot",async ()=> {var f=new Fixture();var id=await f.Service.BookAsync(f.Customer,f.Input());var r=await f.Bookings.GetAsync(id);Assert(r!.TotalPrice==7000 && r.NightlyPrice==3500 && r.Status==ReservationStatus.Pending);});
await Test("Gold package is included in the server-side total",async ()=> {var f=new Fixture();f.Package.PricePerNight=750;var id=await f.Service.BookAsync(f.Customer,f.Input());var r=await f.Bookings.GetAsync(id);Assert(r!.RoomSubtotal==7000 && r.PackageSubtotal==1500 && r.TotalPrice==8500 && r.PackageName=="Gold");});
await Test("Inactive package cannot be booked",async ()=> {var f=new Fixture();f.Package.IsActive=false;await Reject(()=>f.Service.BookAsync(f.Customer,f.Input()));});
await Test("Booking and status changes create history",async ()=> {var f=new Fixture();var id=await f.Service.BookAsync(f.Customer,f.Input(),"Test müşteri");await f.Service.ChangeBookingAsync(id,ReservationStatus.Confirmed,null,"Test resepsiyon");var detail=await f.Service.BookingDetailsAsync(id,f.Customer);Assert(detail.Events.Count==2 && detail.Events[0].Actor=="Test müşteri" && detail.Events[1].Status==ReservationStatus.Confirmed);});
await Test("Customer can edit room dates and package before check-in",async ()=> {var f=new Fixture();var id=await f.Service.BookAsync(f.Customer,f.Input());var secondType=new RoomType{Name="Suite",Description="Test",BasePrice=5000,Capacity=3,BedCount=2,SizeInSquareMeters=40};var second=new Room{Number="202",RoomTypeId=secondType.Id,RoomType=secondType};f.Types.Add(secondType);f.Rooms.Add(second);await f.Service.UpdateBookingAsync(new(){Id=id,RoomId=second.Id,StayPackageId=f.Package.Id,CheckInDate=day.AddDays(1),CheckOutDate=day.AddDays(4),GuestCount=3,GuestName="Yeni Misafir",GuestPhone="05001112233"},f.Customer,"Müşteri");var r=await f.Bookings.GetAsync(id);Assert(r!.RoomNumber=="202"&&r.RoomSubtotal==15000&&r.TotalPrice==15000&&(await f.Events.ListAsync()).Last().Title=="Rezervasyon güncellendi");});
await Test("Completed booking cannot be edited",async ()=> {var f=new Fixture();var id=await f.Service.BookAsync(f.Customer,f.Input());(await f.Bookings.GetAsync(id))!.Status=ReservationStatus.CheckedOut;var input=new ReservationEditInput{Id=id,RoomId=f.Room.Id,StayPackageId=f.Package.Id,CheckInDate=day,CheckOutDate=day.AddDays(2),GuestCount=2,GuestName="Test",GuestPhone="05000000000"};await Reject(()=>f.Service.UpdateBookingAsync(input,f.Customer));});
await Test("Extra service updates reservation total and history",async ()=> {var f=new Fixture();var service=new ExtraService{Name="Transfer",Description="Havalimanı",Price=1200};f.ExtraServices.Add(service);var id=await f.Service.BookAsync(f.Customer,f.Input());await f.Service.AddExtraAsync(id,service.Id,2,f.Customer,"Müşteri");var r=await f.Bookings.GetAsync(id);Assert(r!.ServicesSubtotal==2400&&r.TotalPrice==9400&&(await f.ReservationExtras.ListAsync()).Single().Quantity==2);});
await Test("Promotion applies percentage discount with limit",async ()=> {var f=new Fixture();var promotion=new Promotion{Code="MERIDIAN10",Name="Test",Description="Test",Kind=PromotionKind.Percentage,Value=10,StartDate=day,EndDate=day.AddDays(30),MinimumNights=2,UsageLimit=1};f.Promotions.Add(promotion);var id=await f.Service.BookAsync(f.Customer,f.Input());await f.Service.ApplyPromotionAsync(id," meridian-10 ",f.Customer,"Müşteri");var r=await f.Bookings.GetAsync(id);Assert(r!.DiscountAmount==700&&r.TotalPrice==6300&&promotion.TimesUsed==1);});
await Test("Promotion usage limit blocks the next reservation",async ()=> {var f=new Fixture();var promotion=new Promotion{Code="SON1",Name="Son",Description="Test",Kind=PromotionKind.FixedAmount,Value=500,StartDate=day,EndDate=day.AddDays(30),UsageLimit=1,TimesUsed=1};f.Promotions.Add(promotion);var id=await f.Service.BookAsync(f.Customer,f.Input());await Reject(()=>f.Service.ApplyPromotionAsync(id,"SON1",f.Customer));});
await Test("Calendar maps active stays into each occupied day",async ()=> {var f=new Fixture();var id=await f.Service.BookAsync(f.Customer,f.Input());await f.Service.ChangeBookingAsync(id,ReservationStatus.Confirmed);var calendar=await f.Service.CalendarAsync(day);Assert(calendar.Rooms.Single().Days.Count(x=>x.Entries.Count==1)==2);});
await Test("Customer cannot view another booking detail",async ()=> {var f=new Fixture();var id=await f.Service.BookAsync(f.Customer,f.Input());await Reject(()=>f.Service.BookingDetailsAsync(id,Guid.NewGuid()));});
await Test("Past dates rejected",async ()=> {var f=new Fixture();var i=f.Input();i.CheckInDate=day.AddDays(-1);await Reject(()=>f.Service.BookAsync(f.Customer,i));});
await Test("Same-day checkout rejected",async ()=> {var f=new Fixture();var i=f.Input();i.CheckOutDate=i.CheckInDate;await Reject(()=>f.Service.BookAsync(f.Customer,i));});
await Test("Over-capacity rejected",async ()=> {var f=new Fixture();var i=f.Input();i.GuestCount=3;await Reject(()=>f.Service.BookAsync(f.Customer,i));});
await Test("Cleaning blocks booking",async ()=> {var f=new Fixture();f.Room.Status=RoomStatus.Cleaning;await Reject(()=>f.Service.BookAsync(f.Customer,f.Input()));});
await Test("Open repair blocks booking even if room flag is ready",async ()=> {var f=new Fixture();f.Jobs.Add(new RoomJob{RoomId=f.Room.Id,Kind=JobKind.Maintenance});await Reject(()=>f.Service.BookAsync(f.Customer,f.Input()));});
await Test("Duplicate pending request rejected",async ()=> {var f=new Fixture();await f.Service.BookAsync(f.Customer,f.Input());await Reject(()=>f.Service.BookAsync(f.Customer,f.Input()));});
await Test("Second approval for same dates rejected",async ()=> {var f=new Fixture();var a=await f.Service.BookAsync(f.Customer,f.Input());var b=await f.Service.BookAsync(Guid.NewGuid(),f.Input());await f.Service.ChangeBookingAsync(a,ReservationStatus.Confirmed);await Reject(()=>f.Service.ChangeBookingAsync(b,ReservationStatus.Confirmed));});
await Test("Check-in requires confirmation",async ()=> {var f=new Fixture();var id=await f.Service.BookAsync(f.Customer,f.Input());await Reject(()=>f.Service.ChangeBookingAsync(id,ReservationStatus.CheckedIn));});
await Test("Checkout opens cleaning and blocks next booking",async ()=> {var f=new Fixture();var id=await f.Service.BookAsync(f.Customer,f.Input());await f.Service.ChangeBookingAsync(id,ReservationStatus.Confirmed);await f.Service.ChangeBookingAsync(id,ReservationStatus.CheckedIn);await f.Service.ChangeBookingAsync(id,ReservationStatus.CheckedOut);Assert(f.Room.Status==RoomStatus.Cleaning);Assert((await f.Jobs.ListAsync()).Count==1);await Reject(()=>f.Service.BookAsync(f.Customer,f.Input()));});
await Test("Customer can cancel pending request on check-in day",async ()=> {var f=new Fixture();var id=await f.Service.BookAsync(f.Customer,f.Input());await f.Service.ChangeBookingAsync(id,ReservationStatus.Cancelled,f.Customer,"Test müşteri");var detail=await f.Service.BookingDetailsAsync(id,f.Customer);Assert(detail.Reservation.Status==ReservationStatus.Cancelled && detail.Events.Last().Status==ReservationStatus.Cancelled);});
await Test("Customer can cancel confirmed booking until check-in is completed",async ()=> {var f=new Fixture();var id=await f.Service.BookAsync(f.Customer,f.Input());await f.Service.ChangeBookingAsync(id,ReservationStatus.Confirmed);await f.Service.ChangeBookingAsync(id,ReservationStatus.Cancelled,f.Customer,"Test müşteri");var detail=await f.Service.BookingDetailsAsync(id,f.Customer);Assert(detail.Reservation.Status==ReservationStatus.Cancelled && detail.Events.Last().Actor=="Test müşteri");});
await Test("Cannot cancel someone else's booking",async ()=> {var f=new Fixture();var id=await f.Service.BookAsync(f.Customer,f.Input());await Reject(()=>f.Service.ChangeBookingAsync(id,ReservationStatus.Cancelled,Guid.NewGuid()));});
await Test("Cannot review without completed stay",async ()=> {var f=new Fixture();var id=await f.Service.BookAsync(f.Customer,f.Input());await Reject(()=>f.Service.ReviewAsync(f.Customer,new(){ReservationId=id,Rating=5,Comment="Guzel oda"}));});
await Test("Exactly one review per completed stay",async ()=> {var f=new Fixture();var id=await f.Service.BookAsync(f.Customer,f.Input());(await f.Bookings.GetAsync(id))!.Status=ReservationStatus.CheckedOut;var input=new ReviewInput{ReservationId=id,Rating=5,Comment="Guzel oda"};await f.Service.ReviewAsync(f.Customer,input);await Reject(()=>f.Service.ReviewAsync(f.Customer,input));Assert(!(await f.Reviews.ListAsync())[0].IsApproved);});
await Test("Reviewer ownership enforced",async ()=> {var f=new Fixture();var id=await f.Service.BookAsync(f.Customer,f.Input());(await f.Bookings.GetAsync(id))!.Status=ReservationStatus.CheckedOut;await Reject(()=>f.Service.ReviewAsync(Guid.NewGuid(),new(){ReservationId=id,Rating=5,Comment="Guzel oda"}));});
await Test("Wrong worker role cannot complete repair",async ()=> {var f=new Fixture();await f.Service.ReportJobAsync(new(){RoomId=f.Room.Id,Description="Klima bozuk",Kind=JobKind.Maintenance});var job=(await f.Jobs.ListAsync())[0];await Reject(()=>f.Service.CompleteJobAsync(job.Id,JobKind.Cleaning,"cleaner"));});
await Test("Cleaning completion does not override open repair",async ()=> {var f=new Fixture();await f.Service.ReportJobAsync(new(){RoomId=f.Room.Id,Description="Oda kirli",Kind=JobKind.Cleaning});await f.Service.ReportJobAsync(new(){RoomId=f.Room.Id,Description="Klima bozuk",Kind=JobKind.Maintenance});var job=(await f.Jobs.ListAsync())[0];await f.Service.CompleteJobAsync(job.Id,JobKind.Cleaning,"cleaner");Assert(f.Room.Status==RoomStatus.Maintenance);});
await Test("Completing all jobs releases room",async ()=> {var f=new Fixture();await f.Service.ReportJobAsync(new(){RoomId=f.Room.Id,Description="Oda kirli",Kind=JobKind.Cleaning});var job=(await f.Jobs.ListAsync())[0];await f.Service.CompleteJobAsync(job.Id,JobKind.Cleaning,"cleaner");Assert(f.Room.Status==RoomStatus.Available);});
await Test("Room with active request cannot be archived",async ()=> {var f=new Fixture();await f.Service.BookAsync(f.Customer,f.Input());await Reject(()=>f.Service.ArchiveRoomAsync(f.Room.Id));});
await Test("Bulk room creation adds sequential rooms",async ()=> {var f=new Fixture();var count=await f.Service.CreateRoomsAsync(new(){RoomTypeId=f.Type.Id,StartNumber=201,Count=3,Floor=2});Assert(count==3);var added=(await f.Rooms.ListAsync()).Where(x=>x.Number!="101").OrderBy(x=>x.Number).ToList();Assert(added.Count==3 && added[0].Number=="201" && added[2].Number=="203" && added.All(x=>x.Floor==2));});
await Test("Bulk room creation rejects duplicate batch without partial add",async ()=> {var f=new Fixture();await Reject(()=>f.Service.CreateRoomsAsync(new(){RoomTypeId=f.Type.Id,StartNumber=100,Count=3,Floor=1}));Assert((await f.Rooms.ListAsync()).Count==1);});
await Test("Room type can be saved without optional fields",async ()=> {var f=new Fixture();await f.Service.SaveTypeAsync(new(){Name="Standard",Description="Test oda",BasePrice=2500,Capacity=2,BedCount=1,SizeInSquareMeters=24});var saved=(await f.Types.ListAsync()).Single(x=>x.Name=="Standard");Assert(saved.Amenities=="" && saved.ImageUrls=="");});
await Test("Room type stores featured display order",async ()=> {var f=new Fixture();await f.Service.SaveTypeAsync(new(){Name="Suite",Description="Test oda",BasePrice=5000,Capacity=3,BedCount=2,SizeInSquareMeters=45,IsFeatured=true,FeaturedOrder=3});var saved=(await f.Types.ListAsync()).Single(x=>x.Name=="Suite");Assert(saved.IsFeatured && saved.FeaturedOrder==3);});
await Test("History keeps original room label and type",async ()=> {var f=new Fixture();var id=await f.Service.BookAsync(f.Customer,f.Input());var b=(await f.Bookings.GetAsync(id))!;f.Room.Number="999";f.Type.Name="Changed";Assert(b.ToDto(false).RoomNumber=="101" && b.ToDto(false).TypeName=="Deluxe");});
await Test("Dynamic home content has safe defaults",async ()=> {var f=new ContentFixture();var home=await f.Service.HomeAsync();Assert(home.Input.HeroTitle.Contains("Şehrin içinde") && home.HeroImageUrl==null);});
await Test("News slugs are normalized and stored",async ()=> {var f=new ContentFixture();await f.Service.SaveNewsAsync(new(){Slug="Şehirde Yeni Bir Gün!",Category="Rehber",Title="Yeni gün",Summary="Kısa bir haber özeti",Body="Yeterince uzun haber metni",PublishedAt=day,ReadingMinutes=2},null,false);Assert((await f.Articles.ListAsync()).Single().Slug=="sehirde-yeni-bir-gun");});
await Test("Fake image upload is rejected",async ()=> {var f=new ContentFixture();await Reject(()=>f.Service.UpdateRoomImagesAsync(f.Type.Id,[new("fake.png","image/png",[1,2,3,4])],[]));});
await Test("Uploaded room image is linked to its room type",async ()=> {var f=new ContentFixture();var png=new byte[]{0x89,0x50,0x4e,0x47,0x0d,0x0a,0x1a,0x0a};await f.Service.UpdateRoomImagesAsync(f.Type.Id,[new("room.png","image/png",png)],[]);Assert((await f.Images.ListAsync()).Single().RoomTypeId==f.Type.Id && (await f.Media.ListAsync()).Single().ContentType=="image/png");});
Console.WriteLine($"{passed} business-rule tests passed. PostgreSQL/Identity/browser integration is a separate manual checklist.");

sealed class MemoryRepository<T>:IRepository<T> where T:BaseEntity
{
    private readonly List<T> data=[];
    public Task<T?> GetAsync(Guid id)=>Task.FromResult(data.FirstOrDefault(x=>x.Id==id));
    public Task<List<T>> ListAsync(Expression<Func<T,bool>>? filter=null)=>Task.FromResult(filter==null?data.ToList():data.Where(filter.Compile()).ToList());
    public Task<bool> AnyAsync(Expression<Func<T,bool>> filter)=>Task.FromResult(data.Any(filter.Compile()));
    public void Add(T item)=>data.Add(item);
    public void Remove(T item)=>data.Remove(item);
}
sealed class MemoryUnit:IUnitOfWork
{
    // Does NOT simulate database locking or rollback. Tests only the service rules.
    public Task<T> LockedAsync<T>(Guid key,Func<Task<T>> operation)=>operation();
}
sealed class FixedClock:IHotelClock
{
    public DateTime UtcNow=>new(2026,9,9,10,0,0,DateTimeKind.Utc);
    public DateOnly Today=>new(2026,9,9);
}
sealed class Fixture
{
    public readonly Guid Customer=Guid.NewGuid();
    public readonly RoomType Type=new(){Name="Deluxe",Description="Test oda",BasePrice=3500,Capacity=2,BedCount=1,SizeInSquareMeters=32};
    public readonly Room Room;
    public readonly MemoryRepository<Reservation> Bookings=new();
    public readonly MemoryRepository<Review> Reviews=new();
    public readonly MemoryRepository<RoomJob> Jobs=new();
    public readonly MemoryRepository<Room> Rooms=new();
    public readonly MemoryRepository<RoomType> Types=new();
    public readonly StayPackage Package=new(){Name="Gold",Description="Test paket",Benefits="Kahvaltı",PricePerNight=0};
    public readonly MemoryRepository<StayPackage> Packages=new();
    public readonly MemoryRepository<ReservationEvent> Events=new();
    public readonly MemoryRepository<ExtraService> ExtraServices=new();
    public readonly MemoryRepository<ReservationExtra> ReservationExtras=new();
    public readonly MemoryRepository<Promotion> Promotions=new();
    public HotelService Service {get;}
    public Fixture()
    {
        Room=new(){Number="101",RoomTypeId=Type.Id,RoomType=Type};
        Types.Add(Type);
        Rooms.Add(Room);
        Packages.Add(Package);
        Service=new(Types,Rooms,Bookings,Reviews,Jobs,Packages,Events,ExtraServices,ReservationExtras,Promotions,new MemoryUnit(),new FixedClock());
    }
    public BookingInput Input()=>new(){RoomId=Room.Id,StayPackageId=Package.Id,CheckInDate=new(2026,9,9),CheckOutDate=new(2026,9,11),GuestCount=2,GuestName="Test User",GuestPhone="05000000000"};
}
sealed class ContentFixture
{
    public readonly RoomType Type=new(){Name="Deluxe",Description="Test",BasePrice=1000,Capacity=2,BedCount=1,SizeInSquareMeters=25};
    public readonly MemoryRepository<HomePageContent> Home=new();
    public readonly MemoryRepository<NewsArticle> Articles=new();
    public readonly MemoryRepository<MediaAsset> Media=new();
    public readonly MemoryRepository<RoomTypeImage> Images=new();
    public readonly MemoryRepository<RoomType> Types=new();
    public ContentService Service {get;}
    public ContentFixture(){Types.Add(Type);Service=new(Home,Articles,Media,Images,Types,new MemoryUnit(),new FixedClock());}
}
