using System.ComponentModel.DataAnnotations;
namespace HotelManagement.Application.DTOs;

public record MediaUploadInput(string FileName, string ContentType, byte[] Data, string AltText = "");
public record MediaFileDto(byte[] Data, string ContentType, string FileName, string Sha256, DateTime? UpdatedAtUtc);

public class HomePageInput
{
    [Required, StringLength(100)] public string HeroEyebrow { get; set; } = "";
    [Required, StringLength(180)] public string HeroTitle { get; set; } = "";
    [Required, StringLength(600)] public string HeroText { get; set; } = "";
    [StringLength(160)] public string HeroCaption { get; set; } = "";
    [Required, StringLength(100)] public string RoomsEyebrow { get; set; } = "";
    [Required, StringLength(180)] public string RoomsTitle { get; set; } = "";
    [Required, StringLength(600)] public string RoomsText { get; set; } = "";
    [Required, StringLength(100)] public string ExperienceEyebrow { get; set; } = "";
    [Required, StringLength(180)] public string ExperienceTitle { get; set; } = "";
    [Required, StringLength(600)] public string ExperienceText { get; set; } = "";
    [Required, StringLength(100)] public string ExperienceOneTitle { get; set; } = "";
    [Required, StringLength(500)] public string ExperienceOneText { get; set; } = "";
    [Required, StringLength(100)] public string ExperienceTwoTitle { get; set; } = "";
    [Required, StringLength(500)] public string ExperienceTwoText { get; set; } = "";
    [Required, StringLength(100)] public string ExperienceThreeTitle { get; set; } = "";
    [Required, StringLength(500)] public string ExperienceThreeText { get; set; } = "";
    [Required, StringLength(100)] public string NewsEyebrow { get; set; } = "";
    [Required, StringLength(180)] public string NewsTitle { get; set; } = "";
    [Required, StringLength(100)] public string ContactEyebrow { get; set; } = "";
    [Required, StringLength(180)] public string ContactTitle { get; set; } = "";
    [Required, StringLength(600)] public string ContactText { get; set; } = "";
    [Required, Phone, StringLength(30)] public string ContactPhone { get; set; } = "";
    [Required, EmailAddress, StringLength(254)] public string ContactEmail { get; set; } = "";
}

public record HomePageDto(Guid Id, HomePageInput Input, string? HeroImageUrl);

public class NewsArticleInput
{
    public Guid Id { get; set; }
    [Required, StringLength(120)] public string Slug { get; set; } = "";
    [Required, StringLength(80)] public string Category { get; set; } = "";
    [Required, StringLength(180)] public string Title { get; set; } = "";
    [Required, StringLength(500)] public string Summary { get; set; } = "";
    [Required, StringLength(12000)] public string Body { get; set; } = "";
    public DateOnly PublishedAt { get; set; }
    [Range(1, 60)] public int ReadingMinutes { get; set; } = 2;
    public bool IsPublished { get; set; } = true;
    public bool IsFeatured { get; set; }
    [Range(0, 999)] public int SortOrder { get; set; }
}

public record NewsArticleDto(Guid Id, string Slug, string Category, string Title, string Summary,
    string Body, DateOnly PublishedAt, int ReadingMinutes, bool IsPublished, bool IsFeatured,
    int SortOrder, string? CoverImageUrl);
