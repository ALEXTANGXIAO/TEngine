using TEngine.Core.Network;

namespace GameLogic
{
    public class NetworkUtils
    {
        /// <summary>
        /// 网络消息校验错误码。
        /// </summary>
        /// <param name="response">网络回复消息包。</param>
        /// <returns>是否存在错误。</returns>
        public static bool CheckError(IResponse response)
        {
            bool hasError = false;
            if (response == null)
            {
                // TODO 根据错误码Tips提示。
                // var networkError = "NetWork Response Error";
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