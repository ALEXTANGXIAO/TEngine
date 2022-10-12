using System;
using System.IO;
using System.Collections.Generic;

namespace Sproto
{
	public class SprotoTypeSerialize {
//		private byte[] header;

		private int header_idx;
		private int header_sz;
		private int header_cap = SprotoTypeSize.sizeof_header;


		private SprotoStream data;
		private int data_idx;

		private int lasttag = -1;
		private int index = 0;


		public SprotoTypeSerialize (int max_field_count) {
			this.header_sz = SprotoTypeSize.sizeof_header + max_field_count * SprotoTypeSize.sizeof_field;
		}

		private void set_header_fn(int fn) {
			this.data [this.header_idx-2] = (byte)(fn & 0xff);
			this.data [this.header_idx-1] = (byte)((fn >> 8) & 0xff);
		}

		private void write_header_record(int record) {
			this.data [this.header_idx + this.header_cap-2] = (byte)(record & 0xff);
			this.data [this.header_idx + this.header_cap-1] = (byte)((record >> 8) & 0xff);

			this.header_cap += 2;
			this.index++;
		}

		private void write_uint32_to_uint64_sign(bool is_negative) {
			byte v = (byte)((is_negative)?(0xff):(0));

			this.data.WriteByte (v);
			this.data.WriteByte (v);
			this.data.WriteByte (v);
			this.data.WriteByte (v);
		}

		private void write_tag(int tag, int value) {
			int stag = tag - this.lasttag - 1;
			if (stag > 0) {
				// skip tag
				stag = (stag - 1) * 2 + 1;
				if (stag > 0xffff)
					SprotoTypeSize.error ("tag is too big.");

				this.write_header_record (stag);
			}

			this.write_header_record (value);
			this.lasttag = tag;
		}

		private void write_uint32(UInt32 v) {
			this.data.WriteByte ((byte)(v & 0xff));
			this.data.WriteByte ((byte)((v >> 8) & 0xff));
			this.data.WriteByte ((byte)((v >> 16) & 0xff));
			this.data.WriteByte ((byte)((v >> 24) & 0xff));
		}

		private void write_uint64(UInt64 v) {
			this.data.WriteByte ((byte)(v & 0xff));
			this.data.WriteByte ((byte)((v >> 8) & 0xff));
			this.data.WriteByte ((byte)((v >> 16) & 0xff));
			this.data.WriteByte ((byte)((v >> 24) & 0xff));
			this.data.WriteByte ((byte)((v >> 32) & 0xff));
			this.data.WriteByte ((byte)((v >> 40) & 0xff));
			this.data.WriteByte ((byte)((v >> 48) & 0xff));
			this.data.WriteByte ((byte)((v >> 56) & 0xff));
		}

		private void fill_size(int sz) {
			if (sz < 0)
				SprotoTypeSize.error ("fill invaild size.");

			this.write_uint32 ((UInt32)sz);
		}

		private int encode_integer(UInt32 v) {
			this.fill_size (sizeof(UInt32));

			this.write_uint32 (v);
			return SprotoTypeSize.sizeof_length + sizeof(UInt32);
		}

		private int encode_uint64(UInt64 v) {
			this.fill_size (sizeof(UInt64));

			this.write_uint64 (v);
			return SprotoTypeSize.sizeof_length + sizeof(UInt64);
		}

		private int encode_bytes(byte[] s) {
			this.fill_size (s.Length);
			this.data.Write (s, 0, s.Length);

			return SprotoTypeSize.sizeof_length + s.Length;
		}

		private int encode_string(string str){
			byte[] s = System.Text.Encoding.UTF8.GetBytes (str);
			return encode_bytes (s);
		}

			
		private int encode_struct(SprotoTypeBase obj){
			int sz_pos = this.data.Position;

			this.data.Seek (SprotoTypeSize.sizeof_length, SeekOrigin.Current);
			int len = obj.encode (this.data);
			int cur_pos = this.data.Position;

			this.data.Seek (sz_pos, SeekOrigin.Begin);
			this.fill_size (len);
			this.data.Seek (cur_pos, SeekOrigin.Begin);

			return SprotoTypeSize.sizeof_length + len;
		}

		private void clear() {
			this.index = 0;
			this.header_idx = 2;
			this.lasttag = -1;
			this.data = null;
			this.header_cap = SprotoTypeSize.sizeof_header;
		}


		// API
		public void write_decimal(double d, double floor, int tag) {
			Int64 v = (Int64)(d * floor + 0.5);
			write_integer (v, tag);
		}

		public void write_decimal(List<double> d_list, double floor, int tag) {
			if (d_list == null || d_list.Count <= 0)
				return;
			
			List<Int64> integer_list = new List<Int64> ();
			foreach (double v in d_list) {
				integer_list.Add ((Int64)(v * floor + 0.5));
			}
			write_integer (integer_list, tag);

		}

		public void write_integer(Int64 integer, int tag) {
			Int64 vh = integer >> 31;
			int sz = (vh == 0 || vh == -1)?(sizeof(UInt32)):(sizeof(UInt64));
			int value = 0;

			if (sz == sizeof(UInt32)) {
				UInt32 v = (UInt32)integer;
				if (v < 0x7fff) {
					value = (int)((v + 1) * 2);
					sz = 2;
				} else {
					sz = this.encode_integer (v);
				}

			} else if (sz == sizeof(UInt64)) {
				UInt64 v = (UInt64)integer;
				sz = this.encode_uint64 (v);

			} else {
				SprotoTypeSize.error("invaild integer size.");
			}

			this.write_tag (tag, value);
		}

