using System.Net.Sockets;

namespace TEngine
{
    internal sealed partial class NetworkManager
    {
        private sealed class ConnectState
        {
            private readonly Socket _socket;
            private readonly object _userData;

            public ConnectState(Socket socket, object userData)
            {
                _socket = socket;
                _userData = userData;
            }

            public Socket Socket => _socket;

            public object UserData => _userData;
        }
    }
}
