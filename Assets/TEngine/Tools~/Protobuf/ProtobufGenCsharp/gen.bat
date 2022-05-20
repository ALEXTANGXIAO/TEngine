@echo off

echo param[0] = %0
echo param[1] = %1

protogen --proto_path=..\..\Assets\Framework\Scripts\Rpc\Proto --csharp_out=..\..\Assets\Framework\Scripts\Rpc\protobuf-net %1

pause