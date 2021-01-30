@setlocal
@ECHO off

set PROJ_NAME=ServerHost

@REM Note: Complex values for TEST_CATEGORY can be passed in through environment variable 
@REM       (as workaround for some limitations of Windows cmd script calling)
@REM       Command line arguments passed in to this script will override any env variable settings.
@REM       For example, "Test.cmd BVT" will just run TestCategory = BVT (Build Verification Tests)

if NOT .%1. == .. (
  set TEST_CATEGORY=%1
  shift
)

if NOT .%TEST_CATEGORY%. == .. (
  @echo Using TestCategory = %TEST_CATEGORY%
)

SET CONFIGURATION=Release

SET CMDHOME=%~dp0
@REM Remove trailing backslash \
set CMDHOME=%CMDHOME:~0,-1%

cd "%CMDHOME%"
@cd

set PROJ=%PROJ_NAME%

@echo ==== Test %PROJ% %CONFIGURATION% ====

set TESTS=Tests\Tests.DotNetFramework\bin\%CONFIGURATION%\ServerHost.Tests.dll
@Echo Test assemblies = %TESTS%

if NOT .%TEST_CATEGORY%. == .. (
  set TEST_ARGS= --filter "Category=%TEST_CATEGORY%"
)

@Echo ON

dotnet test %TEST_ARGS%

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
