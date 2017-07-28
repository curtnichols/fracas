SETLOCAL

SET PACKAGE_VERSION=0.7

..\.nuget\nuget.exe pack -Version %PACKAGE_VERSION% fracas.nuspec

ENDLOCAL
