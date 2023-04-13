@echo off & setlocal enabledelayedexpansion
cd %~dp0
echo =================start gen proto code=================
set pb_path=pb_message
set out_path=..\..\Assets\GameMain\HotFix\Scripts\Proto
del /f /s /q %out_path%\*.*
for /f "delims=" %%i in ('dir /b %pb_path%') do (
echo ------------%%i start gen
protoc -I=pb_message --csharp_out=../../Assets/GameMain/HotFix/Scripts/Proto pb_message\%%i
echo ------------%%i gen success
)
echo =================end gen proto code=================
set cur_path=%~dp0
set outEventPath=../../Assets\GameMain\HotFix\Scripts\Proto\Definition\Constant
call ProtobufResolver.exe %cur_path%pb_message %outEventPath%
echo =================end gen proto event=================
pause

