#/usr/bin/bash

PROJ=ServerHost.sln

nuget restore $PROJ && msbuild $PROJ
