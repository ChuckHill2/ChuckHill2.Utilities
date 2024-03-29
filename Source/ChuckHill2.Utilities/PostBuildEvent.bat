@ECHO OFF
@REM -------------------------------------------------------------------------
@REM Visual Studio release post-build step to:
@REM   Create documentation from the source code
@REM
@REM Prerequsites:
@REM   1. This batch file must reside in the project folder.
@REM   2. Microsoft HTML Help Workshop must be installed.
@REM
@REM Usage:
@REM   $(ProjectDir)PostBuildEvent.bat $(Configuration) $(OutDir)
@REM
@REM Created by Chuck Hill, 07/13/2020
@REM -------------------------------------------------------------------------

SETLOCAL
@REM Batch commandline properties that match MSBuild properties.
SET ProjectDir=%~dp0
SET Configuration=%~1
SET OutDir=%~2

@REM ProjectDir and OutDir have a trailing backslash. We have to remove it.
SET ProjectNameTmp=%ProjectDir:~0,-1%
FOR %%f IN (%ProjectNameTmp%) DO SET ProjectName=%%~nxf

IF /I NOT "%Configuration%"=="Release" GOTO :EOF

@REM Everything is relative to $(ProjectDir)
CD %ProjectDir%

@REM Evaluate relative $(OutDir) to be an absolute path.
@REM $(OutDir) may be an entirely different location and not under $(ProjectDir)
@REM OutDir trailing backslash removed here.
PUSHD %OutDir%
SET OutDir=%CD%
POPD
SET HtmlHelp=%OutDir%\HtmlHelp

@REM Replaceable parameters for Doxygen.config
FOR /F delims^=^"^ tokens^=2 %%G IN ('FINDSTR AssemblyVersion Properties\AssemblyInfo.cs ..\SolutionInfo.cs 2^>NUL') DO SET PROJECT_NUMBER=%%G
REM FOR /F "usebackq" %%G IN (`powershell.exe "[System.Reflection.Assembly]::LoadFrom('%OutDir%\%ProjectName%.dll').GetName().Version.ToString();"`) DO SET PROJECT_NUMBER1=%%G
REM FOR /F "usebackq tokens=3 delims=<>" %%G IN (`FINDSTR ^^^<Version^^^> %ProjectName%.csproj ..\Directory.Build.props 2^>NUL`) DO SET PROJECT_NUMBER=%%G

SET CHM_FILE=%OutDir%\%ProjectName%.chm
SET HHC_LOCATION=%ProgramFiles(x86)%\HTML Help Workshop\hhc.exe
SET OUTPUT_DIRECTORY=%OutDir%
SET GENERATE_HTMLHELP=Yes
SET PROJECT_NAME=%ProjectName%

SET PROJECT_BRIEF=C# WinForms Utility Library

SET CHM_FILE_LOCKED=FALSE
IF EXIST %CHM_FILE% ((call ) 1>>%CHM_FILE%) 2>nul && (SET CHM_FILE_LOCKED=FALSE) || (SET CHM_FILE_LOCKED=TRUE)

IF %CHM_FILE_LOCKED%==TRUE (
ECHO Error: Cannot update %CHM_FILE% while it is still open.
EXIT /B 1
)

IF NOT EXIST "%HHC_LOCATION%" (
ECHO Warning: Unable to build CHM help file. Legacy Microsoft HTML Help Workshop 
ECHO must be installed. This tool is deprecated and no longer found at Microsoft. 
ECHO The installer may be found stored under the SolutionPostBuild project.
ECHO.
ECHO HTML Help Workshop is the *ONLY* available tool that can create CHM help
ECHO files. It cannot be included as a tool here because it uses COM components.
SET GENERATE_HTMLHELP=No
)

IF EXIST %CHM_FILE% DEL /F %CHM_FILE%
IF EXIST %HtmlHelp% RD /S /Q %HtmlHelp%
@REM Doxygen markdown parser is pretty dumb and many markdown features don't work. Be careful.
@REM Copy Readme.md images to target destination because Doxygen wont.
@REM XCOPY ..\..\ReadmeImages %HtmlHelp%\ReadmeImages /S /I

@REM Cannot use nuget because its latest is version 1.8.14. The actual latest is 1.8.20. We need the newer features.
@REM Eek! As of Microsoft Visual Studio Enterprise 2019 Version 16.7.7, and within the 
@REM      post build event, %ProgramFiles%==%ProgramFiles(x86)%==C:\Program Files (x86) !!!
SET DOXYGEN=C:\Program Files\doxygen\bin\doxygen.exe
IF NOT EXIST "%DOXYGEN%" SET DOXYGEN=%ProgramFiles(x86)%\doxygen\bin\doxygen.exe
IF NOT EXIST "%DOXYGEN%" CALL :GETFILE DOXYGEN doxygen.exe
IF NOT EXIST "%DOXYGEN%" (
ECHO Error: Doxygen document generator has not been installed. Cannot continue.
ECHO The latest version may be downloaded from https://www.doxygen.nl/download.html
EXIT /B 1
)

FOR /F "tokens=1,2,3 delims=. " %%A in ('"%DOXYGEN%" -v') DO SET /A "DOXVERSION=%%A << 16 | %%B << 8 | %%C"
IF %DOXVERSION% LSS 67604 (
ECHO Error: The version of %DOXYGEN% is less than 1.8.20. Cannot continue.
ECHO The latest version may be downloaded from https://www.doxygen.nl/download.html
EXIT /B 1
)

ECHO.
ECHO "%DOXYGEN%" Doxygen.config
ECHO.
@REM Doxygen formats errors just like MSBUILD causing build failure, so we have to hide them with '2^>NUL'
"%DOXYGEN%" Doxygen.config 2>NUL
ECHO.

@REM Nuget pack requires chm help file as a part of its build.
IF GENERATE_HTMLHELP==Yes IF NOT EXIST %CHM_FILE%  (
ECHO Error: Failed to build CHM help file. Cannot create nuget package without it.
EXIT /B 1
)

IF NOT %GENERATE_HTMLHELP%==Yes (
ECHO Info: Nuget pack disabled due to required CHM help file not created.
EXIT /B 0
)

CALL :GETFILE NUGET nuget.exe
IF NOT EXIST "%NUGET%" (
ECHO Error: nuget.exe packager is not found. It must be in PATH. Cannot continue.
ECHO The latest version may be downloaded from https://www.nuget.org/downloads
EXIT /B 1
)

ECHO.
ECHO "%NUGET%" pack %ProjectName%.csproj -properties configuration=%Configuration%;OutDir="%OutDir%" -Verbosity detailed -OutputDirectory "%OutDir%"
ECHO.
"%NUGET%" pack %ProjectName%.csproj -properties configuration=%Configuration%;OutDir="%OutDir%" -Verbosity detailed -OutputDirectory "%OutDir%"
ECHO.

EXIT /B 0

:GETFILE
SET %~1=%~$PATH:2
GOTO :EOF

