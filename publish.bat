@echo off
IF "%~1" NEQ "" set EXE_VERSION=-%1
set SOLUTION_PATH=%~p0
set APP_NAME=RokitIgniter
set PROJECT_PATH=%SOLUTION_PATH%
set PUBLISH_PATH=%PROJECT_PATH%\bin\Publish
set ZIP_EXE_PATH="C:\Program Files\7-Zip\7z.exe"
if exist %PUBLISH_PATH%\NUL del /f/s/q %PUBLISH_PATH% > nul & rmdir /s/q %PUBLISH_PATH%

cd %PROJECT_PATH%

::regular
echo publishing regular version...
dotnet publish --nologo -r win-x64 -c Release --self-contained false -p:PublishSingleFile=true -o %PUBLISH_PATH%\TEMP

cd %PUBLISH_PATH%\TEMP
%ZIP_EXE_PATH% a -t7z -r -mx9 ..\%APP_NAME%%EXE_VERSION%.7z *.*
cd %PROJECT_PATH%
del /f/s/q %PUBLISH_PATH%\TEMP > nul & rmdir /s/q %PUBLISH_PATH%\TEMP

:: regular self-contained
echo publishing regular self-contained version...
dotnet publish --nologo -r win-x64 -c Release --self-contained true -p:PublishSingleFile=true -o %PUBLISH_PATH%\TEMP

cd %PUBLISH_PATH%\TEMP
%ZIP_EXE_PATH% a -t7z -r -mx9 ..\%APP_NAME%-SelfContained%EXE_VERSION%.7z *.*
cd %PROJECT_PATH%
del /f/s/q %PUBLISH_PATH%\TEMP > nul & rmdir /s/q %PUBLISH_PATH%\TEMP

cd %SOLUTION_PATH%
