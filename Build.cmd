@setlocal
@ECHO off

set PROJ_NAME=ServerHost

SET CMDHOME=%~dp0.
if "%FrameworkDir%" == "" set FrameworkDir=%WINDIR%\Microsoft.NET\Framework
if "%FrameworkVersion%" == "" set FrameworkVersion=v4.0.30319

SET MSBUILDEXEDIR=%FrameworkDir%\%FrameworkVersion%
SET MSBUILDEXE=%MSBUILDEXEDIR%\MSBuild.exe

if "%builduri%" == "" set builduri=Build.cmd

SET BinariesDir=%CMDHOME%\Binaries

set PROJ=%CMDHOME%\%PROJ_NAME%.sln

@echo ===== Building %PROJ% =====

@echo Restore NuGet packages ===================

nuget restore


@echo Build Debug ==============================

SET CONFIGURATION=Debug
SET OutDir=%BinariesDir%\%CONFIGURATION%

SET STEP=%CONFIGURATION%

"%MSBUILDEXE%" /p:Configuration=%CONFIGURATION% "%PROJ%"
@if ERRORLEVEL 1 GOTO :ErrorStop
@echo BUILD ok for %CONFIGURATION% %PROJ%

@echo Build Release ============================

SET CONFIGURATION=Release
SET OutDir=%BinariesDir%\%CONFIGURATION%

SET STEP=%CONFIGURATION%

"%MSBUILDEXE%" /p:Configuration=%CONFIGURATION% "%PROJ%"
@if ERRORLEVEL 1 GOTO :ErrorStop
@echo BUILD ok for %CONFIGURATION% %PROJ%

@echo NuGet Package ============================

SET STEP=NuGet

if "%APPVEYOR%" == "True" (
    @echo == Skipping NuGet pack in Appveyor build
) else if "%Configuration%" == "Release" (

  nuget pack %PROJ_NAME%.nuspec

  @if ERRORLEVEL 1 GOTO :ErrorStop
  @echo Nuget Pack ok
)

@echo ======= Build succeeded for %PROJ% =======
@GOTO :EOF

:ErrorStop
set RC=%ERRORLEVEL%
if "%STEP%" == "" set STEP=%CONFIGURATION%
@echo ===== Build FAILED for %PROJ% -- %STEP% with error %RC% - CANNOT CONTINUE =====
exit /B %RC%
:EOF
