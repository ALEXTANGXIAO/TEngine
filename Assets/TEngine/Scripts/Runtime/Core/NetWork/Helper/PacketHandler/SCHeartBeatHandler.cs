namespace TEngine.Runtime
{
    /// <summary>
    /// 心跳Handler
    /// </summary>
    public class CSHeartBeatHandler : PacketHandlerBase
    {
        public override int Id
        {
            get
            {
                return 2;
            }
        }

        public override void Handle(object sender, Packet packet)
        {
            CSHeartBeat packetImpl = (CSHeartBeat)packet;
            if (packetImpl == null)
            {
                Log.Fatal("Receive CSHeartBeat Failed !!!");
                return;
            }
            Log.Info("Receive packet '{0}'.", packetImpl.Id.ToString());
        }
    }
}