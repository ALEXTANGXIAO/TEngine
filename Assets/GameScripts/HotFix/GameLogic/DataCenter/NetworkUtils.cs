using TEngine.Core.Network;

namespace GameLogic
{
    public class NetworkUtils
    {
        public static bool CheckError(IResponse response)
        {
            bool hasError = false;
            if (response == null)
            {
                var networkError = "NetWork Response Error";
                hasError = true;
            }
            else
            {
                hasError = response.ErrorCode != 0;
            }
            return hasError;
        }
    }
}