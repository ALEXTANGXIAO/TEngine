@REM @echo off & setlocal enabledelayedexpansion
@REM cd %~dp0
@REM echo =================start gen proto code=================
@REM set pb_path=pb_message
@REM set out_path=..\..\Assets\GameMain\HotFix\Scripts\Proto
@REM del /f /s /q %out_path%\*.*
@REM for /f "delims=" %%i in ('dir /b %pb_path%') do (
@REM echo ------------%%i start gen
@REM protoc -I=pb_message --csharp_out=../../Assets/GameMain/HotFix/Scripts/Proto pb_message\%%i
@REM echo ------------%%i gen success
@REM )
@REM echo =================end gen proto code=================
@REM set cur_path=%~dp0
@REM set outEventPath=../../Assets\GameMain\HotFix\Scripts\Proto\Definition\Constant
@REM call ProtobufResolver.exe %cur_path%pb_message %outEventPath%
@REM echo =================end gen proto event=================
@REM pause

protoc -I=pb_schemas --csharp_out=Gen pb_schemas\ProtoBase.proto

pause