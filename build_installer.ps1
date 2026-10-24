param(
    [switch]$CleanProfile
)

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
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Gagal mengompilasi executable utama."
    }
}

# Deteksi Compiler ISCC.exe
$isccPaths = @(
    "C:\Program Files\Inno Setup 7\ISCC.exe",
    "C:\Program Files (x86)\Inno Setup 7\ISCC.exe",
    "C:\Program Files\Inno Setup 6\ISCC.exe",
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    "$env:LOCALAPPDATA\Programs\Inno Setup 7\ISCC.exe",
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe"
)

$isccExe = $null
foreach ($path in $isccPaths) {
    if (Test-Path $path) {
        $isccExe = $path
        break
    }
}

if (-not $isccExe) {
    $cmd = Get-Command "iscc.exe" -ErrorAction SilentlyContinue
    if ($cmd) {
        $isccExe = $cmd.Source
    }
}

if (-not $isccExe) {
    Write-Error "Inno Setup Compiler (ISCC.exe) tidak ditemukan! Pastikan Inno Setup 6 atau 7 terpasang."
}

$baseName = if ($CleanProfile) { "SmartVidkonAutomation_Setup_Clean_v1.0.0" } else { "SmartVidkonAutomation_Setup_v1.0.0" }
$profileName = if ($CleanProfile) { "DISTRIBUSI BERSIH WINGET / PUBLIK (Zero-Bloat Asset)" } else { "PENGUJIAN LOKAL & SANDBOX (Termasuk Aset Uji)" }

Write-Host "=================================================================" -ForegroundColor Cyan
Write-Host "  SMART VIDKON AUTOMATION - INSTALLER PACKAGING ENGINE" -ForegroundColor Cyan
Write-Host "=================================================================" -ForegroundColor Cyan
Write-Host "Profil     : $profileName" -ForegroundColor Yellow
Write-Host "App Dir    : $appDir"
Write-Host "Dist Dir   : $distDir"
Write-Host "Target EXE : $baseName.exe"
Write-Host "ISCC Exe   : $isccExe"

$issPath = Join-Path $appDir "SmartVidkonAutomation.iss"
$icoPath = Join-Path $appDir "desktop\SmartVidkon.ico"

$filesSection = if ($CleanProfile) {
@"
[Files]
Source: "$appDir\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "$appDir\desktop\SmartVidkon.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "$appDir\config\*"; DestDir: "{app}\config"; Flags: ignoreversion recursesubdirs createallsubdirs; Permissions: users-full
Source: "$appDir\docs\*"; DestDir: "{app}\docs"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "$appDir\bin\*"; DestDir: "{app}\bin"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "$appDir\engine\*"; DestDir: "{app}\engine"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "$appDir\assets\branding\SmartVidkon_Logo.png"; DestDir: "{app}\assets\branding"; Flags: ignoreversion; Permissions: users-full
Source: "$appDir\assets\footage\README.txt"; DestDir: "{app}\assets\footage"; Flags: ignoreversion; Permissions: users-full
Source: "$appDir\assets\luts\luts_registry.json"; DestDir: "{app}\assets\luts"; Flags: ignoreversion; Permissions: users-full
Source: "$appDir\database\katalog_vidcon_summary.json"; DestDir: "{app}\database"; Flags: ignoreversion; Permissions: users-full

[Dirs]
Name: "{app}\projects"; Flags: uninsneveruninstall; Permissions: users-full
Name: "{app}\config"; Permissions: users-full
Name: "{app}\database"; Permissions: users-full
Name: "{app}\assets"; Permissions: users-full
Name: "{app}\assets\bgm"; Permissions: users-full
Name: "{app}\assets\footage"; Permissions: users-full
Name: "{app}\assets\luts"; Permissions: users-full
Name: "{app}\assets\branding"; Permissions: users-full
"@
} else {
@"
[Files]
Source: "$appDir\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "$appDir\desktop\SmartVidkon.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "$appDir\config\*"; DestDir: "{app}\config"; Flags: ignoreversion recursesubdirs createallsubdirs; Permissions: users-full
Source: "$appDir\docs\*"; DestDir: "{app}\docs"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "$appDir\bin\*"; DestDir: "{app}\bin"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "$appDir\engine\*"; DestDir: "{app}\engine"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "$appDir\assets\*"; DestDir: "{app}\assets"; Flags: ignoreversion recursesubdirs createallsubdirs; Permissions: users-full
Source: "$appDir\database\*"; DestDir: "{app}\database"; Flags: ignoreversion recursesubdirs createallsubdirs; Permissions: users-full

[Dirs]
Name: "{app}\projects"; Flags: uninsneveruninstall; Permissions: users-full
Name: "{app}\config"; Permissions: users-full
Name: "{app}\database"; Permissions: users-full
Name: "{app}\assets"; Permissions: users-full
Name: "{app}\assets\footage"; Permissions: users-full
"@
}

