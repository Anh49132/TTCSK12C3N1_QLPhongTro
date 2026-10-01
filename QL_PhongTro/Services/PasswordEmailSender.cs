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

public interface ITemporaryPasswordEmailSender
{
    Task SendTemporaryAsync(string email, string name, string role, string password);
}

public interface IRegistrationEmailSender
{
    Task<EmailDeliveryMode> SendConfirmationAsync(string email, string name, string code);
}

public enum EmailDeliveryMode { Pickup, Smtp }

public sealed class PasswordEmailSender(IOptions<PasswordResetOptions> options, IWebHostEnvironment environment) : IPasswordEmailSender, ITemporaryPasswordEmailSender, IRegistrationEmailSender
{
    public Task SendAsync(string email, string resetUrl) => DeliverAsync(email, "Đặt lại mật khẩu Nhà Trọ",
        "Bạn đã yêu cầu đặt lại mật khẩu Nhà Trọ. Mở liên kết sau trong vòng 30 phút; liên kết chỉ dùng được một lần:\n\n" + resetUrl + "\n\nNếu bạn không yêu cầu, hãy bỏ qua email này.");

    public Task SendTemporaryAsync(string email, string name, string role, string password)
    {
        var url = options.Value.PublicBaseUrl;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var origin) ||
            (origin.Scheme != "https" && !(environment.IsDevelopment() && origin.Scheme == "http" && origin.IsLoopback)) ||
            !string.IsNullOrEmpty(origin.UserInfo) || !string.IsNullOrEmpty(origin.Query) || !string.IsNullOrEmpty(origin.Fragment))
            throw new InvalidOperationException("Configure a trusted PublicBaseUrl.");
        return DeliverAsync(email, "Tài khoản Nhà Trọ – yêu cầu đổi mật khẩu lần đầu",
            $"Xin chào {name},\n\nQuản trị viên đã cấp tài khoản Nhà Trọ cho bạn.\nVai trò: {role}\nEmail đăng nhập: {email}\nMật khẩu tạm: {password}\nĐăng nhập: {url.TrimEnd('/')}/Account/Login\n\nBạn bắt buộc đặt mật khẩu mới ngay lần đăng nhập đầu tiên trước khi sử dụng hệ thống. Không chia sẻ mật khẩu này. Nếu bạn không yêu cầu tài khoản, hãy liên hệ quản trị viên.");
    }

    public async Task<EmailDeliveryMode> SendConfirmationAsync(string email, string name, string code)
    {
        await DeliverAsync(email, "Xác nhận tài khoản Nhà Trọ",
            $"Xin chào {name},\n\nMã xác nhận tài khoản của bạn là: {code}\n\nMã có hiệu lực trong 15 phút. Nếu bạn không thực hiện đăng ký, hãy bỏ qua email này.");
        return string.IsNullOrWhiteSpace(options.Value.PickupDirectory) ? EmailDeliveryMode.Smtp : EmailDeliveryMode.Pickup;
    }

    private async Task DeliverAsync(string email, string subject, string body)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.From)) throw new InvalidOperationException("Missing email sender configuration.");
        using var message = new MailMessage(settings.From, email)
        {
            Subject = subject,
            Body = body,
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
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        await client.SendMailAsync(message, timeout.Token);
    }
}
