namespace TEngine.Logic
{
    public static partial class ErrorCode
    {
        public const int ERR_SignError = 10000;
        public const int ERR_Disconnect = 210000;
        public const int ERR_LoginError = 210005;
        
        //300000开始自定义错误
        public const int ERR_AccountAlreadyRegisted = 300001;
        public const int ERR_AccountOrPasswordError = 300002;
        public const int ERR_UserNotOnline = 300003;
        public const int ERR_ConnectGateKeyError = 300004;
        public const int ERR_CreateNewCharacter = 300007;
        public const int ERR_CannotCreateMoreCharacter = 300008;
        public const int ERR_CharacterAlreadyRegisted = 300009;
        public const int ERR_AccountIsForbid = 300010;
        public const int ERR_AccountIsInGame = 300011;
    }
}