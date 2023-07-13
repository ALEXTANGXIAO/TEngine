using System;
using System.IO;
using TEngine.DataStructure;
using TEngine.Core;
#pragma warning disable CS8603
#pragma warning disable CS8600
#pragma warning disable CS8625

namespace TEngine.Core.Network
{
    public sealed class OuterPackInfo : APackInfo
    {
        public static OuterPackInfo Create()
        {
            return Pool<OuterPackInfo>.Rent();
        }
        
        public static OuterPackInfo Create(uint rpcId, uint protocolCode, long routeTypeCode)
        {
            var outerPackInfo = Pool<OuterPackInfo>.Rent();
            outerPackInfo.RpcId = rpcId;
            outerPackInfo.ProtocolCode = protocolCode;
            outerPackInfo.RouteTypeCode = routeTypeCode;
            return outerPackInfo;
        }

        public override void Dispose()
        {
            base.Dispose();
            Pool<OuterPackInfo>.Return(this);
        }
        
        public override object Deserialize(Type messageType)
        {
            using (MemoryStream)
            {
                MemoryStream.Seek(Packet.OuterPacketHeadLength, SeekOrigin.Begin);
                return ProtoBufHelper.FromStream(messageType, MemoryStream);
            }
        }
    }

    public sealed class OuterPacketParser : APacketParser
    {
        private uint _rpcId;
        private uint _protocolCode;
        private long _routeTypeCode;
        private int _messagePacketLength;
        private bool _isUnPackHead = true;
        private readonly byte[] _messageHead = new byte[Packet.OuterPacketHeadLength];

        public override bool UnPack(CircularBuffer buffer, out APackInfo packInfo)
        {
            packInfo = null;

            while (!IsDisposed)
            {
                if (_isUnPackHead)
                {
                    if (buffer.Length < Packet.OuterPacketHeadLength)
                    {
                        return false;
                    }

                    _ = buffer.Read(_messageHead, 0, Packet.OuterPacketHeadLength);
                    _messagePacketLength = BitConverter.ToInt32(_messageHead, 0);
#if TENGINE_NET
                    if (_messagePacketLength > Packet.PacketBodyMaxLength)
                    {
                        throw new ScanException($"The received information exceeds the maximum limit = {_messagePacketLength}");
                    }
#endif
                    _protocolCode = BitConverter.ToUInt32(_messageHead, Packet.PacketLength);
                    _rpcId = BitConverter.ToUInt32(_messageHead, Packet.OuterPacketRpcIdLocation);
                    _routeTypeCode = BitConverter.ToUInt16(_messageHead, Packet.OuterPacketRouteTypeOpCodeLocation);
                    _isUnPackHead = false;
                }

                try
                {
                    if (buffer.Length < _messagePacketLength)
                    {
                        return false;
                    }
                
                    _isUnPackHead = true;
                    packInfo = OuterPackInfo.Create(_rpcId, _protocolCode, _routeTypeCode);

                    if (_messagePacketLength <= 0)
                    {
                        return true;
                    }
                    
                    var memoryStream = MemoryStreamHelper.GetRecyclableMemoryStream();
                    // 写入消息体的信息到内存中
                    memoryStream.Seek(Packet.OuterPacketHeadLength, SeekOrigin.Begin);
                    buffer.Read(memoryStream, _messagePacketLength);
                    // 写入消息头的信息到内存中
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.Write(BitConverter.GetBytes(_messagePacketLength));
                    memoryStream.Write(BitConverter.GetBytes(_protocolCode));
                    memoryStream.Write(BitConverter.GetBytes(_rpcId));
                    memoryStream.Write(BitConverter.GetBytes(_routeTypeCode));
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    packInfo.MemoryStream = memoryStream;
                    return true;
                }
                catch (Exception e)
                {
                    packInfo?.Dispose();
                    Log.Error(e);
                    return false;
                }
            }

            return false;
        }

        public override APackInfo UnPack(MemoryStream memoryStream)
        {
            OuterPackInfo packInfo = null;
            
            try
            {
                if (memoryStream == null)
                {
                    return null;
                }

                if (memoryStream.Length < Packet.OuterPacketHeadLength)
                {
                    return null;
                }

                _ = memoryStream.Read(_messageHead, 0, Packet.OuterPacketHeadLength);
                _messagePacketLength = BitConverter.ToInt32(_messageHead, 0);
#if TENGINE_NET
                if (_messagePacketLength > Packet.PacketBodyMaxLength)
                {
                    throw new ScanException($"The received information exceeds the maximum limit = {_messagePacketLength}");
                }
#endif
                packInfo = OuterPackInfo.Create();
                packInfo.ProtocolCode = BitConverter.ToUInt32(_messageHead, Packet.PacketLength);
                packInfo.RpcId = BitConverter.ToUInt32(_messageHead, Packet.OuterPacketRpcIdLocation);
                packInfo.RouteTypeCode = BitConverter.ToUInt16(_messageHead, Packet.OuterPacketRouteTypeOpCodeLocation);

                if (memoryStream.Length < _messagePacketLength)
                {
                    return null;
                }

                if (_messagePacketLength <= 0)
                {
                    return packInfo;
                }
                
                var outMemoryStream = MemoryStreamHelper.GetRecyclableMemoryStream();
                memoryStream.WriteTo(outMemoryStream);
                outMemoryStream.Seek(0, SeekOrigin.Begin);
                packInfo.MemoryStream = outMemoryStream;
                return packInfo;
            }
            catch (Exception e)
            {
                packInfo?.Dispose();
                Log.Error(e);
                return null;
            }
        }

        public static MemoryStream Pack(uint rpcId, long routeTypeOpCode, MemoryStream memoryStream)
        {
            memoryStream.Seek(Packet.OuterPacketRpcIdLocation, SeekOrigin.Begin);
            memoryStream.Write(BitConverter.GetBytes(rpcId));
            memoryStream.Write(BitConverter.GetBytes(routeTypeOpCode));
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        public static MemoryStream Pack(uint rpcId, long routeTypeOpCode, object message)
        {
            var opCode = Opcode.PingRequest;
            var packetBodyCount = 0;
            var memoryStream = MemoryStreamHelper.GetRecyclableMemoryStream();
            memoryStream.Seek(Packet.OuterPacketHeadLength, SeekOrigin.Begin);

            if (message != null)
            {
                ProtoBufHelper.ToStream(message, memoryStream);
                opCode = MessageDispatcherSystem.Instance.GetOpCode(message.GetType());
                packetBodyCount = (int)(memoryStream.Position - Packet.OuterPacketHeadLength);
            }

            if (packetBodyCount > Packet.PacketBodyMaxLength)
            {
                throw new Exception($"Message content exceeds {Packet.PacketBodyMaxLength} bytes");
            }
            
            memoryStream.Seek(0, SeekOrigin.Begin);
            memoryStream.Write(BitConverter.GetBytes(packetBodyCount));
            memoryStream.Write(BitConverter.GetBytes(opCode));
            memoryStream.Write(BitConverter.GetBytes(rpcId));
            memoryStream.Write(BitConverter.GetBytes(routeTypeOpCode));
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        public override void Dispose()
        {
            _messagePacketLength = 0;
            Array.Clear(_messageHead, 0, _messageHead.Length);
            base.Dispose();
        }
    }
}