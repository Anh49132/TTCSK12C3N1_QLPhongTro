using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace QL_PhongTro.Services;

public sealed class PasswordResetOptions
{
    public string PublicBaseUrl { get; set; } = "";
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string From { get; set; } = "";
    // Explicit local email preview only; never enabled automatically or in production.
    public string PickupDirectory { get; set; } = "";
}

public interface IPasswordEmailSender
{
    Task SendAsync(string email, string resetUrl);
}

public sealed class PasswordEmailSender(IOptions<PasswordResetOptions> options, IWebHostEnvironment environment) : IPasswordEmailSender
{
    public async Task SendAsync(string email, string resetUrl)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.From)) throw new InvalidOperationException("Missing email sender configuration.");
        using var message = new MailMessage(settings.From, email)
        {
            Subject = "Đặt lại mật khẩu Nhà Trọ",
            Body = "Bạn đã yêu cầu đặt lại mật khẩu Nhà Trọ. Mở liên kết sau trong vòng 30 phút; liên kết chỉ dùng được một lần:\n\n" + resetUrl + "\n\nNếu bạn không yêu cầu, hãy bỏ qua email này.",
            IsBodyHtml = false
        };
        using var client = new SmtpClient { Timeout = 15000 };
        if (!string.IsNullOrWhiteSpace(settings.PickupDirectory))
        {
            if (!environment.IsDevelopment()) throw new InvalidOperationException("Email pickup is development-only.");
            // Relative pickup paths use this machine's temp directory, outside the repository.
            var pickupDirectory = Path.IsPathFullyQualified(settings.PickupDirectory)
                ? settings.PickupDirectory
                : Path.GetFullPath(settings.PickupDirectory, Path.GetTempPath());
            Directory.CreateDirectory(pickupDirectory);
            client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
            client.PickupDirectoryLocation = pickupDirectory;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(settings.Host)) throw new InvalidOperationException("Missing SMTP host.");
            client.Host = settings.Host; client.Port = settings.Port; client.EnableSsl = settings.EnableSsl;
            if (!string.IsNullOrWhiteSpace(settings.Username)) client.Credentials = new NetworkCredential(settings.Username, settings.Password);
        }
        await client.SendMailAsync(message);
    }
}
