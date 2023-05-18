@echo off & setlocal enabledelayedexpansion
cd %~dp0
echo =================start gen proto code=================
set pb_path=pb_schemas
set out_path=..\..\Assets\GameScripts\HotFix\GameProto\GameProtocol
del /f /s /q %out_path%\*.*
for /f "delims=" %%i in ('dir /b %pb_path%') do (
echo ------------%%i start gen
protoc -I=pb_schemas --csharp_out=../../Assets/GameScripts/HotFix/GameProto/GameProtocol pb_schemas\%%i
echo ------------%%i gen success
)
echo =================end gen proto code=================
set cur_path=%~dp0
set outEventPath=../../Assets\GameScripts\HotFix\GameProto\GameProtocol
call ProtobufResolver.exe %cur_path%pb_schemas %outEventPath%
echo =================end gen proto event=================
pause

@REM protoc -I=pb_schemas --csharp_out=Gen pb_schemas\proto_cs.proto proto_cs_common.proto pb_schemas\proto_cs_player.proto

pause