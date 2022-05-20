@echo off
setlocal enabledelayedexpansion  
::Proto文件路径
set SOURCE_PATH=..\..\Assets\TEngine\Scripts\Protobuf\Proto

::C#文件生成路径
set TARGET_PATH=..\..\Assets\TEngine\Scripts\Protobuf\Proto_CSharp

::删除之前创建的文件
del %TARGET_PATH%\*.cs /f /s /q

echo -------------------------------------------------------------

for /R %SOURCE_PATH% %%f in (*.proto) do (
    set "FILE_PATH=%%~nxf"
    echo handle file: !FILE_PATH!
    protogen --proto_path=%SOURCE_PATH% --csharp_out=%TARGET_PATH% !FILE_PATH!
) 
pause