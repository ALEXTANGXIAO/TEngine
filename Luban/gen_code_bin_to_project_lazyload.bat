cd /d %~dp0
set WORKSPACE=..

set GEN_CLIENT=%WORKSPACE%\Luban\Luban.ClientServer\Luban.ClientServer.exe
set CONF_ROOT=%WORKSPACE%\Luban\Config
set DATA_OUTPUT=%ROOT_PATH%..\GenerateDatas
set CUSTOM_TEMP=%WORKSPACE%\Luban\CustomTemplate_Client_LazyLoad

xcopy %CUSTOM_TEMP%\ConfigLoader.cs %WORKSPACE%\Assets\GameScripts\HotFix\GameProto\ConfigLoader.cs /s /e /i /y

%GEN_CLIENT% -j cfg --^
 -d %CONF_ROOT%\Defines\__root__.xml ^
 --input_data_dir %CONF_ROOT%\Datas ^
 --output_data_dir %WORKSPACE%/Assets/AssetRaw/Configs/bytes/ ^
 --gen_types data_bin ^
 -s client 

%GEN_CLIENT% --template_search_path CustomTemplate_Client_LazyLoad -j cfg --^
 -d %CONF_ROOT%\Defines\__root__.xml ^
 --input_data_dir %CONF_ROOT%\Datas ^
 --output_code_dir %WORKSPACE%/Assets/GameScripts/HotFix/GameProto/GameConfig ^
 --output_data_dir ..\GenerateDatas\bidx ^
 --gen_types code_cs_unity_bin,data_bidx ^
 -s client 

echo ======== 生成配置文件结束 ========
set WORKSPACE=..

set "prefix=Idx_"

for %%a in (%DATA_OUTPUT%\bidx\*) do (
    ren "%%a" "Idx_%%~nxa"
)

echo ======== 所有文件已添加前缀 ========

xcopy %DATA_OUTPUT%\bidx\ %WORKSPACE%\Assets\AssetRaw\Configs\bidx\ /s /e /i /y

pause