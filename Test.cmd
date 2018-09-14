@setlocal
@ECHO off

set PROJ_NAME=ServerHost

@REM Note: Complex values for TEST_CATEGORY can be passed in through environment variable 
@REM       (as workaround for some limitations of Windows cmd script calling)
@REM       Command line arguments passed in to this script will override any env variable settings.
@REM       For example, "Test.cmd BVT" will just run TestCategory = BVT (Build Verification Tests)

if NOT .%1. == .. set TEST_CATEGORY=%1

if NOT .%TEST_CATEGORY%. == .. (
  @echo Using TestCategory = %TEST_CATEGORY%
)

SET CONFIGURATION=Release

set USE_BINARIES_DIR=False

SET CMDHOME=%~dp0
@REM Remove trailing backslash \
set CMDHOME=%CMDHOME:~0,-1%

if "%FrameworkDir%" == "" set FrameworkDir=%WINDIR%\Microsoft.NET\Framework
if "%FrameworkVersion%" == "" set FrameworkVersion=v4.0.30319

SET VS_IDE_DIR=%VS120COMNTOOLS%..\IDE
SET VSTEST_EXE_DIR=%VS_IDE_DIR%\CommonExtensions\Microsoft\TestWindow
SET VSTEST_EXE=%VSTEST_EXE_DIR%\VSTest.console.exe

set XUNIT_VER=2.4.0
set XUNIT_DOTNET_PLATFORM_VER=net462
SET XUNIT_EXE_DIR=%CMDHOME%\packages\xunit.runner.console.%XUNIT_VER%\tools\%XUNIT_DOTNET_PLATFORM_VER%
SET XUNIT_EXE=%XUNIT_EXE_DIR%\xunit.console.exe

cd "%CMDHOME%"
@cd

set PROJ=%PROJ_NAME%

@if "%USE_BINARIES_DIR%" == "True" SET OutDir=Binaries\%CONFIGURATION%

set TEST_RESULTS_DIR=TestResults
@if NOT EXIST %TEST_RESULTS_DIR% mkdir %TEST_RESULTS_DIR%
set XUNIT_OPTS=-xml %TEST_RESULTS_DIR%\TestResults.xml

@echo ==== Test %PROJ% %CONFIGURATION% ====

@if "%USE_BINARIES_DIR%" == "True" (
  set TESTS=%OutDir%\ServerHost.Tests.dll
) else (
  set TESTS=Tests\Tests.Net46\bin\%CONFIGURATION%\ServerHost.Tests.dll
)
@Echo Test assemblies = %TESTS%

if NOT .%TEST_CATEGORY%. == .. (
  set TEST_ARGS= -trait "Category=%TEST_CATEGORY%"
)
set TEST_ARGS= %TEST_ARGS% %XUNIT_ARGS%

@Echo ON

"%XUNIT_EXE%" %TESTS% %TEST_ARGS% 

@if ERRORLEVEL 1 GOTO :ErrorStop
@echo Test ok for %PROJ% %CONFIGURATION%

@echo ======= Test succeeded for %PROJ% =======
@GOTO :EOF

:ErrorStop
set RC=%ERRORLEVEL%
@if "%STEP%" == "" set STEP=%CONFIGURATION%
@echo ===== Test FAILED for %PROJ% -- %STEP% with error %RC% - EXITING =====
exit /B %RC%

:EOF
exit /B 0
