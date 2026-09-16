using HotelManagement.Domain.Common;
namespace HotelManagement.Domain.Entities;

public class MediaAsset : BaseEntity
{
    public string FileName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public byte[] Data { get; set; } = [];
    public int Length { get; set; }
    public string Sha256 { get; set; } = "";
}
