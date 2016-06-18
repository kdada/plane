using System;
using System.Net;

namespace AssemblyCSharp
{
	// 网络数据读取器(大端子节序)
	public class NetworkReader
	{
		//数据
		byte[] data;
		//位置
		int pos;
		//数据长度
		int len;

		public NetworkReader (byte[] data, int length)
		{
			this.data = data;
			this.pos = 0;
			this.len = length;
		}

		public void Seek (int length)
		{
			this.pos += length;
			if (this.pos < 0) {
				this.pos = 0;
			}
			if (this.pos > this.len) {
				this.pos = this.len;
			}
		}

		public int Length ()
		{
			return this.len - this.pos;
		}

		public byte[] ReadBytes (int count)
		{
			if (this.len - this.pos < count) {
				return null;
			}
			var buf = new byte[count];
			Array.Copy (this.data, this.pos, buf, 0, count);
			this.pos += count;
			return buf;
		}

		public Nullable<UInt32> ReadUInt32 ()
		{
			var b = this.ReadBytes (4);
			if (b != null) {
				UInt32 d = 0;
				for (int i = 0; i < 4; i++) {
					d += (UInt32)b [i] << (3 - i) * 8;
				}
				return d;
			}
			return null;
		}

		public  Nullable<float> ReadFloat ()
		{
			var b = this.ReadBytes (4);
			if (b != null) {
				if (BitConverter.IsLittleEndian) {
					Array.Reverse (b);
				}
				return BitConverter.ToSingle (b, 0);
			}

			return null;
		}

	}
}

