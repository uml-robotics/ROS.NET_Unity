@echo off
set DEST=%~dp0
set DEST=%DEST%COPY_TO_UNITY_PROJECT
goto :findBabies
goto :eof
:findBabies
for /D %%F in (*meshes) do for %%E in (%%~dpF) do ( 
set src=%%E
set MYDIR1=%src:~0,-1%
for %%f in (%MYDIR1%) do set myfolder=%%~nxf
if not "%myfolder%" == "-1" (
echo xcopy /E /I /Y /Q "%MYDIR1%" "%DEST%\%myfolder%"
xcopy /E /I /Y /Q "%MYDIR1%" "%DEST%\%myfolder%"
)
)
for /D %%A in (*) do ( 
 (Echo "%%A" | FIND /I "COPY_TO_UNITY_PROJECT" 1>NUL) || (
 pushd %%A
 call :findBabies
 popd
 )
)
:eof
exit /b