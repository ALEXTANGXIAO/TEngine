using System.Collections.Generic;
using TEngine;
using TEngine.Core.Network;
using TEngine.Logic;

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
                if (ErrCodeTextMap.TryGetValue(response.ErrorCode,out var ret))
                {
                    Log.Error(ret);
                }
            }

            return hasError;
        }

        //Remark 这里图方便注册错误码文本，正常应该走文本配置表。
        public static Dictionary<int, string> ErrCodeTextMap = new Dictionary<int, string>
        {
            {
                ErrorCode.ERR_AccountAlreadyRegisted, "账户已经被注册了"
            },
            {
                ErrorCode.ERR_AccountOrPasswordError, "账户或者密码错误"
            },
            {
                ErrorCode.ERR_UserNotOnline, "用户当前不在线"
            },
        };
    }
}