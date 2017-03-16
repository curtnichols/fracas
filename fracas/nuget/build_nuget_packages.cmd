SETLOCAL

SET PACKAGE_VERSION=0.6

..\.nuget\nuget.exe pack -Version %PACKAGE_VERSION% fracas.nuspec

ENDLOCAL
