cd /d %~dp0
set WORKSPACE=..

set GEN_CLIENT=%WORKSPACE%\Tools\Luban.ClientServer\Luban.ClientServer.exe
set CONF_ROOT=%WORKSPACE%\Configs
set DATA_OUTPUT=%ROOT_PATH%..\GenerateDatas
set CUSTOM_TEMP=%WORKSPACE%\Configs\CustomTemplate\CustomTemplate_Client_UniTask

xcopy %CUSTOM_TEMP%\ConfigSystem.cs %WORKSPACE%\UnityProject\Assets\GameScripts\HotFix\GameProto\ConfigSystem.cs /s /e /i /y

%GEN_CLIENT% --template_search_path %CONF_ROOT%\CustomTemplate\CustomTemplate_Client_UniTask -j cfg --^
 -d %CONF_ROOT%\Defines\__root__.xml ^
 --input_data_dir %CONF_ROOT%\Excels^
 --output_code_dir %WORKSPACE%/UnityProject/Assets/GameScripts/HotFix/GameProto/GameConfig ^
 --output_data_dir %WORKSPACE%/UnityProject/Assets/AssetRaw/Configs/bytes/ ^
 --gen_types code_cs_unity_bin,data_bin ^
 -s client

echo ======== 生成配置文件结束 ========

pause