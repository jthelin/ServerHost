@setlocal
@ECHO off

set PROJ_NAME=ServerHost

SET CMDHOME=%~dp0.
if "%FrameworkDir%" == "" set FrameworkDir=%WINDIR%\Microsoft.NET\Framework
if "%FrameworkVersion%" == "" set FrameworkVersion=v4.0.30319

cd /d %CMDHOME%

set TEST_RESULTS_DIR=TestResults
@if NOT EXIST %TEST_RESULTS_DIR% mkdir %TEST_RESULTS_DIR%

@echo ============== Test %PROJ% ==============

set XUNIT_VER=2.1.0

SET CONFIGURATION=Debug

@echo ==== Test %CONFIGURATION% ====

set TESTS=Tests\bin\%CONFIGURATION%\Tests.dll

set XUNIT_OPTS=-xml %TEST_RESULTS_DIR%\TestResults.xml

packages\xunit.runner.console.%XUNIT_VER%\tools\xunit.console.exe %TESTS% %XUNIT_OPTS%

@if ERRORLEVEL 1 GOTO :ErrorStop
@echo Test ok for %CONFIGURATION% %PROJ%

@echo ======= Test succeeded for %PROJ% =======
@GOTO :EOF

:ErrorStop
set RC=%ERRORLEVEL%
if "%STEP%" == "" set STEP=%CONFIGURATION%
@echo ===== Test FAILED for %PROJ% -- %STEP% with error %RC% - EXITING =====
exit /B %RC%
:EOF
