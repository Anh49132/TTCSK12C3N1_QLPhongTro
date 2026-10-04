param([string]$Password = 'DemoSprint2@2026', [int]$Port = 5268)
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$demoRoot = Join-Path $repoRoot 'data\sprint2-demo'
$runtime = Join-Path $demoRoot 'runtime'
$output = Join-Path $demoRoot ((Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [Guid]::NewGuid().ToString('N').Substring(0,6))
Push-Location $repoRoot
try {
    dotnet build .\QL_PhongTro\QL_PhongTro.csproj -o $runtime
    if ($LASTEXITCODE -ne 0) { throw 'Build thất bại; chưa tạo dữ liệu mẫu.' }
    dotnet (Join-Path $runtime 'QL_PhongTro.dll') --contentRoot (Join-Path $repoRoot 'QL_PhongTro') --environment Development --Sprint2Demo:Directory $output --Sprint2Demo:Password $Password --Sprint2Demo:Url "http://localhost:$Port" --create-sprint2-demo
    if ($LASTEXITCODE -ne 0) { throw "Tạo dữ liệu mẫu thất bại. Giữ thư mục để kiểm tra: $output" }
    Set-Content -LiteralPath (Join-Path $demoRoot 'latest.txt') -Value $output -Encoding utf8
    $access = Get-Content -LiteralPath (Join-Path $output 'access.json') -Raw -Encoding UTF8 | ConvertFrom-Json
    Write-Host "`nDữ liệu mẫu: $output"
    $access.accounts | Format-Table email,role
    Write-Host "Mật khẩu chung: $($access.password)"
    Write-Host "Chạy: .\verification\Start-Sprint2Demo.ps1"
} finally { Pop-Location }
