using System;
using System.Runtime.CompilerServices;

namespace AssemblyCSharp
{
	public class NetworkWriter
	{
		//数据
		byte[] buffer;

		public NetworkWriter ()
		{
			this.buffer = new byte[0];
		}

		public byte[] Buffer ()
		{
			return this.buffer;
		}

		public int Length ()
		{
			return this.buffer.Length;
		}

		public void WriteBytes (byte[] data)
		{
			var d = this.buffer;
			this.buffer = new byte[d.Length + data.Length];
			d.CopyTo (this.buffer, 0);
			data.CopyTo (this.buffer, d.Length);
		}

		public void WriteUInt32 (UInt32 data)
		{
			var b = new byte[4];
			for (int i = 0; i < 4; i++) {
				b [3 - i] += (byte)(data >> i * 8);
			}
			this.WriteBytes (b);
		}

		public void WriteFloat (float data)
		{
			var d = BitConverter.GetBytes (data);
			if (BitConverter.IsLittleEndian) {
				Array.Reverse (d);
			}
			this.WriteBytes (d);
		}
	}
}

