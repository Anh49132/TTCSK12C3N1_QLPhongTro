@echo off
setlocal EnableExtensions

pushd "%~dp0" >nul || (
    echo [ERROR] Khong the mo thu muc du an.
    exit /b 1
)

set "PROJECT=QL_PhongTro\QL_PhongTro.csproj"

rem Optional machine-local settings. This file is ignored by Git.
rem Explicit environment variables already set in the terminal take precedence.
if exist ".env.local" (
    for /f "usebackq eol=# tokens=1,* delims==" %%A in (".env.local") do (
        if /I "%%A"=="DatabasePath" if not defined DatabasePath set "DatabasePath=%%B"
        if /I "%%A"=="RoomImagesPath" if not defined RoomImagesPath set "RoomImagesPath=%%B"
        if /I "%%A"=="DataProtectionKeysPath" if not defined DataProtectionKeysPath set "DataProtectionKeysPath=%%B"
        if /I "%%A"=="PasswordReset__PickupDirectory" if not defined PasswordReset__PickupDirectory set "PasswordReset__PickupDirectory=%%B"
        if /I "%%A"=="Logging__EventLog__LogLevel__Default" if not defined Logging__EventLog__LogLevel__Default set "Logging__EventLog__LogLevel__Default=%%B"
    )
)

if not "%~1"=="" if /I not "%~1"=="--check-only" (
    echo Cach dung: run.bat [--check-only]
    popd
    exit /b 2
)

where dotnet >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Khong tim thay .NET SDK. Hay cai .NET 10 SDK va thu lai.
    popd
    exit /b 1
)

if not exist "%PROJECT%" (
    echo [ERROR] Khong tim thay %PROJECT%.
    popd
    exit /b 1
)

for /f "usebackq delims=" %%I in (`powershell -NoProfile -Command "$p=$env:DatabasePath; if ([string]::IsNullOrWhiteSpace($p)) { [IO.Path]::GetFullPath((Join-Path $pwd.Path 'QL_PhongTro\Data\local-dev.sqlite')) } elseif ([IO.Path]::IsPathRooted($p)) { [IO.Path]::GetFullPath($p) } else { [IO.Path]::GetFullPath((Join-Path (Join-Path $pwd.Path 'QL_PhongTro') $p)) }"`) do set "DatabasePath=%%I"

if not defined DatabasePath (
    echo [ERROR] Khong xac dinh duoc duong dan database.
    popd
    exit /b 1
)

echo [INFO] DatabasePath=%DatabasePath%
echo [1/3] Dang khoi phuc goi phu thuoc...
dotnet restore "%PROJECT%"
if errorlevel 1 goto :failed

if not exist "%DatabasePath%" (
    echo [2/3] Database chua ton tai. Dang khoi tao file moi...
    dotnet run --project "%PROJECT%" --no-restore -- --initialize-database
    if errorlevel 1 goto :failed
) else (
    echo [2/3] Giu nguyen database hien co.
)

echo [3/3] Dang kiem tra schema database...
dotnet run --project "%PROJECT%" --no-restore -- --check-database
if errorlevel 1 (
    echo [ERROR] Database hien co chua san sang. Script khong tu dong nang cap de tranh thay doi du lieu.
    echo [INFO] Hay sao luu database, sau do chay --update-database theo README.md.
    goto :failed
)

if /I "%~1"=="--check-only" (
    echo [OK] Restore, khoi tao neu can va kiem tra database da thanh cong.
    popd
    exit /b 0
)

echo [INFO] Dang chay web tai http://localhost:5247
echo [INFO] Nhan Ctrl+C de dung ung dung.
dotnet run --project "%PROJECT%" --no-restore --launch-profile http
set "EXIT_CODE=%ERRORLEVEL%"
popd
exit /b %EXIT_CODE%

:failed
set "EXIT_CODE=%ERRORLEVEL%"
if "%EXIT_CODE%"=="0" set "EXIT_CODE=1"
popd
exit /b %EXIT_CODE%
