namespace QL_PhongTro.Models

open System
open System.ComponentModel.DataAnnotations

[<CLIMutable>]
type TaiKhoan =
    { 
        [<Key>]
        Id: int
        [<Required>]
        [<MaxLength(100)>]
        HoTen: string
        [<Required>]
        [<MaxLength(254)>]
        Email: string
        [<Required>]
        [<MaxLength(15)>]
        SoDienThoai: string
        [<Required>]
        [<MaxLength(128)>]
        MatKhau: string
        [<MaxLength(20)>]
        VaiTro: string
        DangHoatDong: bool
        IsStaff: bool
        IsSuperuser: bool
        LastLogin: Nullable<DateTime>
        NgayTao: DateTime
        NgayCapNhat: DateTime
    }
