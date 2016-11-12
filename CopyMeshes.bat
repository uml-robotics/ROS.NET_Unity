@echo off
set BASE=%~dp0
set DEST=%BASE%COPY_TO_UNITY_PROJECT\Resources
goto :start
goto :eof
:findBabies
REM echo IN %cd%
if EXIST meshes for /D %%F in (*meshes) do (
if "%%F" == "meshes" (
for %%f in (%cd%) do set myfolder=%%~nxf
@echo on
xcopy /E /I /Y /Q "%cd%" "%DEST%\%myfolder%"
@echo off
)
if not "%%F" == "meshes" (
set src=%%~dpF
set MYDIR1=%src:~0,-1%
for %%f in (%MYDIR1%) do set myfolder=%%~nxf
@echo on
xcopy /E /I /Y /Q "%MYDIR1%" "%DEST%\%myfolder%"
@echo off
)
)
:start
for /D %%A in (*) do (
(Echo "%%A" | FIND /I "COPY_TO_UNITY_PROJECT" 1>NUL) || (
pushd %%A
call :findBabies
popd
)
)
goto :eof
:error
echo %cd%
echo SOMETHING AIN'T RIGHT
exit 1
:eof
exit /b