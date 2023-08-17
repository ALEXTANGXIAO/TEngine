cd /d %~dp0
set WORKSPACE=..

set GEN_CLIENT=%WORKSPACE%\Luban\Luban.ClientServer\Luban.ClientServer.exe
set CONF_ROOT=%WORKSPACE%\Luban\Config
set DATA_OUTPUT=%ROOT_PATH%..\GenerateDatas
set CUSTOM_TEMP=%WORKSPACE%\Luban\CustomTemplate_Client_UniTask

xcopy %CUSTOM_TEMP%\ConfigLoader.cs %WORKSPACE%\Assets\GameScripts\HotFix\GameProto\ConfigLoader.cs /s /e /i /y

%GEN_CLIENT% --template_search_path CustomTemplate_Client_UniTask -j cfg --^
 -d %CONF_ROOT%\Defines\__root__.xml ^
 --input_data_dir %CONF_ROOT%\Datas ^
 --output_code_dir %WORKSPACE%/Assets/GameScripts/HotFix/GameProto/GameConfig ^
 --output_data_dir %WORKSPACE%/Assets/AssetRaw/Configs/bytes/ ^
 --gen_types code_cs_unity_bin,data_bin ^
 -s client

echo ======== 生成配置文件结束 ========

pause