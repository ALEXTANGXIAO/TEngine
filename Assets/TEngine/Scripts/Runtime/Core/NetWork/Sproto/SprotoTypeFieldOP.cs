using System;

namespace Sproto {

	public class SprotoTypeFieldOP {

		static readonly int slot_bits_size = sizeof(UInt32)*8;
		public UInt32[] has_bits;

		public SprotoTypeFieldOP (int max_field_count) {
			int slot_count = max_field_count / slot_bits_size;
			if(max_field_count % slot_bits_size >0) {
				slot_count++;
			}

			this.has_bits = new UInt32[slot_count];
		}

		private int _get_array_idx(int bit_idx){
			int size = has_bits.Length;
			int array_idx = bit_idx / slot_bits_size;

			return array_idx;
		}

		private int _get_slotbit_idx(int bit_idx){
			int size = has_bits.Length;
			int slotbit_idx = bit_idx % slot_bits_size;

			return slotbit_idx;
		}


		public bool has_field(int field_idx){
			int array_idx = this._get_array_idx(field_idx);
			int slotbit_idx = this._get_slotbit_idx (field_idx);

			UInt32 slot = this.has_bits [array_idx];
			UInt32 mask = (UInt32)(1) << (slotbit_idx);

			return Convert.ToBoolean (slot & mask);
		}

		public void set_field(int field_idx, bool is_has){
			int array_idx = this._get_array_idx(field_idx);
			int slotbit_idx = this._get_slotbit_idx (field_idx);

			UInt32 slot = this.has_bits [array_idx];
			if (is_has) {
				UInt32 mask = (UInt32)(1) << slotbit_idx;
				this.has_bits [array_idx] = slot | mask;
			} else {
				UInt32 mask = ~((UInt32)(1) << slotbit_idx);
				this.has_bits [array_idx] = slot & mask;
			}
		}

		public void clear_field(){
			for(int i=0; i< this.has_bits.Length; i++){
				this.has_bits [i] = 0;
			}
		}
	}

}

