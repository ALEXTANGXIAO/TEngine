using System;

namespace TEngine.Core.Network
{
    public class ScanException : Exception
    {
        public ScanException() { }

        public ScanException(string msg) : base(msg) { }
    }
}