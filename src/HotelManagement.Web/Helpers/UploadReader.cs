using HotelManagement.Application.Common;
using HotelManagement.Application.DTOs;
namespace HotelManagement.Web.Helpers;

public static class UploadReader
{
    public const long MaxImageBytes=5*1024*1024;
    public static async Task<MediaUploadInput?> ReadAsync(IFormFile? file,string altText="")
    {
        if(file==null || file.Length==0) return null;
        if(file.Length>MaxImageBytes) throw new AppException($"{file.FileName}: görsel 5 MB sınırını geçiyor.");
        await using var stream=file.OpenReadStream();
        using var memory=new MemoryStream((int)file.Length);
        await stream.CopyToAsync(memory);
        return new(file.FileName,file.ContentType,memory.ToArray(),altText);
    }
    public static async Task<List<MediaUploadInput>> ReadManyAsync(IEnumerable<IFormFile> files,string altText="")
    {
        var result=new List<MediaUploadInput>();
        foreach(var file in files)
        {
            var item=await ReadAsync(file,altText);
            if(item!=null) result.Add(item);
        }
        return result;
    }
}
