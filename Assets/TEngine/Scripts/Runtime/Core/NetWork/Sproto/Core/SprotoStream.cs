using System;
using System.IO;

namespace Sproto
{
	public class SprotoStream
	{
		private int size;
		private int pos;
		private byte[] buffer;

		public int Position {
			get {return this.pos;}
		}

		public byte[] Buffer {
			get {return this.buffer;}
		}

		public SprotoStream () {
			this.size = 128;
			this.pos = 0;
			this.buffer = new byte[this.size];
		}

		private void _expand(int sz=0) {
			if(this.size - this.pos  < sz) {
				long bak_sz = this.size;
				while (this.size - this.pos < sz) {
					this.size = this.size * 2;
				}

				if (this.size >= SprotoTypeSize.encode_max_size) {
					SprotoTypeSize.error ("object is too large (>" + SprotoTypeSize.encode_max_size + ")");
				}

				byte[] new_buffer = new byte[this.size];
				for (long i = 0; i < bak_sz; i++) {
					new_buffer [i] = this.buffer [i];
				}
				this.buffer = new_buffer;
			}
		}


		public void WriteByte(byte v) {
			this._expand(sizeof(byte));
			this.buffer [this.pos++] = v;
		}


		public void Write(byte[] data, int offset, int count) {
			this._expand(count);
			for (int i = 0; i < count; i++) {
				this.buffer [this.pos++] = data [offset + i];
			}
		}

		public int Seek(int offset, SeekOrigin loc) {
			switch (loc) {
			case SeekOrigin.Begin:
				this.pos = offset;
				break;
			case SeekOrigin.Current:
				this.pos += offset;
				break;
			case SeekOrigin.End:
				this.pos = this.size + offset;
				break;
			}

			this._expand ();
			return this.pos;
		}

		public void Read(byte[] buffer, int offset, int count) {
			for (int i = 0; i < count; i++) {
				buffer[offset+i] = this.buffer[this.pos++];
			}
		}


		public void MoveUp(int position, int up_count) {
			if (up_count <= 0)
				return;

			long count = this.pos - position;
			for (int i = 0; i < count; i++) {
				this.buffer [position - up_count + i] = this.buffer [position + i];
			}
			this.pos -= up_count;
		}

		public byte this[int i] {
			get {
				if (i < 0 || i >= this.size) {
					throw new Exception ("invalid idx:" + i + "@get");
				}
				return this.buffer [i];
			}

			set {
				if (i < 0 || i >= this.size) {
					throw new Exception ("invalid idx:" + i + "@set");
				}
				this.buffer [i] = value;
			}
		}
	}
}
