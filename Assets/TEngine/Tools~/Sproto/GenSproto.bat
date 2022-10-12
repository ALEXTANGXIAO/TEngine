echo off
set curdir=%~dp0
cd Sprotodump
lua.exe ./sprotodump.lua -cs %curdir%/Game.sproto -o %curdir%/../../Scripts/Runtime/Core/NetWork/Proto/GameSproto.cs