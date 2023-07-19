#if TENGINE_NET
using TEngine.DataStructure;
using TEngine.Core;
#pragma warning disable CS8600
#pragma warning disable CS8625
#pragma warning disable CS8603

namespace TEngine.Core.Network;

public sealed class InnerPackInfo : APackInfo
{
    public static InnerPackInfo Create()
    {
        return Pool<InnerPackInfo>.Rent();
    }
    
    public static InnerPackInfo Create(uint rpcId, long routeId, uint protocolCode)
    {
        var innerPackInfo = Pool<InnerPackInfo>.Rent();
        innerPackInfo.RpcId = rpcId;
        innerPackInfo.RouteId = routeId;
        innerPackInfo.ProtocolCode = protocolCode;
        return innerPackInfo;
    }

    public static InnerPackInfo Create(uint rpcId, long routeId, uint protocolCode, long routeTypeCode, MemoryStream memoryStream)
    {
        var innerPackInfo = Pool<InnerPackInfo>.Rent();
        innerPackInfo.RpcId = rpcId;
        innerPackInfo.RouteId = routeId;
        innerPackInfo.ProtocolCode = protocolCode;
        innerPackInfo.RouteTypeCode = routeTypeCode;
        innerPackInfo.MemoryStream = memoryStream;
        return innerPackInfo;
    }

    public override void Dispose()
    {
        base.Dispose();
        Pool<InnerPackInfo>.Return(this);
    }

    public override object Deserialize(Type messageType)
    {
        using (MemoryStream)
        {
            MemoryStream.Seek(Packet.InnerPacketHeadLength, SeekOrigin.Begin);

            switch (ProtocolCode)
            {
                case >= Opcode.InnerBsonRouteResponse:
                {
                    return MongoHelper.Instance.DeserializeFrom(messageType, MemoryStream);
                }
                case >= Opcode.InnerRouteResponse:
                {
                    return ProtoBufHelper.FromStream(messageType, MemoryStream);
                }
                case >= Opcode.OuterRouteResponse:
                {
                    return ProtoBufHelper.FromStream(messageType, MemoryStream);
                }
                case >= Opcode.InnerBsonRouteMessage:
                {
                    return MongoHelper.Instance.DeserializeFrom(messageType, MemoryStream);
                }
                case >= Opcode.InnerRouteMessage:
                case >= Opcode.OuterRouteMessage:
                {
                    return ProtoBufHelper.FromStream(messageType, MemoryStream);
                }
                default:
                {
                    Log.Error($"protocolCode:{ProtocolCode} Does not support processing protocol");
                    return null;
                }
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
                _rpcId = BitConverter.ToUInt32(_messageHead, Packet.OuterPacketRpcIdLocation);
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
                packInfo = InnerPackInfo.Create(_rpcId, _routeId, _protocolCode);

                if (_messagePacketLength > 0)
                {
                    return true;
                }
            
                var memoryStream = MemoryStreamHelper.GetRecyclableMemoryStream();
                // 写入消息体的信息到内存中
                memoryStream.Seek(Packet.InnerPacketHeadLength, SeekOrigin.Begin);
                buffer.Read(memoryStream, _messagePacketLength);
                // 写入消息头的信息到内存中
                memoryStream.Seek(0, SeekOrigin.Begin);
                memoryStream.Write(BitConverter.GetBytes(_messagePacketLength));
                memoryStream.Write(BitConverter.GetBytes(packInfo.ProtocolCode));
                memoryStream.Write(BitConverter.GetBytes(packInfo.RpcId));
                memoryStream.Write(BitConverter.GetBytes(packInfo.RouteId));
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
        InnerPackInfo packInfo = null;
        
        try
        {
            if (memoryStream == null || memoryStream.Length < Packet.InnerPacketHeadLength)
            {
                return null;
            }
                
            _ = memoryStream.Read(_messageHead, 0, Packet.InnerPacketHeadLength);
            _messagePacketLength = BitConverter.ToInt32(_messageHead, 0);

            if (_messagePacketLength > Packet.PacketBodyMaxLength)
            {
                throw new ScanException($"The received information exceeds the maximum limit = {_messagePacketLength}");
            }

            packInfo = InnerPackInfo.Create();
            packInfo.ProtocolCode = BitConverter.ToUInt32(_messageHead, Packet.PacketLength);
            packInfo.RpcId = BitConverter.ToUInt32(_messageHead, Packet.OuterPacketRpcIdLocation);
            packInfo.RouteId = BitConverter.ToInt64(_messageHead, Packet.InnerPacketRouteRouteIdLocation);
                
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
    
    public static void Serialize(object message, MemoryStream stream)
    {
        if (message is IBsonMessage)
        {
            MongoHelper.Instance.SerializeTo(message, stream);
            return;
        }

        ProtoBufHelper.ToStream(message, stream);
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
            Serialize(message, memoryStream);
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
        Array.Clear(_messageHead, 0, _messageHead.Length);
        base.Dispose();
    }
}
#endif