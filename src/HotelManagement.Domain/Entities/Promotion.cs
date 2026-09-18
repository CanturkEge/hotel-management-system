using HotelManagement.Domain.Common;
using HotelManagement.Domain.Enums;
namespace HotelManagement.Domain.Entities;

public class Promotion : BaseEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public PromotionKind Kind { get; set; } = PromotionKind.Percentage;
    public decimal Value { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int MinimumNights { get; set; } = 1;
    public int? UsageLimit { get; set; }
    public int TimesUsed { get; set; }
    public bool IsActive { get; set; } = true;
}
