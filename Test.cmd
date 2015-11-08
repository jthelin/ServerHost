@setlocal
@ECHO off

set PROJ_NAME=ServerHost

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

set XUNIT_VER=2.1.0
SET XUNIT_EXE_DIR=%CMDHOME%\packages\xunit.runner.console.%XUNIT_VER%\tools
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
  set TESTS=%OutDir%\Tests.dll
) else (
  set TESTS=Tests\bin\%CONFIGURATION%\Tests.dll
)
@Echo Test assemblies = %TESTS%

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
