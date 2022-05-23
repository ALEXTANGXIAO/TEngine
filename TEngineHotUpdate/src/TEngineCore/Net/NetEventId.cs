namespace TEngineCore.Net
{
    internal class NetEventId
    {
        public static int HeartBeat = StringId.StringToHash("NetEventId.HeartBeat");
        public static int ConnectTcp = StringId.StringToHash("NetEventId.ConnectTcp");
        public static int ConnectUdp = StringId.StringToHash("NetEventId.ConnectUdp");
    }
}
