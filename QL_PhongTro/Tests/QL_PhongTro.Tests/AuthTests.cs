using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace QL_PhongTro.Tests;

public class AuthTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly AppDbContext _dbContext;
    private readonly HttpClient _client;

    public AuthTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();

        var scope = _factory.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    private async Task<(int statusCode, JsonElement body)> PostJsonAsync(string url, object body)
    {
        var json = JsonSerializer.Serialize(body);
        var response = await _client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
        var statusCode = (int)response.StatusCode;
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return (statusCode, doc.RootElement);
    }

    private TaiKhoan CreateUser(string email = "test@example.com", string phone = "0901234567", string password = "Test123456")
    {
        var user = new TaiKhoan
        {
            HoTen = "Test User",
            Email = email,
            SoDienThoai = phone,
            MatKhau = BCrypt.Net.BCrypt.HashPassword(password),
            VaiTro = "KHACH_THUE",
            DangHoatDong = true,
            IsStaff = false,
            IsSuperuser = false,
            FailedLoginCount = 0,
            NgayTao = DateTime.UtcNow,
            NgayCapNhat = DateTime.UtcNow
        };
        _dbContext.TaiKhoans.Add(user);
        _dbContext.SaveChanges();
        return user;
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokens()
    {
        CreateUser();
        var (statusCode, body) = await PostJsonAsync("/api/auth/login", new
        {
            Email = "test@example.com",
            MatKhau = "Test123456"
        });

        Assert.Equal(200, statusCode);
        Assert.True(body.TryGetProperty("accessToken", out _));
        Assert.True(body.TryGetProperty("refreshToken", out _));
        Assert.True(body.TryGetProperty("userId", out _));
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        CreateUser();
        var (statusCode, body) = await PostJsonAsync("/api/auth/login", new
        {
            Email = "test@example.com",
            MatKhau = "WrongPassword1"
        });

        Assert.Equal(401, statusCode);
        Assert.Equal("LOGIN_FAILED", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Login_WrongEmailOrPhone_ReturnsGenericError()
    {
        CreateUser();
        var (statusCode, body) = await PostJsonAsync("/api/auth/login", new
        {
            Email = "nonexistent@example.com",
            MatKhau = "Test123456"
        });

        Assert.Equal(401, statusCode);
        Assert.Equal("LOGIN_FAILED", body.GetProperty("error").GetString());
        Assert.Contains("Sai", body.GetProperty("message").GetString());
    }

    [Fact]
    public async Task Login_FailedAttemptsReached5_LocksAccount()
    {
        var user = CreateUser();

        for (int i = 0; i < 5; i++)
        {
            await PostJsonAsync("/api/auth/login", new
            {
                Email = "test@example.com",
                MatKhau = "WrongPassword" + i
            });
        }

        var updatedUser = await _dbContext.TaiKhoans.FindAsync(user.Id);
        Assert.NotNull(updatedUser?.LockedUntil);
        Assert.True(updatedUser?.LockedUntil > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_LockedAccount_ReturnsLockoutMessage()
    {
        var user = CreateUser();
        user.FailedLoginCount = 5;
        user.LockedUntil = DateTime.UtcNow.AddMinutes(15);
        _dbContext.TaiKhoans.Update(user);
        _dbContext.SaveChanges();

        var (statusCode, body) = await PostJsonAsync("/api/auth/login", new
        {
            Email = "test@example.com",
            MatKhau = "Test123456"
        });

        Assert.Equal(401, statusCode);
        Assert.Contains("khoá", body.GetProperty("message").GetString());
    }

    [Fact]
    public async Task Login_SuccessAfterFailedAttempts_ResetsCounter()
    {
        var user = CreateUser();
        user.FailedLoginCount = 3;
        _dbContext.TaiKhoans.Update(user);
        _dbContext.SaveChanges();

        await PostJsonAsync("/api/auth/login", new
        {
            Email = "test@example.com",
            MatKhau = "Test123456"
        });

        var updatedUser = await _dbContext.TaiKhoans.FindAsync(user.Id);
        Assert.Equal(0, updatedUser?.FailedLoginCount);
        Assert.Null(updatedUser?.LockedUntil);
    }

    [Fact]
    public async Task Refresh_ValidRefreshToken_ReturnsNewAccessToken()
    {
        CreateUser();
        var (_, loginBody) = await PostJsonAsync("/api/auth/login", new
        {
            Email = "test@example.com",
            MatKhau = "Test123456"
        });

        var refreshToken = loginBody.GetProperty("refreshToken").GetString();

        var (statusCode, body) = await PostJsonAsync("/api/auth/refresh", new
        {
            RefreshToken = refreshToken
        });

        Assert.Equal(200, statusCode);
        Assert.True(body.TryGetProperty("accessToken", out _));
    }

    [Fact]
    public async Task Refresh_InvalidRefreshToken_Returns401()
    {
        var (statusCode, body) = await PostJsonAsync("/api/auth/refresh", new
        {
            RefreshToken = "invalid_token_12345"
        });

        Assert.Equal(401, statusCode);
        Assert.Equal("TOKEN_EXPIRED", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Logout_ValidToken_ClearsRefreshToken()
    {
        var user = CreateUser();
        var (_, loginBody) = await PostJsonAsync("/api/auth/login", new
        {
            Email = "test@example.com",
            MatKhau = "Test123456"
        });
        var refreshToken = loginBody.GetProperty("refreshToken").GetString();

        var (statusCode, body) = await PostJsonAsync("/api/auth/logout", new
        {
            RefreshToken = refreshToken
        });

        Assert.Equal(200, statusCode);
        Assert.True(body.GetProperty("success").GetBoolean());

        var updatedUser = await _dbContext.TaiKhoans.FindAsync(user.Id);
        Assert.Null(updatedUser?.RefreshTokenHash);
    }

    [Fact]
    public async Task Logout_ReuseOldToken_Returns401()
    {
        var user = CreateUser();
        var (_, loginBody) = await PostJsonAsync("/api/auth/login", new
        {
            Email = "test@example.com",
            MatKhau = "Test123456"
        });
        var refreshToken = loginBody.GetProperty("refreshToken").GetString();

        await PostJsonAsync("/api/auth/logout", new { RefreshToken = refreshToken });

        var (statusCode, body) = await PostJsonAsync("/api/auth/refresh", new { RefreshToken = refreshToken });

        Assert.Equal(401, statusCode);
        Assert.Equal("TOKEN_EXPIRED", body.GetProperty("error").GetString());
    }
}
