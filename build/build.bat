@ECHO OFF

::----- Setup Environment -----
set current_path=%~dp0
set MSBUILD="C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
set VERSIONFILE="..\RMTV-recorder\Properties\AssemblyInfo_version.cs"
set VERSIONFILE_COPY="..\RMTV-recorder\Properties\AssemblyInfo_version_copy.cs"
set VERSIONPATTERN=1985.9.9.2018
set HISTORYFILE=release-history.txt
set PROGRAMNAME=RMTV-recorder
set ZIPPATH="C:\Program Files\7-Zip\7z.exe"
set RELEASEPATH="..\RMTV-recorder\bin\Release"

::----- Get Date -----
set date=%date%
set year=%date:~0,4%
set month=%date:~5,2%
set day=%date:~8,2%
set date_frm=%year%-%month%-%day%
::echo %date_frm%

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

::----- Pack Files -----
set PACKDIR=%PROGRAMNAME%_%VERSION%
mkdir %PACKDIR%
copy %RELEASEPATH%\RMTV-recorder.exe %PACKDIR%
xcopy /s /e /h /I /y %RELEASEPATH%\Resource %PACKDIR%\Resource

%ZIPPATH% a -r %PACKDIR%.zip %PACKDIR%
rmdir /s/q %PACKDIR%

::----- Update History File -----
If not exist "%HISTORYFILE%" (
	type nul> "%HISTORYFILE%"
)

echo Version %VERSION%: %date_frm% >%HISTORYFILE%.new
type %HISTORYFILE% >>%HISTORYFILE%.new
move /y %HISTORYFILE%.new %HISTORYFILE%

::----- Recover the version file -----
del %VERSIONFILE%
copy %VERSIONFILE_COPY% %VERSIONFILE%
del %VERSIONFILE_COPY%

::----- End -----
pause