using System;
using System.IO;

namespace TEngine
{
    internal sealed partial class NetworkManager
    {
        private sealed class ReceiveState : IDisposable
        {
            private const int DefaultBufferLength = 1024 * 64;
            private MemoryStream _stream;
            private IPacketHeader _packetHeader;
            private bool _disposed;

            public ReceiveState()
            {
                _stream = new MemoryStream(DefaultBufferLength);
                _packetHeader = null;
                _disposed = false;
            }

            public MemoryStream Stream
            {
                get
                {
                    return _stream;
                }
            }

            public IPacketHeader PacketHeader
            {
                get
                {
                    return _packetHeader;
                }
            }

            public void PrepareForPacketHeader(int packetHeaderLength)
            {
                Reset(packetHeaderLength, null);
            }

            public void PrepareForPacket(IPacketHeader packetHeader)
            {
                if (packetHeader == null)
                {
                    throw new GameFrameworkException("Packet header is invalid.");
                }

                Reset(packetHeader.PacketLength, packetHeader);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (_disposed)
                {
                    return;
                }

                if (disposing)
                {
                    if (_stream != null)
                    {
                        _stream.Dispose();
                        _stream = null;
                    }
                }

                _disposed = true;
            }

            private void Reset(int targetLength, IPacketHeader packetHeader)
            {
                if (targetLength < 0)
                {
                    throw new GameFrameworkException("Target length is invalid.");
                }

                _stream.Position = 0L;
                _stream.SetLength(targetLength);
                _packetHeader = packetHeader;
            }
        }
    }
}
