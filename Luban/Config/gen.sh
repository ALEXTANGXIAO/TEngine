#!/bin/zsh
GEN_CLIENT=../Tools/Luban.ClientServer/Luban.ClientServer.dll

dotnet ${GEN_CLIENT} -j cfg --\
 -d Defines/__root__.xml \
 --input_data_dir Datas \
 --output_data_dir output_json \
 --output_code_dir Gen \
 --gen_types code_cs_unity_json,data_json \
 -s all 
