using System;
using System.IO;
#pragma warning disable CS8625
#pragma warning disable CS8618

namespace TEngine.Core.Network
{
    public class LastMessageInfo : IDisposable
    {
        public object Message;
        public MemoryStream MemoryStream;

        public void Dispose()
        {
            Message = null;
            MemoryStream = null;
        }
    }
}