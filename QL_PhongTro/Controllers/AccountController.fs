namespace QL_PhongTro.Controllers

open System
open System.Text.RegularExpressions
open System.Security.Claims
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open QL_PhongTro.Data
open QL_PhongTro.Models

[<CLIMutable>]
type RegisterViewModel =
    {
        HoTen: string
        Email: string
        SoDienThoai: string
        MatKhau: string
    }

open QL_PhongTro

type AccountController(logger: ILogger<AccountController>, db: AppDbContext, settings: RegistrationSettings) =
    inherit Controller()

    member private _.ValidatePhone (phone: string) =
        if String.IsNullOrWhiteSpace(phone) then false
        else Regex.IsMatch(phone, "^0\\d{9}$")

    member private _.ValidatePassword (pwd: string) =
        if String.IsNullOrEmpty(pwd) then false
        else pwd.Length >= 8 && Regex.IsMatch(pwd, "[A-Za-z]") && Regex.IsMatch(pwd, "\\d")

    member this.Register() : IActionResult =
        this.View()

    [<HttpPost>]
    member this.Register(model: RegisterViewModel) : IActionResult =
        // Server-side validation
        if isNull (box model) then
            this.ModelState.AddModelError("", "Invalid request")
            this.View(model) :> IActionResult
        else
            if String.IsNullOrWhiteSpace(model.HoTen) then this.ModelState.AddModelError("HoTen", "Họ tên là bắt buộc")
            if String.IsNullOrWhiteSpace(model.Email) then this.ModelState.AddModelError("Email", "Email là bắt buộc")
            if not (this.ValidatePhone(model.SoDienThoai)) then this.ModelState.AddModelError("SoDienThoai", "Số điện thoại phải bắt đầu bằng 0 và đúng 10 chữ số")
            if not (this.ValidatePassword(model.MatKhau)) then this.ModelState.AddModelError("MatKhau", "Mật khẩu tối thiểu 8 ký tự, ít nhất một chữ cái và một chữ số")

            // If basic validation failed, return early
            if not this.ModelState.IsValid then
                this.View(model) :> IActionResult
            else
                // Normalize inputs for checks
                let emailNorm = if String.IsNullOrWhiteSpace(model.Email) then model.Email else model.Email.ToLower().Trim()
                let phoneNorm = model.SoDienThoai

                // Check duplicates in DB (if enabled)
                let mutable emailExists = false
                let mutable phoneExists = false
                if settings.EnableDuplicateCheck then
                    emailExists <-
                        query {
                            for u in db.TaiKhoans do
                            where (u.Email = emailNorm)
                            select u
                            take 1
                        }
                        |> Seq.tryHead
                        |> Option.isSome

                    phoneExists <-
                        query {
                            for u in db.TaiKhoans do
                            where (u.SoDienThoai = phoneNorm)
                            select u
                            take 1
                        }
                        |> Seq.tryHead
                        |> Option.isSome

                    if emailExists then this.ModelState.AddModelError("Email", "Email đã được sử dụng")
                    if phoneExists then this.ModelState.AddModelError("SoDienThoai", "Số điện thoại đã được sử dụng")

                if settings.EnableDuplicateCheck && (emailExists || phoneExists) then
                    // return view with errors
                    this.View(model) :> IActionResult
                else
                    // proceed to create user
                    // Hash password with BCrypt
                    let hashed = BCrypt.Net.BCrypt.HashPassword(model.MatKhau)

                    let now = DateTime.UtcNow
                    let user = {
                        Id = 0
                        HoTen = model.HoTen
                        Email = emailNorm
                        SoDienThoai = phoneNorm
                        MatKhau = hashed
                        VaiTro = "KHACH_THUE"
                        DangHoatDong = true
                        IsStaff = false
                        IsSuperuser = false
                        LastLogin = Nullable<DateTime>()
                        NgayTao = now
                        NgayCapNhat = now
                    }

                    // Save
                    db.TaiKhoans.Add(user) |> ignore
                    db.SaveChanges() |> ignore

                    // Sign in user with cookie
                    let claims = [| Claim(ClaimTypes.NameIdentifier, user.Id.ToString()); Claim(ClaimTypes.Name, user.HoTen); Claim(ClaimTypes.Role, user.VaiTro) |]
                    let identity = ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)
                    let principal = ClaimsPrincipal(identity)
                    this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal).GetAwaiter().GetResult()

                    // Redirect to home with success message
                    this.TempData.["RegisterSuccess"] <- "Đăng ký thành công"
                    this.RedirectToAction("Index", "Home") :> IActionResult