		public void write_integer(List<Int64> integer_list, int tag) {
			if (integer_list == null || integer_list.Count <= 0)
				return;

			int sz_pos = this.data.Position;
			this.data.Seek (sz_pos + SprotoTypeSize.sizeof_length, SeekOrigin.Begin);

			int begin_pos = this.data.Position;
			int intlen = sizeof(UInt32);
			this.data.Seek (begin_pos + 1, SeekOrigin.Begin);

			for (int index = 0; index < integer_list.Count; index++) {
				Int64 v = integer_list [index];
				Int64 vh = v >> 31;
				int sz = (vh == 0 || vh == -1)?(sizeof(UInt32)):(sizeof(UInt64));

				if (sz == sizeof(UInt32)) {
					this.write_uint32 ((UInt32)v);
					if (intlen == sizeof(UInt64)) {
						bool is_negative = ((v & 0x80000000) == 0) ? (false) : (true);
						this.write_uint32_to_uint64_sign (is_negative);
					}

				} else if (sz == sizeof(UInt64)) {
					if (intlen == sizeof (UInt32)) {
						this.data.Seek (begin_pos+1, SeekOrigin.Begin);
						for (int i = 0; i < index; i++) {
							UInt64 value = (UInt64)(integer_list[i]);
							this.write_uint64 (value);
						}
						intlen = sizeof(UInt64);
					}
					this.write_uint64 ((UInt64)v);

				} else {
					SprotoTypeSize.error ("invalid integer size(" + sz + ")");
				}
			}

			// fill integer size
			int cur_pos = this.data.Position;
			this.data.Seek (begin_pos, SeekOrigin.Begin);
			this.data.WriteByte ((byte)intlen);

			// fill array size
			int size = (int)(cur_pos - begin_pos);
			this.data.Seek (sz_pos, SeekOrigin.Begin);
			this.fill_size (size);

			this.data.Seek (cur_pos, SeekOrigin.Begin);
			this.write_tag (tag, 0);
		}


		public void write_boolean(bool b, int tag) {
			Int64 v = (b)?(1):(0);
			this.write_integer (v, tag);
		}

		public void write_boolean(List<bool> b_list, int tag) {
			if (b_list == null || b_list.Count <= 0)
				return;

			this.fill_size (b_list.Count);
			for (int i = 0; i < b_list.Count; i++) {
				byte v = (byte)((b_list [i])?(1):(0));
				this.data.WriteByte (v);
			}

			this.write_tag (tag, 0);
		}

		public void write_binary(byte[] bytes, int tag) {
			this.encode_bytes (bytes);
			this.write_tag (tag, 0);
		}

		public void write_binary(List<byte[]> bytes_list, int tag) {
			if (bytes_list == null || bytes_list.Count <= 0)
				return;

			// write size length
			int sz = 0;
			foreach (byte[] v in bytes_list) {
				sz += SprotoTypeSize.sizeof_length + v.Length;
			}
			this.fill_size (sz);

			// write bytes
			foreach (byte[] v in bytes_list) {
				this.encode_bytes (v);
			}

			this.write_tag (tag, 0);
		}


		public void write_string(string str, int tag) {
			this.encode_string (str);
			this.write_tag (tag, 0);
		}

		public void write_string(List<string> str_list, int tag) {
			if (str_list == null || str_list.Count <= 0)
				return;

			// write size length
			int sz = 0;
			foreach (string v in str_list) {
				sz += SprotoTypeSize.sizeof_length + System.Text.Encoding.UTF8.GetByteCount(v);
			}
			this.fill_size (sz);

			// write stirng
			foreach (string v in str_list) {
				this.encode_string (v);
			}

			this.write_tag (tag, 0);
		}


		public void write_obj(SprotoTypeBase obj, int tag) {
			this.encode_struct (obj);
			this.write_tag (tag, 0);
		}

		private delegate void write_func();
		private void write_set(write_func func, int tag) {
			int sz_pos = this.data.Position;
			this.data.Seek (SprotoTypeSize.sizeof_length, SeekOrigin.Current);

			func ();

			int cur_pos = this.data.Position;
			int sz = (int)(cur_pos - sz_pos - SprotoTypeSize.sizeof_length);
			this.data.Seek (sz_pos, SeekOrigin.Begin);
			this.fill_size (sz);

			this.data.Seek (cur_pos, SeekOrigin.Begin);

			this.write_tag (tag, 0);
		}


		public void write_obj<T>(List<T> obj_list, int tag) where T :SprotoTypeBase {
			if (obj_list == null || obj_list.Count <= 0)
				return;

			write_func func = delegate {
				foreach (SprotoTypeBase v in obj_list) {
					this.encode_struct (v);
				}				
			};

			write_set (func, tag);
		}

		public void write_obj<TK, TV>(Dictionary<TK, TV> map, int tag) where TV :SprotoTypeBase {
			if (map == null || map.Count <= 0)
				return;

			write_func func = delegate {
				foreach(var pair in map){
					this.encode_struct(pair.Value);
				}
			};

			write_set (func, tag);
		}

		public void open(SprotoStream stream) {
			// clear state
			this.clear ();

			this.data = stream;
			this.header_idx = stream.Position + this.header_cap;
			this.data_idx = this.data.Seek (this.header_sz, SeekOrigin.Current);
		}


		public int close() {
			this.set_header_fn (this.index);

			int up_count = this.header_sz - this.header_cap;
			this.data.MoveUp (this.data_idx, up_count);

			int count = this.data.Position - this.header_idx + SprotoTypeSize.sizeof_header;

			// clear state
			this.clear ();

			return count;
		}

	}
}

