; ============================================================================
; IIT FINCORE — Script d'Installeur Windows Professionnel (Inno Setup 6)
; Génère un exécutable clé en main : IIT_FINCORE_Setup_v2.0.exe
; ============================================================================

#define MyAppName "IIT FINCORE"
#define MyAppVersion "2.0.0"
#define MyAppPublisher "IIT FINCORE Solutions"
#define MyAppExeName "IIT.FinCore.WinForms.exe"

[Setup]
AppId={{A8F3D912-4C2B-4E19-9D81-7B5E3F2A1C09}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=..\dist\installer
OutputBaseFilename=IIT_FINCORE_Setup_v2.0
SetupIconFile=..\IIT.FinCore.WinForms\Assets\app_icon.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "..\dist\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\database\iit_fincore.sql"; DestDir: "{app}\database"; Flags: ignoreversion
Source: "..\database\upgrade_v2.sql"; DestDir: "{app}\database"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
