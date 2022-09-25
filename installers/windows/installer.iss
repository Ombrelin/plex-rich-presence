#define MyAppName "PLEX Rich Presence"
#define MyAppVersion "2.0"
#define MyAppPublisher "Arsene Lapostolet"
#define MyAppExeName "PlexRichPresence.exe"

[Setup]
AppId={{FCD2F146-6DB8-4DDC-AAA2-E9E5DA061AA6}
AppName={#MyAppName}
AppVersion={#MyAppVersion}.{#BuildNumber}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
UsedUserAreasWarning=no
LicenseFile=../../LICENSE
OutputDir=C:\temp\plex-rich-presence\plex-rich-presence-setup.exe
OutputBaseFilename=plex-rich-presence-setup
SetupIconFile=../../src/PlexRichPresence.UI.Avalonia/Assets/avalonia-logo.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
Source: "C:\temp\plex-rich-presence\*"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent