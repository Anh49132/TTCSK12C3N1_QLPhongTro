using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Models;

namespace QL_PhongTro.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TaiKhoan> TaiKhoans { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<TaiKhoan>();
        entity.ToTable("tai_khoan");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.HoTen).HasColumnName("ho_ten").IsRequired().HasMaxLength(100);
        entity.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(254);
        entity.Property(e => e.SoDienThoai).HasColumnName("so_dien_thoai").IsRequired().HasMaxLength(15);
        entity.Property(e => e.MatKhau).HasColumnName("mat_khau").IsRequired().HasMaxLength(128);
        entity.Property(e => e.VaiTro).HasColumnName("vai_tro").HasMaxLength(20).HasDefaultValue("KHACH_THUE");
        entity.Property(e => e.DangHoatDong).HasColumnName("dang_hoat_dong").HasDefaultValue(true);
        entity.Property(e => e.IsStaff).HasColumnName("is_staff").HasDefaultValue(false);
        entity.Property(e => e.IsSuperuser).HasColumnName("is_superuser").HasDefaultValue(false);
        entity.Property(e => e.LastLogin).HasColumnName("last_login");
        entity.Property(e => e.NgayTao).HasColumnName("ngay_tao").IsRequired();
        entity.Property(e => e.NgayCapNhat).HasColumnName("ngay_cap_nhat").IsRequired();
    }
}
