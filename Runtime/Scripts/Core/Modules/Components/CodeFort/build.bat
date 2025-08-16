@echo off
REM -------------------------------------------------------
REM Build CodeFort DLL and copy to Unity package
REM -------------------------------------------------------

REM Set paths
set SOURCE_DIR=%~dp0
set BUILD_DIR=%SOURCE_DIR%build
set UNITY_PLUGIN_DIR=%SOURCE_DIR%..\..\..\..\..\Plugins\x86_64

REM Create build folder if it doesn't exist
if not exist "%BUILD_DIR%" mkdir "%BUILD_DIR%"

REM Navigate to build folder
cd "%BUILD_DIR%"

REM Run CMake configure and build
cmake .. -DCMAKE_BUILD_TYPE=Release
cmake --build . --config Release

REM Copy the DLL to Unity plugin folder
if not exist "%UNITY_PLUGIN_DIR%" mkdir "%UNITY_PLUGIN_DIR%"
copy /Y "%BUILD_DIR%\Release\CodeFort.dll" "%UNITY_PLUGIN_DIR%"

REM Delete the build folder
cd "%SOURCE_DIR%"
rmdir /S /Q "%BUILD_DIR%"

echo.
echo DLL copied to Unity package: %UNITY_PLUGIN_DIR%
pause
