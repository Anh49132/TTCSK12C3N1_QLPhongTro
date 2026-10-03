using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace QL_PhongTro.Tests;

public sealed partial class PermissionTests
{
    [Fact]
    public async Task OwnerUploadsJpegAndPngWithOriginalAndSmallOrderedThumbnail()
    {
        var listingId = CreatePublicListing("TRONG", "DANG_HIEN_THI", includePhoto: false);
        var (roomId, _) = GetListingRoomAndBuilding(listingId);
        using var owner = await Login("CHU_NHA");
        var token = await RoomImageToken(owner, roomId);

        using var jpeg = await Upload(owner, roomId, token, "room.jpg", EncodeJpeg(1200, 800));
        Assert.Equal(HttpStatusCode.OK, jpeg.StatusCode);
        using var jpegJson = JsonDocument.Parse(await jpeg.Content.ReadAsStringAsync());
        Assert.Equal(1, jpegJson.RootElement.GetProperty("order").GetInt32());
        Assert.Equal(1, jpegJson.RootElement.GetProperty("count").GetInt32());

        using var png = await Upload(owner, roomId, token, "room.png", EncodePng(640, 480));
        Assert.Equal(HttpStatusCode.OK, png.StatusCode);
        using var pngJson = JsonDocument.Parse(await png.Content.ReadAsStringAsync());
        Assert.Equal(2, pngJson.RootElement.GetProperty("order").GetInt32());
        Assert.Equal(2, pngJson.RootElement.GetProperty("count").GetInt32());

        Assert.Equal(2L, Scalar("SELECT COUNT(*) FROM anh_phong WHERE phong_id=$room", ("$room", roomId)));
        var originalPath = jpegJson.RootElement.GetProperty("originalPath").GetString()!;
        var thumbnailPath = jpegJson.RootElement.GetProperty("thumbnailPath").GetString()!;
        using var guest = Client();
        var original = await guest.GetByteArrayAsync(originalPath);
        var thumbnail = await guest.GetByteArrayAsync(thumbnailPath);
        Assert.Equal(EncodeJpeg(1200, 800).Length, original.Length);
        Assert.True(thumbnail.Length < original.Length);
        using var thumbImage = await Image.LoadAsync(new MemoryStream(thumbnail));
        Assert.Equal(400, thumbImage.Width);
        Assert.Equal(267, thumbImage.Height);

        var edit = await owner.GetStringAsync($"/PhongTro/Edit/{roomId}");
        Assert.Contains("2/8 ảnh", edit);
        Assert.Contains("data-image-id", edit);
        CleanupRoomImages(roomId);
    }

