#!/bin/bash

xunit_version=2.4.1
dotnet_version=net461

mono ~/.nuget/packages/xunit.runner.console/$xunit_version/tools/$dotnet_version/xunit.console.exe Tests/Tests.Net46/bin/Debug/ServerHost.Tests.dll

