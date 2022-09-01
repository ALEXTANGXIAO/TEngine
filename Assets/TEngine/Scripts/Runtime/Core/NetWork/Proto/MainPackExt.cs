using TEngine.Runtime;

namespace TEngineProto
{
    public partial class MainPack:IMemory
    {
        public void Clear()
        {
            requestcode = RequestCode.RequestNone;
            actioncode = ActionCode.ActionNone;
            returncode = ReturnCode.ReturnNone;
        }
    }
}
