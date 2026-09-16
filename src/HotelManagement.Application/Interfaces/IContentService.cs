using HotelManagement.Application.DTOs;
namespace HotelManagement.Application.Interfaces;

public interface IContentService
{
    Task<HomePageDto> HomeAsync();
    Task SaveHomeAsync(HomePageInput input, MediaUploadInput? heroImage, bool removeHeroImage);
    Task<List<NewsArticleDto>> NewsAsync(bool includeUnpublished = false);
    Task<NewsArticleDto?> NewsBySlugAsync(string slug, bool includeUnpublished = false);
    Task SaveNewsAsync(NewsArticleInput input, MediaUploadInput? coverImage, bool removeCoverImage);
    Task SetNewsPublishedAsync(Guid id, bool published);
    Task UpdateRoomImagesAsync(Guid roomTypeId, IReadOnlyList<MediaUploadInput> uploads, IReadOnlyCollection<Guid> removeImageIds);
    Task<MediaFileDto?> MediaAsync(Guid id);
}
