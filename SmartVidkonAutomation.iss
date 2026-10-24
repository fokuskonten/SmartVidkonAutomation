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
OutputDir=D:\FokusKonten-Server\App_Dektop\Smart_Vidkon_Automation\dist
OutputBaseFilename=SmartVidkonAutomation_Setup_v1.0.0
SetupIconFile=D:\FokusKonten-Server\App_Dektop\Smart_Vidkon_Automation\desktop\SmartVidkon.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "D:\FokusKonten-Server\App_Dektop\Smart_Vidkon_Automation\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\FokusKonten-Server\App_Dektop\Smart_Vidkon_Automation\desktop\SmartVidkon.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\FokusKonten-Server\App_Dektop\Smart_Vidkon_Automation\config\*"; DestDir: "{app}\config"; Flags: ignoreversion recursesubdirs createallsubdirs; Permissions: users-full
Source: "D:\FokusKonten-Server\App_Dektop\Smart_Vidkon_Automation\docs\*"; DestDir: "{app}\docs"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "D:\FokusKonten-Server\App_Dektop\Smart_Vidkon_Automation\bin\*"; DestDir: "{app}\bin"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "D:\FokusKonten-Server\App_Dektop\Smart_Vidkon_Automation\engine\*"; DestDir: "{app}\engine"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "D:\FokusKonten-Server\App_Dektop\Smart_Vidkon_Automation\assets\*"; DestDir: "{app}\assets"; Flags: ignoreversion recursesubdirs createallsubdirs; Permissions: users-full
Source: "D:\FokusKonten-Server\App_Dektop\Smart_Vidkon_Automation\database\*"; DestDir: "{app}\database"; Flags: ignoreversion recursesubdirs createallsubdirs; Permissions: users-full

[Dirs]
Name: "{app}\projects"; Flags: uninsneveruninstall; Permissions: users-full
Name: "{app}\config"; Permissions: users-full
Name: "{app}\database"; Permissions: users-full
Name: "{app}\assets"; Permissions: users-full
Name: "{app}\assets\footage"; Permissions: users-full

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\SmartVidkon.ico"
Name: "{group}\Panduan Ringkas Pengguna"; Filename: "{app}\docs\PANDUAN_RINGKAS_PENGGUNA.html"
Name: "{group}\Cara Aktivasi Lisensi"; Filename: "{app}\docs\CARA_AKTIVASI_LISENSI.txt"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\SmartVidkon.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent