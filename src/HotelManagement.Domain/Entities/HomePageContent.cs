using HotelManagement.Domain.Common;
namespace HotelManagement.Domain.Entities;

public class HomePageContent : BaseEntity
{
    public static readonly Guid SingletonId = Guid.Parse("03815fb6-77b7-44c2-a9c1-a632de206a4d");

    public string HeroEyebrow { get; set; } = "";
    public string HeroTitle { get; set; } = "";
    public string HeroText { get; set; } = "";
    public string HeroCaption { get; set; } = "";
    public Guid? HeroImageId { get; set; }
    public string RoomsEyebrow { get; set; } = "";
    public string RoomsTitle { get; set; } = "";
    public string RoomsText { get; set; } = "";
    public string ExperienceEyebrow { get; set; } = "";
    public string ExperienceTitle { get; set; } = "";
    public string ExperienceText { get; set; } = "";
    public string ExperienceOneTitle { get; set; } = "";
    public string ExperienceOneText { get; set; } = "";
    public string ExperienceTwoTitle { get; set; } = "";
    public string ExperienceTwoText { get; set; } = "";
    public string ExperienceThreeTitle { get; set; } = "";
    public string ExperienceThreeText { get; set; } = "";
    public string NewsEyebrow { get; set; } = "";
    public string NewsTitle { get; set; } = "";
    public string ContactEyebrow { get; set; } = "";
    public string ContactTitle { get; set; } = "";
    public string ContactText { get; set; } = "";
    public string ContactPhone { get; set; } = "";
    public string ContactEmail { get; set; } = "";
}
