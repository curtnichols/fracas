SETLOCAL

SET PACKAGE_VERSION=0.4

..\.nuget\nuget.exe pack -Version %PACKAGE_VERSION% fracas.nuspec

ENDLOCAL
