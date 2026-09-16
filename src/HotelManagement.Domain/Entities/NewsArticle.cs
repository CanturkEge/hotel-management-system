using HotelManagement.Domain.Common;
namespace HotelManagement.Domain.Entities;

public class NewsArticle : BaseEntity
{
    public string Slug { get; set; } = "";
    public string Category { get; set; } = "";
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Body { get; set; } = "";
    public DateOnly PublishedAt { get; set; }
    public int ReadingMinutes { get; set; } = 1;
    public bool IsPublished { get; set; } = true;
    public bool IsFeatured { get; set; }
    public int SortOrder { get; set; }
    public Guid? CoverImageId { get; set; }
}
