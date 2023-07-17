namespace TEngine
{
	public static partial class OuterOpcode
	{
		 public const int H_C2G_Message = 100000001;
		 public const int H_C2G_MessageRequest = 110000001;
		 public const int H_G2C_MessageResponse = 160000001;
		 public const int H_C2G_PushMessageToClient = 100000002;
		 public const int H_G2C_ReceiveMessageToServer = 100000003;
		 public const int H_C2G_LoginAddressRequest = 110000002;
		 public const int H_G2C_LoginAddressResponse = 160000002;
		 public const int H_C2M_Message = 190000001;
		 public const int H_C2M_MessageRequest = 200000001;
		 public const int H_M2C_MessageResponse = 250000001;
		 public const int H_C2M_PushAddressMessageToClient = 190000002;
		 public const int H_M2C_ReceiveAddressMessageToServer = 190000003;
		 public const int H_C2G_LoginRequest = 110000003;
		 public const int H_G2C_LoginResponse = 160000003;
		 public const int H_C2G_RegisterRequest = 110000004;
		 public const int H_G2C_RegisterResponse = 160000004;
		 public const int CmdGmReq = 110000005;
		 public const int CmdGmRes = 160000005;
	}
}
