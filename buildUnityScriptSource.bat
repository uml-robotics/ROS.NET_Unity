@echo off
for /D %%A in (C:\Windows\Microsoft.NET\Framework*) do IF EXIST %%A\v4* for /f "tokens=*" %%B in ('dir /b /a:d %%A') do set TOOLSPATH=%%A\%%B
if NOT DEFINED TOOLSPATH goto :Fail
if NOT EXIST %TOOLSPATH%\msbuild.exe goto :Fail

REM Compile YAMLParser, and generate Messages CSPROJ
%TOOLSPATH%\msbuild.exe JustBuildMessages.sln /t:rebuild

REM clean "copy/paste from here" folder
if EXIST COPY_TO_UNITY_PROJECT\ rmdir /Q /S COPY_TO_UNITY_PROJECT\
mkdir COPY_TO_UNITY_PROJECT
mkdir COPY_TO_UNITY_PROJECT\Resources
REM copy required source directories (including generated messages)
xcopy /E /I /Y /Q .ros.net\ROS_Comm COPY_TO_UNITY_PROJECT\ROS_Comm\
xcopy /E /I /Y /Q .ros.net\tf.net COPY_TO_UNITY_PROJECT\tf.net\
xcopy /E /I /Y /Q .ros.net\XmlRpc_Wrapper COPY_TO_UNITY_PROJECT\XmlRpc_Wrapper
xcopy /E /I /Y /Q .ros.net\MeshLib COPY_TO_UNITY_PROJECT\MeshLib\
xcopy /E /I /Y /Q Messages COPY_TO_UNITY_PROJECT\Messages\

REM cleanup DLLs to avoid duplicate definitions on windows (or other issues on different unity platforms and/or targets)
IF EXIST COPY_TO_UNITY_PROJECT\Messages\Messages.dll del COPY_TO_UNITY_PROJECT\Messages\Messages.dll
for /D %%D in (COPY_TO_UNITY_PROJECT\*) do (
	IF EXIST %%D\bin rmdir /Q /S %%D\bin
	IF EXIST %%D\obj rmdir /Q /S %%D\obj
	IF EXIST %%D\properties rmdir /Q /S %%D\Properties
)
CopyMeshes.bat
goto :eof
:Fail
echo "Could not locate msbuild! This build script should work with .NET build tools version >v4.0.*"
echo "If you have not installed visual studio 2008 or higher, or have not installed .NET 3.5 and/or 4.0, do so"
echo "Otherwise, open UnityROS.NETHack.sln in Visual Studio, and verify that it builds that way."
:eof