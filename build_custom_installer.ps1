# ==============================================================================
# build_custom_installer.ps1 - Kompilasi Custom Frameless Installer Mandiri
# Menghasilkan dist/SmartVidkonAutomation_Installer.exe (Single Standalone Executable)
# ==============================================================================
$ErrorActionPreference = "Stop"

$appDir = $PSScriptRoot
if ([string]::IsNullOrEmpty($appDir)) {
    $appDir = "D:\FokusKonten-Server\App_Dektop\Smart_Vidkon_Automation"
}

$distDir = Join-Path $appDir "dist"
if (-not (Test-Path $distDir)) {
    New-Item -ItemType Directory -Path $distDir -Force | Out-Null
}

$mainExe = Join-Path $appDir "SmartVidkonAutomation.exe"
if (-not (Test-Path $mainExe)) {
    Write-Host "Executable utama belum dikompilasi! Menjalankan build_desktop.ps1 terlebih dahulu..." -ForegroundColor Yellow
    & (Join-Path $appDir "build_desktop.ps1")
}

Write-Host "=================================================================" -ForegroundColor Cyan
Write-Host "  SMART VIDKON AUTOMATION - CUSTOM FRAMELESS INSTALLER" -ForegroundColor Cyan
Write-Host "=================================================================" -ForegroundColor Cyan

# 1. Siapkan Berkas Payload Bersih (app_payload.zip)
$tempStaging = Join-Path $appDir "dist\_payload_staging"
if (Test-Path $tempStaging) { Remove-Item -Path $tempStaging -Recurse -Force }
New-Item -ItemType Directory -Path $tempStaging -Force | Out-Null

Write-Host "Menyiapkan staging berkas aplikasi bersih..." -ForegroundColor Yellow

# Salin Berkas Utama
Copy-Item (Join-Path $appDir "SmartVidkonAutomation.exe") -Destination $tempStaging
Copy-Item (Join-Path $appDir "desktop") -Destination $tempStaging -Recurse
Copy-Item (Join-Path $appDir "bin") -Destination $tempStaging -Recurse
Copy-Item (Join-Path $appDir "engine") -Destination $tempStaging -Recurse
Copy-Item (Join-Path $appDir "config") -Destination $tempStaging -Recurse
Copy-Item (Join-Path $appDir "docs") -Destination $tempStaging -Recurse

# Salin Struktur Aset Bersih (Zero-Bloat)
$stgAssets = Join-Path $tempStaging "assets"
New-Item -ItemType Directory -Path (Join-Path $stgAssets "bgm") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $stgAssets "footage") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $stgAssets "luts") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $stgAssets "branding") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $tempStaging "projects") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $tempStaging "database") -Force | Out-Null

Copy-Item (Join-Path $appDir "assets\branding\SmartVidkon_Logo.png") -Destination (Join-Path $stgAssets "branding")
Copy-Item (Join-Path $appDir "assets\branding\running_cat_transparent.png") -Destination (Join-Path $stgAssets "branding")
Copy-Item (Join-Path $appDir "assets\footage\README.txt") -Destination (Join-Path $stgAssets "footage")
Copy-Item (Join-Path $appDir "assets\luts\luts_registry.json") -Destination (Join-Path $stgAssets "luts")
Copy-Item (Join-Path $appDir "database\katalog_vidcon_summary.json") -Destination (Join-Path $tempStaging "database")

# Kompres Staging ke payload.zip
$payloadZip = Join-Path $appDir "installer\payload.zip"
if (Test-Path $payloadZip) { Remove-Item -Path $payloadZip -Force }

Write-Host "Mengompresi payload aplikasi ke installer/payload.zip..." -ForegroundColor Yellow
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($tempStaging, $payloadZip, [System.IO.Compression.CompressionLevel]::Fastest, $false)

# Hapus staging sementara
Remove-Item -Path $tempStaging -Recurse -Force
$zipInfo = Get-Item $payloadZip
Write-Host "Ukuran Payload ZIP: $([math]::Round($zipInfo.Length / 1MB, 2)) MB" -ForegroundColor Green

# 2. Kompilasi Installer C#
$cscExe = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$outExe = Join-Path $distDir "SmartVidkonAutomation_Installer.exe"
$icoPath = Join-Path $appDir "desktop\SmartVidkon.ico"
$logoPath = Join-Path $appDir "assets\branding\SmartVidkon_Logo.png"
$runningCatPath = Join-Path $appDir "assets\branding\running_cat_transparent.png"
$heroBannerPath = Join-Path $appDir "assets\branding\SmartVidkon_Hero_Banner_transparent.png"

$sources = @(
    (Join-Path $appDir "installer\Program.cs"),
    (Join-Path $appDir "installer\InstallerUI.cs"),
    (Join-Path $appDir "installer\InstallerEngine.cs"),
    (Join-Path $appDir "installer\InstallerForm.cs")
)

$argsList = @(
    "/target:winexe",
    "/platform:x64",
    "/optimize+",
    "/win32icon:`"$icoPath`"",
    "/out:`"$outExe`"",
    "/r:System.dll,System.Core.dll,System.Drawing.dll,System.Windows.Forms.dll,System.IO.Compression.dll,System.IO.Compression.FileSystem.dll",
    "/resource:`"$payloadZip`",payload.zip",
    "/resource:`"$logoPath`",SmartVidkon_Logo.png",
    "/resource:`"$runningCatPath`",running_cat_transparent.png",
    "/resource:`"$heroBannerPath`",SmartVidkon_Hero_Banner_transparent.png"
)

foreach ($s in $sources) {
    $argsList += "`"$s`""
}

Write-Host "`nMemulai kompilasi C# Installer via csc.exe..." -ForegroundColor Yellow
$proc = Start-Process -FilePath $cscExe -ArgumentList ($argsList -join " ") -NoNewWindow -Wait -PassThru

# Hapus payload.zip setelah embed agar tidak menumpuk di repo
if (Test-Path $payloadZip) { Remove-Item -Path $payloadZip -Force }

if ($proc.ExitCode -eq 0 -and (Test-Path $outExe)) {
    $info = Get-Item $outExe
    $hash = Get-FileHash -Path $outExe -Algorithm SHA256
    $szMb = [math]::Round($info.Length / 1MB, 2)
    Write-Host ""
    Write-Host "[SUKSES] CUSTOM FRAMELESS INSTALLER SELESAI DIBUAT 100%!" -ForegroundColor Green
    Write-Host "Output EXE  : $($info.FullName)" -ForegroundColor Green
    Write-Host "Ukuran File : $szMb MB ($($info.Length) bytes)" -ForegroundColor Green
    Write-Host "SHA256 Hash : $($hash.Hash)" -ForegroundColor Green
    Write-Host "Exit Code   : 0" -ForegroundColor Green
    Write-Host ""
    exit 0
} else {
    Write-Host ""
    Write-Host "[GAGAL] Kompilasi C# installer gagal! Kode keluar: $($proc.ExitCode)" -ForegroundColor Red
    Write-Host ""
    exit $proc.ExitCode
}
