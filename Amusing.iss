[Setup]
AppName=Amusing Web
AppVersion=1.0
CreateUninstallRegKey=no
AppendDefaultDirName=no
DefaultDirName=C:\Amusing\Amusing
DisableDirPage=yes
DisableProgramGroupPage=yes
OutputBaseFilename=AmusingInstaller
OutputDir=C:\Users\hnijk\Desktop
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin

[Files]
; ── Website bestanden → C:\Amusing\Amusing
Source: "C:\Users\hnijk\source\repos\hnsoftwaredevelopment\Amusing\Amusing\bin\Release\net9.0\win-x64\publish\*"; \
    DestDir: "{app}"; \
    Flags: recursesubdirs createallsubdirs ignoreversion;

; ── User Secrets
Source: "C:\Users\hnijk\source\repos\hnsoftwaredevelopment\Amusing\Amusing\bin\aspnet-Amusing-d4b4dda8-27a3-4a96-94e4-f605690b8606\*"; \
    DestDir: "{userappdata}\Microsoft\UserSecrets\aspnet-Amusing-d4b4dda8-27a3-4a96-94e4-f605690b8606"; \
    Flags: recursesubdirs createallsubdirs ignoreversion;

; ── Icoon
Source: "C:\Users\hnijk\source\repos\hnsoftwaredevelopment\Amusing\Amusing\bin\AmusingLogo.ico"; \
    DestDir: "{app}"; \
    Flags: ignoreversion;

[Icons]
Name: "{userdesktop}\Amusing Beheer"; \
    Filename: "{app}\Amusing.bat"; \
    IconFilename: "{app}\AmusingLogo.ico"; \
    Comment: "Start Amusing lokaal op";

[Code]
// Wordt aangeroepen vóór de installatie begint
procedure CreateAppDirs();
begin
  // ForceDirectories maakt de volledige padstructuur aan in één keer.
  // Bestaat de map al, dan doet het niets — geen fout, geen melding.
  ForceDirectories('C:\Amusing');
  ForceDirectories('C:\Amusing\Amusing');
end;

// Stel NTFS-rechten in zodat gebruikers kunnen lezen/schrijven na installatie
procedure SetDirPermissions();
var
  ResultCode: Integer;
begin
  // icacls is ingebouwd in Windows en past rechten aan zonder extra tools
  Exec('icacls', 'C:\Amusing /grant Users:(OI)(CI)F /T /Q', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  CreateAppDirs();
  SetDirPermissions();
  Result := '';  // Lege string = geen foutmelding, installatie gaat door
end;