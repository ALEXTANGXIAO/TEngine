namespace GameLogic
{
    /// <summary>
    /// 网络消息包。
    /// </summary>
    public partial class ProtoPacket : PacketBase
    {
        public override int Id => 1;

        public override void Clear()
        {
            Close();
        }
    }
}
