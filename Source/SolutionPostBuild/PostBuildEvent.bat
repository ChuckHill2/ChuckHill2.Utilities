@ECHO OFF
@REM -------------------------------------------------------------------------
@REM Visual Studio release post-build step to:
@REM   Create GitHub folder of release binaries
@REM
@REM Usage:
@REM   $(ProjectDir)PostBuildEvent.bat $(Configuration)
@REM
@REM Created by Chuck Hill, 02/10/2021
@REM -------------------------------------------------------------------------

SETLOCAL
SET Configuration=%~1
IF /I NOT "%Configuration%"=="Release" GOTO :EOF

@REM Batch commandline properties that match MSBuild properties.
SET ProjectDir=%~dp0

@REM ProjectDir has a trailing backslash. We have to remove it.
SET ProjectDir=%ProjectDir:~0,-1%

SET OutDir=%ProjectDir%\bin
SET SolutionDir=%ProjectDir%\..

@REM Evaluate relative $(OutDir) to be an absolute path.
@REM $(OutDir) may be an entirely different location and not under $(ProjectDir)
@REM OutDir trailing backslash removed here.
PUSHD %OutDir%
SET OutDir=%CD%
POPD 
PUSHD %SolutionDir%
SET SolutionDir=%CD%
POPD 

@REM Everything is relative to $(ProjectDir)
CD %ProjectDir%

CALL :ECHOCOPY %SolutionDir%\ChuckHill2.Utilities\bin\%Configuration%\ChuckHill2.Utilities.chm
CALL :ECHOCOPY %SolutionDir%\ChuckHill2.Utilities\bin\%Configuration%\ChuckHill2.Utilities.dll
CALL :ECHOCOPY %SolutionDir%\ChuckHill2.Utilities\bin\%Configuration%\ChuckHill2.Utilities.pdb
CALL :ECHOCOPY %SolutionDir%\LoggerDemo\bin\%Configuration%\LoggerDemo.exe
CALL :ECHOCOPY %SolutionDir%\LoggerEditor\bin\%Configuration%\ChuckHill2.LoggerEditor.exe
CALL :ECHOCOPY %SolutionDir%\UtilitiesDemo\bin\%Configuration%\UtilitiesDemo.exe
CALL :ECHOCOPY %SolutionDir%\XmlDiffMergeDemo\bin\%Configuration%\XmlDiffMergeDemo.exe

EXIT /B 0

:ECHOCOPY
ECHO Copying %~1 to %OutDir%
COPY /Y %~1 %OutDir%
GOTO :EOF
