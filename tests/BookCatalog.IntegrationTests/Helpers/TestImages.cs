using Microsoft.AspNetCore.StaticFiles;
using System.Net.Http.Headers;

namespace BookCatalog.IntegrationTests.Helpers;

public static class TestImages
{
    public const string ValidJpeg = "image1.jpeg";
    public const string ValidPng = "image2.png";
    private static readonly FileExtensionContentTypeProvider ContentTypes = new();

    public static string GetPath(string fileName) =>
        Path.Combine(AppContext.BaseDirectory, "assets", fileName);
    public static string GetContentType(string fileName)
    {
        if(!ContentTypes.TryGetContentType(fileName, out var contentType))
            contentType = "application/octet-stream";
        return contentType;
    }

    public static StreamContent LoadAsStreamContent(string fileName)
    {
        var stream = Load(fileName);
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(fileName));
        return streamContent;
    }
    public static Stream Load(string fileName) => File.OpenRead(GetPath(fileName));
}