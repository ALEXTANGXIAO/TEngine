using System;
using System.Collections.Generic;

namespace Sproto
{
	public class ProtocolFunctionDictionary
	{
		public class MetaInfo {
			public Type ProtocolType;
			public KeyValuePair<Type, typeFunc> Request;
			public KeyValuePair<Type, typeFunc> Response;
		};

		public delegate SprotoTypeBase typeFunc (byte[] buffer, int offset);
		private Dictionary<int, MetaInfo> MetaDictionary;
		private Dictionary<Type, int> ProtocolDictionary;

		public ProtocolFunctionDictionary () {
			this.MetaDictionary = new Dictionary<int, MetaInfo> ();
			this.ProtocolDictionary = new Dictionary<Type, int> ();
		}

		private MetaInfo _getMeta(int tag) {
			MetaInfo data;
			if (!this.MetaDictionary.TryGetValue (tag, out data)) {
				data = new MetaInfo ();
				this.MetaDictionary.Add (tag, data);
			}
			return data;
		}

		public void SetProtocol<ProtocolType>(int tag) {
			MetaInfo data = this._getMeta(tag);
			data.ProtocolType = typeof(ProtocolType);
			this.ProtocolDictionary.Add (data.ProtocolType, tag);
		}


		public void SetRequest<T>(int tag) where T: SprotoTypeBase, new() {
			MetaInfo data = this._getMeta (tag);
			_set<T> (tag, out data.Request);
		}


		public void SetResponse<T>(int tag) where T: SprotoTypeBase, new() {
			MetaInfo data = this._getMeta (tag);
			_set<T> (tag, out data.Response);
		}


		private void _set<T>(int tag, out KeyValuePair<Type, typeFunc> field) where T : SprotoTypeBase, new() {
			typeFunc _func = delegate (byte[] buffer, int offset) {
				T obj = new T();
				obj.init(buffer, offset);
				return obj;
			};

			field = new KeyValuePair<Type, typeFunc> (typeof(T), _func);
		}


		private SprotoTypeBase _gen(KeyValuePair<Type, typeFunc> field, int tag, byte[] buffer, int offset=0) {
			if (field.Value != null) {
				SprotoTypeBase obj = field.Value (buffer, offset);
#if (!INCLUDE_IL2CPP)
				if (obj.GetType () != field.Key) {
					throw new Exception("sproto type: "+obj.GetType().ToString() + "not is expected. [" + field.Key.ToString() + "]");
				}
#endif
				return obj;
			}
			return null;
		}


		public SprotoTypeBase GenResponse(int tag, byte[] buffer, int offset=0) {
			MetaInfo data = this.MetaDictionary[tag];
			return _gen (data.Response, tag, buffer, offset);
		}

		public SprotoTypeBase GenRequest(int tag, byte[] buffer, int offset=0) {
			MetaInfo data = this.MetaDictionary[tag];
			return _gen (data.Request, tag, buffer, offset);
		}


		public MetaInfo this[int tag] {
			get {
				return this.MetaDictionary [tag];
			}
		}

		public int this[Type protocolType] {
			get {
				return this.ProtocolDictionary [protocolType];
			}
		}
	}
}
