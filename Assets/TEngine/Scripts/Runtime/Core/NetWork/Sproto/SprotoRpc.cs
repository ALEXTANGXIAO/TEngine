using System;
using System.Collections.Generic;

namespace Sproto
{
	public class SprotoRpc
	{
		public enum RpcType{
			REQUEST = 1,
			RESPONSE = 2,
		};

		public struct RpcInfo {
			public RpcType type;
			public long? session;
			public int? tag;

			public SprotoTypeBase requestObj;
			public SprotoTypeBase responseObj;
			public ResponseFunction Response;
		};

		public class RpcRequest {
			private SprotoType.Package package = new SprotoType.Package();
			private SprotoStream stream = new SprotoStream();
			private SprotoPack spack = new SprotoPack();

			private ProtocolFunctionDictionary protocol;
			private SprotoRpc rpc;

			public RpcRequest(ProtocolFunctionDictionary protocol, SprotoRpc rpc) {
				this.protocol = protocol;
				this.rpc = rpc;
			}

			public byte[] Invoke<T>(SprotoTypeBase request=null, long? session=null) {
				int tag = protocol[typeof(T)];
				ProtocolFunctionDictionary.MetaInfo info = protocol[tag];
#if (!INCLUDE_IL2CPP)
				if(request != null && request.GetType() != info.Request.Key) {
					throw new Exception("request type: " + request.GetType().ToString() + "not is expected. [" + info.Request.Key.GetType().ToString() + "]");

				}
#endif
				package.clear();
				package.type = tag;

				if(session != null) {
					rpc.sessionDictionary.Add((long)session, info.Response.Value);
					package.session = (long)session;
				}

				stream.Seek (0, System.IO.SeekOrigin.Begin);
				int len = package.encode (stream);

				if (request != null) {
					len += request.encode (stream);
				}

				return spack.pack(stream.Buffer, len);
			}
		}

		public delegate byte[] ResponseFunction(SprotoTypeBase response);

		private SprotoStream stream = new SprotoStream();
		private SprotoPack spack = new SprotoPack();
		private Dictionary<long, ProtocolFunctionDictionary.typeFunc> sessionDictionary = new Dictionary<long,  ProtocolFunctionDictionary.typeFunc>();
		private ProtocolFunctionDictionary protocol;
		private SprotoType.Package package = new SprotoType.Package ();

		public SprotoRpc (ProtocolBase protocolObj=null) {
			this.protocol =  (protocolObj!=null)?(protocolObj.Protocol):(null);
		}


		public RpcRequest Attach(ProtocolBase protocolObj=null) {
			ProtocolFunctionDictionary protocol = (protocolObj!=null)?(protocolObj.Protocol):(null);
			RpcRequest request = new RpcRequest (protocol, this);
			return request;
		}


		public RpcInfo Dispatch(byte[] buffer, int offset=0) {
			buffer = this.spack.unpack (buffer, buffer.Length - offset);
			offset = this.package.init (buffer);
			RpcInfo info;

			// request
			if (this.package.HasType) {
				int tag = (int)this.package.type;
				info.session = null;
				info.tag = tag;
				info.responseObj = null;
				info.requestObj = (this.protocol!=null)?(this.protocol.GenRequest (tag, buffer, offset)):(null);
				info.type = RpcType.REQUEST;
				info.Response = null;
				if (this.package.HasSession) {
					long session = this.package.session;
					info.Response = delegate (SprotoTypeBase response) {
						ProtocolFunctionDictionary.MetaInfo pinfo = this.protocol [tag];
#if (!INCLUDE_IL2CPP)
						if (response.GetType () != pinfo.Response.Key) {
							throw new Exception ("response type: " + response.GetType ().ToString () + "is not expected.(" + pinfo.Response.Key.ToString () + ")");
						}
#endif
						this.stream.Seek (0, System.IO.SeekOrigin.Begin);
						this.package.clear();
						this.package.session = session;
						this.package.encode (this.stream);

						response.encode (this.stream);

						int len = stream.Position;
						byte[] data = new byte[len];
						stream.Seek (0, System.IO.SeekOrigin.Begin);

						stream.Read (data, 0, len);
						return this.spack.pack (data);
					};
				}

			} else { // response
				if (!this.package.HasSession) {
					throw new Exception ("session not found");
				}

				ProtocolFunctionDictionary.typeFunc response;
				if (!this.sessionDictionary.TryGetValue (this.package.session, out response)) {
					throw new Exception ("Unknown session: " + this.package.session);
				}

				info.tag = null;
				info.session = this.package.session;
				info.requestObj = null;
				info.Response = null;
				info.type = RpcType.RESPONSE;
				info.responseObj =  (response == null)?(null):(response (buffer, offset));
			}

			return info;
		}
	}
}
