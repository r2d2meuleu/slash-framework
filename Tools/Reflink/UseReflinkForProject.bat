set BATCH_DIR=%~dp0

REM Adjust this path to point to Unity Project folder
set UNITY_PROJECT_PATH=%BATCH_DIR%/../..

REM Reflink framework
call "%SLASH_FRAMEWORK%/Tools/Reflink/UseReflink.bat" "%SLASH_FRAMEWORK%" "%UNITY_PROJECT_PATH%" "%BATCH_DIR%/SlashLibraries.txt"