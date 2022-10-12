using System;
using System.IO;
using System.Collections.Generic;

namespace Sproto {
	public abstract class SprotoTypeBase {
		protected SprotoTypeFieldOP has_field;
		protected SprotoTypeSerialize serialize;
		protected SprotoTypeDeserialize deserialize;


		public SprotoTypeBase(int max_field_count) {
			this.has_field = new SprotoTypeFieldOP (max_field_count);
			this.serialize = new SprotoTypeSerialize (max_field_count);
			this.deserialize = new SprotoTypeDeserialize ();
		}

		public int init (byte[] buffer, int offset=0){
			this.clear ();
			this.deserialize.init (buffer, offset);
			this.decode ();

			return this.deserialize.size ();
		}

		public long init (SprotoTypeReader reader) {
			this.clear ();
			this.deserialize.init (reader);
			this.decode ();

			return this.deserialize.size ();
		}

		public SprotoTypeBase(int max_field_count, byte[] buffer) {
			this.has_field = new SprotoTypeFieldOP (max_field_count);
			this.serialize = new SprotoTypeSerialize (max_field_count);
			this.deserialize = new SprotoTypeDeserialize (buffer);
		}
			
		public abstract int encode (SprotoStream stream);

		public  byte[] encode () {
			SprotoStream stream = new SprotoStream ();
			this.encode (stream);
			int len = stream.Position;

			byte[] buffer = new byte[len];
			stream.Seek (0, SeekOrigin.Begin);
			stream.Read (buffer, 0, len);

			return buffer;
		}

		protected abstract void decode ();

		public void clear(){
			// clear has slot
			this.has_field.clear_field ();

			// clear deserialize
			this.deserialize.clear ();
		}
	}

}