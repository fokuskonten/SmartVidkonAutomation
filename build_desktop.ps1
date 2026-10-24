# ==============================================================================
# build_desktop.ps1 - Skrip Kompilasi C# WinForms Smart Vidkon Automation
# Menggunakan Microsoft (R) Visual C# Compiler .NET Framework v4.0.30319 (x64)
# ==============================================================================
$ErrorActionPreference = "Stop"

$appDir = $PSScriptRoot
if ([string]::IsNullOrEmpty($appDir)) {
    $appDir = "D:\FokusKonten-Server\App_Dektop\Smart_Vidkon_Automation"
}

$desktopDir = Join-Path $appDir "desktop"
$outputExe = Join-Path $appDir "SmartVidkonAutomation.exe"
$iconPath = Join-Path $desktopDir "SmartVidkon.ico"

$cscExe = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $cscExe)) {
    $cscExe = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
}

Write-Host "=================================================================" -ForegroundColor Cyan
Write-Host "  SMART VIDKON AUTOMATION - COMPILATION ENGINE" -ForegroundColor Cyan
Write-Host "=================================================================" -ForegroundColor Cyan
Write-Host "App Dir     : $appDir"
Write-Host "Desktop Dir : $desktopDir"
Write-Host "Output EXE  : $outputExe"
Write-Host "Compiler    : $cscExe"

# Tutup instance yang sedang berjalan jika ada agar file EXE tidak terkunci
$runningProc = Get-Process -Name "SmartVidkonAutomation" -ErrorAction SilentlyContinue
if ($runningProc) {
    Write-Host "Menutup instance berjalan (PID: $($runningProc.Id)) agar file tidak terkunci..." -ForegroundColor Yellow
    Stop-Process -Name "SmartVidkonAutomation" -Force -ErrorAction SilentlyContinue
    Start-Sleep -Milliseconds 600
}

$sourceFiles = Get-ChildItem -Path $desktopDir -Recurse -Filter "*.cs" | Select-Object -ExpandProperty FullName
Write-Host "Source Files: $($sourceFiles.Count) files"

$arguments = New-Object System.Collections.Generic.List[string]
$arguments.Add("/target:winexe")
$arguments.Add("/platform:anycpu")
$arguments.Add("/optimize+")
$arguments.Add("/nologo")
$arguments.Add("/utf8output")
$arguments.Add("/out:$outputExe")

if (Test-Path $iconPath) {
    $arguments.Add("/win32icon:$iconPath")
    Write-Host "Icon Linked : $iconPath"
}

$references = @(
    "System.dll",
    "System.Core.dll",
    "System.Drawing.dll",
    "System.Windows.Forms.dll",
    "System.Web.Extensions.dll",
    "System.Net.Http.dll"
)
foreach ($ref in $references) {
    $arguments.Add("/r:$ref")
}

foreach ($sf in $sourceFiles) {
    $arguments.Add($sf)
}

Write-Host ""
Write-Host "Memulai kompilasi..." -ForegroundColor Yellow

$proc = Start-Process -FilePath $cscExe -ArgumentList $arguments -NoNewWindow -Wait -PassThru

if ($proc.ExitCode -eq 0 -and (Test-Path $outputExe)) {
    $fileInfo = Get-Item $outputExe
    $hash = Get-FileHash -Path $outputExe -Algorithm SHA256
    $sz = [math]::Round($fileInfo.Length / 1KB, 2)
    Write-Host ""
    Write-Host "[SUKSES] KOMPILASI BERHASIL 100%!" -ForegroundColor Green
    Write-Host "File Output : $($fileInfo.FullName)" -ForegroundColor Green
    Write-Host "Ukuran File : $sz KB ($($fileInfo.Length) bytes)" -ForegroundColor Green
    Write-Host "SHA256 Hash : $($hash.Hash)" -ForegroundColor Green
    Write-Host "Exit Code   : 0" -ForegroundColor Green
    Write-Host ""
    exit 0
}
else {
    Write-Host ""
    Write-Host "[GAGAL] KOMPILASI GAGAL! Kode Keluar: $($proc.ExitCode)" -ForegroundColor Red
    Write-Host ""
    exit $proc.ExitCode
}
