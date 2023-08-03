#if TENGINE_NET
using System.Buffers;
using TEngine.DataStructure;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

namespace TEngine.Core.Network;

public sealed class InnerPackInfo : APackInfo
{
    public static InnerPackInfo Create(IMemoryOwner<byte> memoryOwner)
    {
        var innerPackInfo = Rent<InnerPackInfo>();
        innerPackInfo.MemoryOwner = memoryOwner;
        return innerPackInfo;
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
        Pool<InnerPackInfo>.Return(this);
    }

    public override object Deserialize(Type messageType)
    {
        var memoryOwnerMemory = MemoryOwner.Memory;
        memoryOwnerMemory = memoryOwnerMemory.Slice(Packet.InnerPacketHeadLength, MessagePacketLength);
        
        switch (ProtocolCode)
        {
            case >= Opcode.InnerBsonRouteResponse:
            {
                return MongoHelper.Instance.Deserialize(memoryOwnerMemory, messageType);
            }
            case >= Opcode.InnerRouteResponse:
            {
                return ProtoBufHelper.FromMemory(messageType, memoryOwnerMemory);
            }
            case >= Opcode.OuterRouteResponse:
            {
                return ProtoBufHelper.FromMemory(messageType, memoryOwnerMemory);
            }
            case >= Opcode.InnerBsonRouteMessage:
            {
                return MongoHelper.Instance.Deserialize(memoryOwnerMemory, messageType);
            }
            case >= Opcode.InnerRouteMessage:
            case >= Opcode.OuterRouteMessage:
            {
                return ProtoBufHelper.FromMemory(messageType, memoryOwnerMemory);
            }
            default:
            {
                Log.Error($"protocolCode:{ProtocolCode} Does not support processing protocol");
                return null;
            }
        }
    }
}

public sealed class InnerPacketParser : APacketParser
{
    private uint _rpcId;
    private long _routeId;
    private uint _protocolCode;
    private int _messagePacketLength;
    private bool _isUnPackHead = true;
    private readonly byte[] _messageHead = new byte[Packet.InnerPacketHeadLength];
    
    public InnerPacketParser()
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
                if (buffer.Length < Packet.InnerPacketHeadLength)
                {
                    return false;
                }
                
                _ = buffer.Read(_messageHead, 0, Packet.InnerPacketHeadLength);
                _messagePacketLength = BitConverter.ToInt32(_messageHead, 0);
    
                if (_messagePacketLength > Packet.PacketBodyMaxLength)
                {
                    throw new ScanException($"The received information exceeds the maximum limit = {_messagePacketLength}");
                }
                
                _protocolCode = BitConverter.ToUInt32(_messageHead, Packet.PacketLength);
                _rpcId = BitConverter.ToUInt32(_messageHead, Packet.InnerPacketRpcIdLocation);
                _routeId = BitConverter.ToInt64(_messageHead, Packet.InnerPacketRouteRouteIdLocation);
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
                var memoryOwner = MemoryPool.Rent(Packet.InnerPacketMaxLength);
                packInfo = InnerPackInfo.Create(memoryOwner);
                packInfo.RpcId = _rpcId;
                packInfo.RouteId = _routeId;
                packInfo.ProtocolCode = _protocolCode;
                packInfo.MessagePacketLength = _messagePacketLength;
                // 写入消息体的信息到内存中
                buffer.Read(memoryOwner.Memory.Slice(Packet.InnerPacketHeadLength), _messagePacketLength);
                // 写入消息头的信息到内存中
                _messageHead.AsMemory().CopyTo( memoryOwner.Memory.Slice(0, Packet.InnerPacketHeadLength));
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
    
    public override bool UnPack(IMemoryOwner<byte> memoryOwner, out APackInfo packInfo)
    {
        packInfo = null;

        try
        {
            var memorySpan = memoryOwner.Memory.Span;

             if (memorySpan.Length < Packet.InnerPacketHeadLength)
            {
                return false;
            }
            
            _messagePacketLength = BitConverter.ToInt32(memorySpan);
            
            if (_messagePacketLength > Packet.PacketBodyMaxLength)
            {
                throw new ScanException($"The received information exceeds the maximum limit = {_messagePacketLength}");
            }
            
            packInfo = InnerPackInfo.Create(memoryOwner);
            packInfo.MessagePacketLength = _messagePacketLength;
            packInfo.ProtocolCode = BitConverter.ToUInt32(memorySpan[Packet.PacketLength..]);
            packInfo.RpcId = BitConverter.ToUInt32(memorySpan[Packet.OuterPacketRpcIdLocation..]);
            packInfo.RouteId = BitConverter.ToInt64(memorySpan[Packet.InnerPacketRouteRouteIdLocation..]);
            
            if (memorySpan.Length < _messagePacketLength)
            {
                return false;
            }

            return _messagePacketLength >= 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public static MemoryStream Pack(uint rpcId, long routeId, MemoryStream memoryStream)
    {
        memoryStream.Seek(Packet.InnerPacketRpcIdLocation, SeekOrigin.Begin);
        memoryStream.Write(BitConverter.GetBytes(rpcId));
        memoryStream.Write(BitConverter.GetBytes(routeId));
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    public static MemoryStream Pack(uint rpcId, long routeId, object message)
    {
        var opCode = Opcode.PingRequest;
        var packetBodyCount = 0;
        var memoryStream = MemoryStreamHelper.GetRecyclableMemoryStream();
        
        memoryStream.Seek(Packet.InnerPacketHeadLength, SeekOrigin.Begin);

        if (message != null)
        {
            if (message is IBsonMessage)
            {
                try
                {
                    
                    MongoHelper.Instance.SerializeTo(message, memoryStream);
                }
                catch (Exception e)
                {
                    Log.Fatal(e);
                    throw;
                }
            }
            else
            {
                ProtoBufHelper.ToStream(message, memoryStream);
            }

            opCode = MessageDispatcherSystem.Instance.GetOpCode(message.GetType());
            packetBodyCount = (int)(memoryStream.Position - Packet.InnerPacketHeadLength);
        }

        if (packetBodyCount > Packet.PacketBodyMaxLength)
        {
            throw new Exception($"Message content exceeds {Packet.PacketBodyMaxLength} bytes");
        }

        memoryStream.Seek(0, SeekOrigin.Begin);
        memoryStream.Write(BitConverter.GetBytes(packetBodyCount));
        memoryStream.Write(BitConverter.GetBytes(opCode));
        memoryStream.Write(BitConverter.GetBytes(rpcId));
        memoryStream.Write(BitConverter.GetBytes(routeId));
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    public override void Dispose()
    {
        _messagePacketLength = 0;
        base.Dispose();
    }
}
#endif