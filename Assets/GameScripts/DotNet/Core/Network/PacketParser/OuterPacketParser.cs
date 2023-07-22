using System;
using System.Buffers;
using System.IO;
using TEngine.DataStructure;
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

namespace TEngine.Core.Network
{
    public sealed class OuterPackInfo : APackInfo
    {
        public static OuterPackInfo Create(IMemoryOwner<byte> memoryOwner)
        {
            var outerPackInfo = Rent<OuterPackInfo>();;
            outerPackInfo.MemoryOwner = memoryOwner;
            return outerPackInfo;
        }

        public override MemoryStream CreateMemoryStream()
        {
            var recyclableMemoryStream = MemoryStreamHelper.GetRecyclableMemoryStream();
            recyclableMemoryStream.Write(MemoryOwner.Memory.Span.Slice(0, Packet.InnerPacketHeadLength + MessagePacketLength));
            recyclableMemoryStream.Seek(0, SeekOrigin.Begin);
            return recyclableMemoryStream;
        }

        public override void Dispose()
        {
            base.Dispose();
            Pool<OuterPackInfo>.Return(this);
        }

        public override object Deserialize(Type messageType)
        {
            var memoryOwnerMemory = MemoryOwner.Memory;
            var memory = memoryOwnerMemory.Slice(Packet.OuterPacketHeadLength, MessagePacketLength);
            return ProtoBufHelper.FromMemory(messageType, memory);
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

        public OuterPacketParser()
        {
            MemoryPool = MemoryPool<byte>.Shared;
        }
        
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
                    // 创建消息包
                    var memoryOwner = MemoryPool.Rent(Packet.OuterPacketMaxLength);
                    packInfo = OuterPackInfo.Create(memoryOwner);
                    packInfo.RpcId = _rpcId;
                    packInfo.ProtocolCode = _protocolCode;
                    packInfo.RouteTypeCode = _routeTypeCode;
                    packInfo.MessagePacketLength = _messagePacketLength;
                    // 写入消息体的信息到内存中
                    buffer.Read(memoryOwner.Memory.Slice(Packet.OuterPacketHeadLength), _messagePacketLength);
                    // 写入消息头的信息到内存中
                    _messageHead.AsMemory().CopyTo(memoryOwner.Memory.Slice(0, Packet.OuterPacketHeadLength));
                    return _messagePacketLength > 0;
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

        public override bool UnPack(IMemoryOwner<byte> memoryOwner, out APackInfo packInfo)
        {
            packInfo = null;
            var memory = memoryOwner.Memory;

            try
            {
                if (memory.Length < Packet.OuterPacketHeadLength)
                {
                    return false;
                }

                var memorySpan = memory.Span;
                _messagePacketLength = BitConverter.ToInt32(memorySpan);
#if TENGINE_NET
                if (_messagePacketLength > Packet.PacketBodyMaxLength)
                {
                    throw new ScanException($"The received information exceeds the maximum limit = {_messagePacketLength}");
                }
#endif
                packInfo = OuterPackInfo.Create(memoryOwner);
                packInfo.MessagePacketLength = _messagePacketLength;
                packInfo.ProtocolCode = BitConverter.ToUInt32(memorySpan.Slice(Packet.PacketLength));
                packInfo.RpcId = BitConverter.ToUInt32(memorySpan.Slice(Packet.OuterPacketRpcIdLocation));
                packInfo.RouteTypeCode = BitConverter.ToUInt16(memorySpan.Slice(Packet.OuterPacketRouteTypeOpCodeLocation));

                if (memory.Length < _messagePacketLength)
                {
                    return false;
                }

                return _messagePacketLength >= 0;
            }
            catch (Exception e)
            {
                packInfo?.Dispose();
                Log.Error(e);
                return false;
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