$issContent = @"
; Inno Setup Script - Smart Vidkon Automation v1.0.0
#define MyAppName "Smart Vidkon Automation"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "FokusKonten Enterprise"
#define MyAppExeName "SmartVidkonAutomation.exe"
#define MyAppAssocName MyAppName + " Project"
#define MyAppAssocExt ".svk"
#define MyAppAssocKey StringChange(MyAppAssocName, " ", "") + MyAppAssocExt

[Setup]
AppId={{E8150317-9BFE-4FE1-B7D5-3A9E648EB4C0}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\Smart Vidkon Automation
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=$distDir
OutputBaseFilename=$baseName
SetupIconFile=$icoPath
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

$filesSection

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\SmartVidkon.ico"
Name: "{group}\Panduan Ringkas Pengguna"; Filename: "{app}\docs\PANDUAN_RINGKAS_PENGGUNA.html"
Name: "{group}\Cara Aktivasi Lisensi"; Filename: "{app}\docs\CARA_AKTIVASI_LISENSI.txt"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\SmartVidkon.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
"@

[System.IO.File]::WriteAllText($issPath, $issContent, [System.Text.Encoding]::UTF8)
Write-Host "Script ISS : $issPath"

Write-Host "`nMemulai proses pemaketan Inno Setup..." -ForegroundColor Yellow

$proc = Start-Process -FilePath $isccExe -ArgumentList "`"$issPath`"" -NoNewWindow -Wait -PassThru

$setupExe = Join-Path $distDir "$baseName.exe"

if ($proc.ExitCode -eq 0 -and (Test-Path $setupExe)) {
    $info = Get-Item $setupExe
    $hash = Get-FileHash -Path $setupExe -Algorithm SHA256
    $szMb = [math]::Round($info.Length / 1MB, 2)
    Write-Host ""
    Write-Host "[SUKSES] INSTALLER BERHASIL DIBUAT 100%!" -ForegroundColor Green
    Write-Host "Profil      : $profileName" -ForegroundColor Green
    Write-Host "Installer   : $($info.FullName)" -ForegroundColor Green
    Write-Host "Ukuran File : $szMb MB ($($info.Length) bytes)" -ForegroundColor Green
    Write-Host "SHA256 Hash : $($hash.Hash)" -ForegroundColor Green
    Write-Host "Exit Code   : 0" -ForegroundColor Green
    Write-Host ""

    # Sinkronisasi ke Winget Manifest jika Clean Profile
    if ($CleanProfile) {
        $wingetInstYaml = Join-Path $appDir "winget\FokusKonten.SmartVidkonAutomation.installer.yaml"
        if (Test-Path $wingetInstYaml) {
            $yamlTxt = [System.IO.File]::ReadAllText($wingetInstYaml, [System.Text.Encoding]::UTF8)
            $yamlTxt = $yamlTxt -replace "InstallerSha256:\s+[A-Fa-f0-9]+", "InstallerSha256: $($hash.Hash)"
            [System.IO.File]::WriteAllText($wingetInstYaml, $yamlTxt, [System.Text.Encoding]::UTF8)
            Write-Host "[WINGET] Hash SHA-256 diperbarui di manifest winget." -ForegroundColor Cyan
        }
    }

    exit 0
}
else {
    Write-Host ""
    Write-Host "[GAGAL] Pemaketan installer gagal! Kode keluar: $($proc.ExitCode)" -ForegroundColor Red
    Write-Host ""
    exit $proc.ExitCode
}
