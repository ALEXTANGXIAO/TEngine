using TEngine.Runtime;

namespace TEngineProto
{
    public partial class MainPack:Packet
    {
        public override void Clear()
        {
            requestcode = RequestCode.RequestNone;
            actioncode = ActionCode.ActionNone;
            returncode = ReturnCode.ReturnNone;
        }
    }
}
