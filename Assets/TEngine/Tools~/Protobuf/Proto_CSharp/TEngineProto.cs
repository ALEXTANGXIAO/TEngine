using ProtoBuf;
using TEngine.Runtime;
using System.Collections.Generic;
namespace TEngineProto
{
	[global::ProtoBuf.ProtoContract()]
	public enum RequestCode
	{
		RequestNone = 0,

		Heart = 1,

		User = 2,

		Room = 3,

		Game = 4,

	}

	[global::ProtoBuf.ProtoContract()]
	public enum ActionCode
	{
		ActionNone = 0,

		HeartBeat = 1,

		Register = 1000,

		Login = 1001,

		CreateRoom = 1002,

		FindRoom = 1003,

		GetPlayers = 1004,

		JoinRoom = 1005,

		ExitRoom = 1006,

		Chat = 2000,

	}

	[global::ProtoBuf.ProtoContract()]
	public enum ReturnCode
	{
		ReturnNone = 0,

		Success = 1,

		Fail = 2,

		MsgTimeOut = 3,

	}

	[global::ProtoBuf.ProtoContract()]
	public partial class MainPack
	{
		[global::ProtoBuf.ProtoMember(1)]
		public RequestCode requestcode { get; set; }

		[global::ProtoBuf.ProtoMember(2)]
		public ActionCode actioncode { get; set; }

		[global::ProtoBuf.ProtoMember(3)]
		public ReturnCode returncode { get; set; }

		[global::ProtoBuf.ProtoMember(4)]
		public LoginPack loginPack { get; set; }

		[global::ProtoBuf.ProtoMember(5)]
		[global::System.ComponentModel.DefaultValue("")]
		public string extstr { get; set; }

		[global::ProtoBuf.ProtoMember(6)]
		public List<RoomPack> roompack = new List<RoomPack>();

		[global::ProtoBuf.ProtoMember(7)]
		public PlayerPack playerpack { get; set; }

		[global::ProtoBuf.ProtoMember(8)]
		public long HeatEchoTime { get; set; }

	}

	[global::ProtoBuf.ProtoContract()]
	public partial class LoginPack
	{
		[global::ProtoBuf.ProtoMember(1)]
		[global::System.ComponentModel.DefaultValue("")]
		public string username { get; set; }

		[global::ProtoBuf.ProtoMember(2)]
		[global::System.ComponentModel.DefaultValue("")]
		public string password { get; set; }

	}

	[global::ProtoBuf.ProtoContract()]
	public partial class RoomPack
	{
		[global::ProtoBuf.ProtoMember(1)]
		[global::System.ComponentModel.DefaultValue("")]
		public string roomname { get; set; }

		[global::ProtoBuf.ProtoMember(2)]
		public int maxnum { get; set; }

		[global::ProtoBuf.ProtoMember(3)]
		public int curnum { get; set; }

		[global::ProtoBuf.ProtoMember(6)]
		public int roomID { get; set; }

		[global::ProtoBuf.ProtoMember(12)]
		public List<PlayerPack> playerpack = new List<PlayerPack>();

	}

	[global::ProtoBuf.ProtoContract()]
	public partial class PlayerPack
	{
		[global::ProtoBuf.ProtoMember(1)]
		[global::System.ComponentModel.DefaultValue("")]
		public string playerName { get; set; }

		[global::ProtoBuf.ProtoMember(2)]
		[global::System.ComponentModel.DefaultValue("")]
		public string playerID { get; set; }

		[global::ProtoBuf.ProtoMember(3)]
		public int hp { get; set; }

		[global::ProtoBuf.ProtoMember(4)]
		public PosPack posPack { get; set; }

	}

	[global::ProtoBuf.ProtoContract()]
	public partial class PosPack
	{
		[global::ProtoBuf.ProtoMember(1)]
		public float PosX { get; set; }

		[global::ProtoBuf.ProtoMember(2)]
		public float PosY { get; set; }

		[global::ProtoBuf.ProtoMember(3)]
		public float PosZ { get; set; }

		[global::ProtoBuf.ProtoMember(4)]
		public float RotaX { get; set; }

		[global::ProtoBuf.ProtoMember(5)]
		public float RotaY { get; set; }

		[global::ProtoBuf.ProtoMember(6)]
		public float RotaZ { get; set; }

		[global::ProtoBuf.ProtoMember(8)]
		public int Animation { get; set; }

		[global::ProtoBuf.ProtoMember(9)]
		public int Direction { get; set; }

		[global::ProtoBuf.ProtoMember(10)]
		public float MoveX { get; set; }

		[global::ProtoBuf.ProtoMember(11)]
		public float MoveY { get; set; }

		[global::ProtoBuf.ProtoMember(12)]
		public float MoveZ { get; set; }

	}

}
