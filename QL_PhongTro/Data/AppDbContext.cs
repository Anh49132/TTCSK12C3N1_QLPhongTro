using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Models;

namespace QL_PhongTro.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TaiKhoan> TaiKhoans { get; set; } = null!;
    public DbSet<ToaNha> ToaNhas { get; set; } = null!;
    public DbSet<PhongTro> PhongTros { get; set; } = null!;

    public DbSet<AppRole> AppRoles { get; set; } = null!;
    public DbSet<AppModule> AppModules { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppRole>().ToTable("app_role").HasKey(r => r.Code);
        modelBuilder.Entity<AppModule>().ToTable("app_module").HasKey(m => m.Code);
        var permission = modelBuilder.Entity<RolePermission>();
        permission.ToTable("role_permission").HasKey(p => new { p.RoleCode, p.ModuleCode });
        permission.HasOne<AppRole>().WithMany().HasForeignKey(p => p.RoleCode).OnDelete(DeleteBehavior.Restrict);
        permission.HasOne<AppModule>().WithMany().HasForeignKey(p => p.ModuleCode).OnDelete(DeleteBehavior.Restrict);

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

        var building = modelBuilder.Entity<ToaNha>();
        building.ToTable("toa_nha");
        building.HasKey(e => e.Id);
        building.Property(e => e.Id).HasColumnName("id");
        building.Property(e => e.ChuNhaId).HasColumnName("chu_nha_id").IsRequired();
        building.Property(e => e.QuanLyId).HasColumnName("quan_ly_id");
        building.Property(e => e.TenToaNha).HasColumnName("ten_toa_nha").IsRequired().HasMaxLength(150);
        building.Property(e => e.DiaChi).HasColumnName("dia_chi").IsRequired();
        building.Property(e => e.PhuongXa).HasColumnName("phuong_xa").HasMaxLength(100);
        building.Property(e => e.QuanHuyen).HasColumnName("quan_huyen").HasMaxLength(100);
        building.Property(e => e.TinhThanh).HasColumnName("tinh_thanh").HasMaxLength(100);
        building.Property(e => e.SoTang).HasColumnName("so_tang");
        building.Property(e => e.NgayChotHangThang).HasColumnName("ngay_chot_hang_thang").HasDefaultValue(1);
        building.Property(e => e.DangHoatDong).HasColumnName("dang_hoat_dong").HasDefaultValue(true);
        building.Property(e => e.GhiChu).HasColumnName("ghi_chu");
        building.HasIndex(e => e.ChuNhaId).HasDatabaseName("ix_toa_nha_chu_nha_id");
        building.HasOne<TaiKhoan>().WithMany().HasForeignKey(e => e.ChuNhaId).OnDelete(DeleteBehavior.Restrict);

        var room = modelBuilder.Entity<PhongTro>();
        room.ToTable("phong_tro");
        room.HasKey(e => e.Id);
        room.Property(e => e.Id).HasColumnName("id");
        room.Property(e => e.ToaNhaId).HasColumnName("toa_nha_id").IsRequired();
        room.Property(e => e.MaPhong).HasColumnName("ma_phong").IsRequired().HasMaxLength(20);
        room.Property(e => e.Tang).HasColumnName("tang").IsRequired();
        room.Property(e => e.LoaiPhong).HasColumnName("loai_phong").HasMaxLength(50);
        room.Property(e => e.DienTich).HasColumnName("dien_tich").HasColumnType("decimal(8,2)").IsRequired();
        room.Property(e => e.GiaThue).HasColumnName("gia_thue").IsRequired();
        room.Property(e => e.TienCocDuKien).HasColumnName("tien_coc_du_kien").HasDefaultValue(0L);
        room.Property(e => e.SoNguoiToiDa).HasColumnName("so_nguoi_toi_da").IsRequired();
        room.Property(e => e.TrangThai).HasColumnName("trang_thai").IsRequired().HasMaxLength(25).HasDefaultValue("TRONG");
        room.Property(e => e.MoTa).HasColumnName("mo_ta");
        room.Property(e => e.NgayTao).HasColumnName("ngay_tao").IsRequired();
        room.Property(e => e.PhienBan).HasColumnName("phien_ban").HasDefaultValue(0);
        room.HasIndex(e => new { e.ToaNhaId, e.MaPhong }).HasDatabaseName("ux_phong_tro_toa_nha_ma_phong").IsUnique();
        room.HasIndex(e => new { e.ToaNhaId, e.TrangThai }).HasDatabaseName("ix_phong_tro_toa_nha_trang_thai");
        room.HasOne<ToaNha>().WithMany().HasForeignKey(e => e.ToaNhaId).OnDelete(DeleteBehavior.Restrict);
    }
}
