namespace QL_PhongTro

type RegistrationSettings() =
    member val EnableDuplicateCheck: bool = true with get, set

namespace QL_PhongTro.Data

open System
open Microsoft.EntityFrameworkCore
open QL_PhongTro.Models

type AppDbContext(options: DbContextOptions<AppDbContext>) =
    inherit DbContext(options)

    [<DefaultValue>]
    val mutable taiKhoansField: DbSet<TaiKhoan>
    member this.TaiKhoans
        with get() = this.taiKhoansField
        and set v = this.taiKhoansField <- v

    override _.OnModelCreating(modelBuilder: ModelBuilder) =
        let entity = modelBuilder.Entity<TaiKhoan>()
        entity.ToTable("tai_khoan") |> ignore
        entity.HasKey([| "Id" |]) |> ignore
        entity.Property(fun e -> e.Id).HasColumnName("id") |> ignore
        entity.Property(fun e -> e.HoTen).HasColumnName("ho_ten").IsRequired().HasMaxLength(100) |> ignore
        entity.Property(fun e -> e.Email).HasColumnName("email").IsRequired().HasMaxLength(254) |> ignore
        entity.Property(fun e -> e.SoDienThoai).HasColumnName("so_dien_thoai").IsRequired().HasMaxLength(15) |> ignore
        entity.Property(fun e -> e.MatKhau).HasColumnName("mat_khau").IsRequired().HasMaxLength(128) |> ignore
        entity.Property(fun e -> e.VaiTro).HasColumnName("vai_tro").HasMaxLength(20).HasDefaultValue("KHACH_THUE") |> ignore
        entity.Property(fun e -> e.DangHoatDong).HasColumnName("dang_hoat_dong").HasDefaultValue(true) |> ignore
        entity.Property(fun e -> e.IsStaff).HasColumnName("is_staff").HasDefaultValue(false) |> ignore
        entity.Property(fun e -> e.IsSuperuser).HasColumnName("is_superuser").HasDefaultValue(false) |> ignore
        entity.Property(fun e -> e.LastLogin).HasColumnName("last_login") |> ignore
        entity.Property(fun e -> e.NgayTao).HasColumnName("ngay_tao").IsRequired() |> ignore
        entity.Property(fun e -> e.NgayCapNhat).HasColumnName("ngay_cap_nhat").IsRequired() |> ignore
        ()
