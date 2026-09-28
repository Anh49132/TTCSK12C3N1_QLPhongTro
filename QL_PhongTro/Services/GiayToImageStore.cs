using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace QL_PhongTro.Services;

public sealed record PreparedImage(byte[] Bytes, string Extension);

public class GiayToImageStore(IWebHostEnvironment environment, IConfiguration configuration)
{
    public const long MaxBytes = 5 * 1024 * 1024;
    private readonly string root = Path.GetFullPath(configuration["IdentityImagePath"]
        ?? Path.Combine(environment.ContentRootPath, "App_Data", "identity-images"));

    public async Task<PreparedImage?> PrepareAsync(IFormFile? file)
    {
        if (file is null) return null;
        if (file.Length > MaxBytes) throw new InvalidDataException("Mỗi ảnh không được vượt quá 5MB (5.242.880 byte).");
        if (file.Length == 0) throw new InvalidDataException("Tệp ảnh rỗng, vui lòng chọn lại.");
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is not (".jpg" or ".jpeg" or ".png"))
            throw new InvalidDataException("Chỉ chấp nhận ảnh JPG hoặc PNG.");
        try
        {
            using var stream = file.OpenReadStream();
            var info = await Image.IdentifyAsync(stream);
            var format = info.Metadata.DecodedImageFormat?.Name;
            if (format is not ("JPEG" or "PNG") || (format == "PNG") != (extension == ".png"))
                throw new InvalidDataException("Nội dung tệp phải là ảnh JPG hoặc PNG đúng với phần mở rộng.");
            if ((long)info.Width * info.Height > 50_000_000)
                throw new InvalidDataException("Ảnh có độ phân giải quá lớn (tối đa 50 megapixel).");
            stream.Position = 0;
            using var image = await Image.LoadAsync(new DecoderOptions { MaxFrames = 1 }, stream);
            image.Mutate(x => x.AutoOrient());
            if (image.Width > 1600) image.Mutate(x => x.Resize(1600, 0));
            image.Metadata.ExifProfile = null;
            image.Metadata.XmpProfile = null;
            image.Metadata.IptcProfile = null;
            using var output = new MemoryStream();
            if (format == "JPEG") await image.SaveAsync(output, new JpegEncoder { Quality = 85 });
            else await image.SaveAsync(output, new PngEncoder());
            return new PreparedImage(output.ToArray(), format == "JPEG" ? ".jpg" : ".png");
        }
        catch (Exception ex) when (ex is UnknownImageFormatException or InvalidImageContentException or NotSupportedException)
        {
            throw new InvalidDataException("Tệp ảnh bị hỏng hoặc không phải ảnh JPG/PNG hợp lệ.", ex);
        }
    }

    public async Task<string> SaveAsync(PreparedImage image)
    {
        Directory.CreateDirectory(root);
        var name = Guid.NewGuid().ToString("N") + image.Extension;
        try { await File.WriteAllBytesAsync(Path.Combine(root, name), image.Bytes); }
        catch { Delete(name); throw; }
        return name;
    }

    public string? GetPath(string? name)
    {
        if (string.IsNullOrEmpty(name) || Path.GetFileName(name) != name ||
            !Guid.TryParseExact(Path.GetFileNameWithoutExtension(name), "N", out _) ||
            Path.GetExtension(name) is not (".jpg" or ".png")) return null;
        return Path.Combine(root, name);
    }

    public void Delete(string? name)
    {
        var path = GetPath(name);
        if (path is null) return;
        try { File.Delete(path); }
        catch (IOException) { /* Leave an unreferenced file for maintenance. */ }
        catch (UnauthorizedAccessException) { /* Do not undo a committed profile. */ }
    }
}
