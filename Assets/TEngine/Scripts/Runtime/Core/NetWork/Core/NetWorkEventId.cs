namespace TEngine.Runtime
{
    public static class NetWorkEventId
    {
        public static int NetworkConnectedEvent = StringId.StringToHash("NetWorkEventId.NetworkConnectedEvent");
        public static int NetworkClosedEvent = StringId.StringToHash("NetWorkEventId.NetworkClosedEvent");
        public static int NetworkMissHeartBeatEvent = StringId.StringToHash("NetWorkEventId.NetworkMissHeartBeatEvent");
        public static int NetworkErrorEvent = StringId.StringToHash("NetWorkEventId.NetworkErrorEvent");
        public static int NetworkCustomErrorEvent = StringId.StringToHash("NetWorkEventId.NetworkCustomErrorEvent");
    }
}