    [Fact]
    public async Task OwnerCanUploadEightImagesButNinthIsRejected()
    {
        var listingId = CreatePublicListing("TRONG", "DANG_HIEN_THI", includePhoto: false);
        var (roomId, _) = GetListingRoomAndBuilding(listingId);
        using var owner = await Login("CHU_NHA");
        var token = await RoomImageToken(owner, roomId);
        var bytes = EncodeJpeg(64, 48);

        for (var index = 0; index < 8; index++)
        {
            using var response = await Upload(owner, roomId, token, $"room-{index}.jpg", bytes);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        using var ninth = await Upload(owner, roomId, token, "ninth.jpg", bytes);
        Assert.Equal(HttpStatusCode.Conflict, ninth.StatusCode);
        Assert.Contains("đủ 8 ảnh", await ninth.Content.ReadAsStringAsync());
        Assert.Equal(8L, Scalar("SELECT COUNT(*) FROM anh_phong WHERE phong_id=$room", ("$room", roomId)));
        CleanupRoomImages(roomId);
    }

    [Fact]
    public async Task InvalidUploadFormatsAndOversizedFilesAreRejectedWithoutRowsOrFiles()
    {
        var listingId = CreatePublicListing("TRONG", "DANG_HIEN_THI", includePhoto: false);
        var (roomId, _) = GetListingRoomAndBuilding(listingId);
        using var owner = await Login("CHU_NHA");
        var token = await RoomImageToken(owner, roomId);
        var png = EncodePng(20, 20);
        var corruptPng = png[..Math.Min(24, png.Length)];
        var executable = new byte[] { 0x4d, 0x5a, 0x90, 0x00, 0x03, 0x00 };
        var pdf = System.Text.Encoding.ASCII.GetBytes("%PDF-1.7\n1 0 obj\n<<>>\nendobj\n");
        var gif = Convert.FromBase64String("R0lGODlhAQABAAD/ACwAAAAAAQABAAACADs=");
        var tooLarge = new byte[QL_PhongTro.Services.RoomImageStore.MaxBytes + 1];
        var rejected = new (string Name, byte[] Bytes, string Message)[]
        {
            ("animation.gif", gif, "JPG hoặc PNG"),
            ("document.pdf", pdf, "JPG hoặc PNG"),
            ("payload.jpg", executable, "hỏng"),
            ("renamed.jpg", png, "không khớp"),
            ("broken.png", corruptPng, "hỏng"),
            ("large.jpg", tooLarge, "5 MB")
        };

        foreach (var (name, bytes, message) in rejected)
        {
            using var response = await Upload(owner, roomId, token, name, bytes);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(message, await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
        }

        Assert.Equal(0L, Scalar("SELECT COUNT(*) FROM anh_phong WHERE phong_id=$room", ("$room", roomId)));
        var directory = RoomImageDirectory(roomId);
        Assert.True(!Directory.Exists(directory) || Directory.GetFiles(directory).Length == 0);
    }

    [Fact]
    public async Task AnonymousOrAnotherAccountCannotUploadToRoom()
    {
        var listingId = CreatePublicListing("TRONG", "DANG_HIEN_THI", includePhoto: false);
        var (roomId, buildingId) = GetListingRoomAndBuilding(listingId);
        var bytes = EncodeJpeg(24, 24);

        using (var anonymous = Client())
        using (var response = await Upload(anonymous, roomId, string.Empty, "room.jpg", bytes))
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        using var otherOwner = await Login("CHU_NHA");
        var token = await RoomImageToken(otherOwner, roomId);
        Execute("UPDATE toa_nha SET chu_nha_id=$owner WHERE id=$building",
            ("$owner", accounts["ADMIN"]), ("$building", buildingId));
        using var forbidden = await Upload(otherOwner, roomId, token, "room.jpg", bytes);
        Assert.Equal(HttpStatusCode.NotFound, forbidden.StatusCode);
        Assert.Equal(0L, Scalar("SELECT COUNT(*) FROM anh_phong WHERE phong_id=$room", ("$room", roomId)));
        Assert.True(!Directory.Exists(RoomImageDirectory(roomId)) || Directory.GetFiles(RoomImageDirectory(roomId)).Length == 0);
    }

    private static async Task<string> RoomImageToken(HttpClient client, int roomId)
    {
        var html = await client.GetStringAsync($"/PhongTro/Edit/{roomId}");
        var token = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
        Assert.NotEmpty(token);
        return WebUtility.HtmlDecode(token);
    }

    private static async Task<HttpResponseMessage> Upload(HttpClient client, int roomId, string token, string name, byte[] bytes)
    {
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(bytes), "file", name);
        if (token.Length > 0) content.Add(new StringContent(token), "__RequestVerificationToken");
        return await client.PostAsync($"/PhongTro/UploadImage/{roomId}", content);
    }

    private string RoomImageDirectory(int roomId) => Path.Combine(appPath, "wwwroot", "uploads", "rooms", roomId.ToString());

    private void CleanupRoomImages(int roomId)
    {
        var directory = RoomImageDirectory(roomId);
        if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
    }

    private static byte[] EncodeJpeg(int width, int height)
    {
        using var image = new Image<Rgba32>(Configuration.Default, width, height, new Rgba32(210, 160, 90));
        using var output = new MemoryStream();
        image.Save(output, new JpegEncoder { Quality = 90 });
        return output.ToArray();
    }

    private static byte[] EncodePng(int width, int height)
    {
        using var image = new Image<Rgba32>(Configuration.Default, width, height, new Rgba32(70, 130, 120));
        using var output = new MemoryStream();
        image.Save(output, new PngEncoder());
        return output.ToArray();
    }
}