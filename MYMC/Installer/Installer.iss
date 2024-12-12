#define MyAppName "Mini Youtube Music Controller"
#define MyAppVersion "0.0.0.6"
#define MyAppPublisher "Mustafa Can Yucel"
#define MyAppExeName "MYMC.exe"

[Setup]
AppId={{7461b952-e3d2-46e7-a1fb-c4fe1d48427c}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={commonpf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=EULA.txt
OutputDir=Output
OutputBaseFilename=MYMC_Setup
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Main Application
Source: "bin\*"; Excludes: "*.pdb"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Code]
function HasDotNet8: Boolean;
var
  Path: string;
  FindRec: TFindRec;
begin
  Result := False;
  Path := ExpandConstant('{commonpf}\dotnet\shared\Microsoft.WindowsDesktop.App');
  
  if DirExists(Path) then
  begin
    if FindFirst(Path + '\9.*', FindRec) then
    begin
      try
        Result := True;
      finally
        FindClose(FindRec);
      end;
    end;
  end;
end;

function InitializeSetup: Boolean;
begin
    Result := False;
    
    if not HasDotNet8 then
    begin
        MsgBox('.NET 9.0 Desktop Runtime is required to run this application.' + #13#10 +
               'Please install it from https://dotnet.microsoft.com/download/dotnet/9.0', 
               mbCriticalError, MB_OK);
        Exit;
    end;
    
    Result := True;
end;

