@setlocal
@ECHO off

set PROJ_NAME=ServerHost

SET CMDHOME=%~dp0
@REM Remove trailing backslash \
set CMDHOME=%CMDHOME:~0,-1%

@REM Get path to MSBuild Binaries
set ProgramFiles_x86=%ProgramFiles(x86)%
for %%p in (BuildTools Enterprise Professional Community) do (
    if exist "%ProgramFiles_x86%\Microsoft Visual Studio\2017\%%p" (
      SET VS_Product=%%p
      @GOTO :FoundMsBuildDir
    )
)

@echo Could not find Visual Studio 2017 on this machine. Cannot continue.
exit /b 1

:FoundMsBuildDir
SET MSBUILDEXEDIR=%ProgramFiles_x86%\Microsoft Visual Studio\2017\%VS_Product%\MSBuild\15.0\Bin
@ECHO MsBuildExeDir=%MSBUILDEXEDIR%
@REM Can't multi-block if statement when check condition contains '(' and ')' char, so do as single line checks
if NOT "%MSBUILDEXEDIR%" == "" SET MSBUILDEXE=%MSBUILDEXEDIR%\MSBuild.exe
if NOT "%MSBUILDEXEDIR%" == "" GOTO :MsBuildFound

@REM Try to find VS command prompt init script
where /Q MsBuild.exe
if ERRORLEVEL 1 (
    @echo Could not find MSBuild in the system. Cannot continue.
    exit /b 1
) else (
    @REM MsBuild.exe is in PATH, so just use it.
   SET MSBUILDEXE=MSBuild.exe
 )
:MsBuildFound

SET MSBUILDOPT=/verbosity:minimal

if "%builduri%" == "" set builduri=Build.cmd
set USE_BINARIES_DIR=False

set PROJ=%CMDHOME%\%PROJ_NAME%.sln

@echo ===== Building %PROJ% =====

@if "%USE_BINARIES_DIR%" == "True" SET BinariesDir=%CMDHOME%\Binaries

@echo Restore NuGet packages ===================
SET STEP=NuGet-Restore

call nuget-restore.cmd

@if ERRORLEVEL 1 GOTO :ErrorStop

@echo Build Debug ==============================
SET STEP=Debug

SET CONFIGURATION=%STEP%

@if "%USE_BINARIES_DIR%" == "True" SET OutDir=%BinariesDir%\%CONFIGURATION%

"%MSBUILDEXE%" /p:Configuration=%CONFIGURATION% %MSBUILDOPT% "%PROJ%"
@if ERRORLEVEL 1 GOTO :ErrorStop
@echo BUILD ok for %CONFIGURATION% %PROJ%

@echo Build Release ============================
SET STEP=Release

SET CONFIGURATION=%STEP%

@if "%USE_BINARIES_DIR%" == "True" SET OutDir=%BinariesDir%\%CONFIGURATION%

"%MSBUILDEXE%" /p:Configuration=%CONFIGURATION% %MSBUILDOPT% "%PROJ%"
@if ERRORLEVEL 1 GOTO :ErrorStop
@echo BUILD ok for %CONFIGURATION% %PROJ%

if EXIST %PROJ_NAME%.nuspec (
  @echo ===== Build NuGet package for %PROJ% =====
  SET STEP=NuGet-Pack

  nuget pack %PROJ_NAME%.nuspec -Symbols -NonInteractive
  @if ERRORLEVEL 1 GOTO :ErrorStop
  @echo NuGet package ok for %PROJ%
) else (
  @echo NO NuGet package spec file found
)

@echo ===== Build succeeded for %PROJ% =====

@GOTO :EOF

:ErrorStop
set RC=%ERRORLEVEL%
if "%STEP%" == "" set STEP=%CONFIGURATION%
@echo ===== Build FAILED for %PROJ% -- %STEP% with error %RC% - CANNOT CONTINUE =====
exit /B %RC%
:EOF
