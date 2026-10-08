param([string]$Directory, [int]$Port = 5249)
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($Directory)) {
    $Directory = (Get-Content -LiteralPath (Join-Path $repoRoot 'data/s308-demo/latest.txt') -Raw -Encoding UTF8).Trim()
}
$database = Join-Path $Directory 's308.sqlite'
if (!(Test-Path -LiteralPath $database)) { throw 'Chưa có CSDL demo S3-08. Không dùng CSDL gốc để thử.' }
$runtime = Join-Path $repoRoot ('data/s308-runtime/' + [Guid]::NewGuid().ToString('N'))
dotnet build (Join-Path $repoRoot 'QL_PhongTro/QL_PhongTro.csproj') --no-restore -o $runtime
if ($LASTEXITCODE -ne 0) { throw 'Build không thành công.' }
Write-Host "Web demo: http://localhost:$Port/HoaDonDichVu/Monthly"
Write-Host "Chỉ thao tác bản sao: $database"
$runArgs = @((Join-Path $runtime 'QL_PhongTro.dll'), '--contentRoot', (Join-Path $repoRoot 'QL_PhongTro'),
    '--environment', 'Development', '--urls', "http://localhost:$Port", '--DatabasePath', $database,
    '--RoomImagesPath', (Join-Path $Directory 'room-images'), '--DataProtectionKeysPath', (Join-Path $Directory 'keys'),
    '--PasswordReset:PickupDirectory', (Join-Path $Directory 'mail'), '--Logging:EventLog:LogLevel:Default', 'None')
& dotnet @runArgs
