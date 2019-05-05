@ECHO OFF

::----- Setup Environment -----
set current_path=%~dp0
set MSBUILD="C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
set VERSIONFILE="..\RMTV-recorder\Properties\AssemblyInfo_version.cs"
set VERSIONFILE_COPY="..\RMTV-recorder\Properties\AssemblyInfo_version_copy.cs"
set VERSIONPATTERN=1985.9.9.2018

::----- Input -----
:: Version, Build type
set /P BUILDTYPE=Choose a build type (1) Build (2) Rebuild:
set /P VERSION=Enter version:
echo %VERSION%

::----- Replace the version file -----
copy %VERSIONFILE% %VERSIONFILE_COPY%
for /f "delims=" %%i in ('type "%VERSIONFILE%" ^& break ^> "%VERSIONFILE%" ') do (
        set "line=%%i"
        setlocal enabledelayedexpansion
        >>"%VERSIONFILE%" echo(!line:%VERSIONPATTERN%=%VERSION%!
        endlocal
)
::----- Build solution -----
IF "%BUILDTYPE%" == "2" (
    %MSBUILD% "..\RMTV-recorder.sln" /p:Configuration=Release /t:Clean,Build
 
) ELSE (
    %MSBUILD% "..\RMTV-recorder.sln" /p:Configuration=Release
)

::----- Recover the version file -----
del %VERSIONFILE%
copy %VERSIONFILE_COPY% %VERSIONFILE%
del %VERSIONFILE_COPY%

::----- Message -----
pause