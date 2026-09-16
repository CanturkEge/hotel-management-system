using HotelManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace HotelManagement.Web.Controllers;

[AllowAnonymous]
public class MediaController(IContentService content):Controller
{
    [HttpGet("/media/{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var media=await content.MediaAsync(id);
        if(media==null)return NotFound();
        var etag='"'+media.Sha256+'"';
        if(Request.Headers.IfNoneMatch.Any(x=>x==etag))return StatusCode(StatusCodes.Status304NotModified);
        Response.Headers.ETag=etag;
        Response.Headers.CacheControl="public,max-age=86400,immutable";
        Response.Headers["X-Content-Type-Options"]="nosniff";
        return File(media.Data,media.ContentType,enableRangeProcessing:true);
    }
}
