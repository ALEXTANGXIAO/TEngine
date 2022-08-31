using TEngineProto;

namespace TEngine.Runtime
{
    /// <summary>
    /// 心跳Handler
    /// </summary>
    public class CSHeartBeatHandler
    {
        public static MainPack AllocHeartBeatPack()
        {
            var mainPack = MemoryPool.Acquire<MainPack>();
            mainPack.actioncode = ActionCode.HeartBeat;
            mainPack.requestcode = RequestCode.Heart;
            return mainPack;
        }
        
        public static void Handler(MainPack mainPack)
        {
            if (mainPack == null)
            {
                Log.Fatal("Receive CSHeartBeat Failed !!!");
                return;
            }
            Log.Info("Receive packet '{0}'.", mainPack.ToString());
        }
    }
}