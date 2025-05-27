using System;
using System.Collections.Generic;
using System.Text;

namespace HexaMod.Util
{
	public class SerializationHelper
	{
		public List<byte> data = new List<byte>();
		public int cursor;

		public byte Read()
		{
			cursor++;
			return data[cursor - 1];
		}

		public byte[] Read(int size)
		{
			cursor += size;
			return data.GetRange(cursor - size, size).ToArray();
		}

		public string ReadString()
		{
			return Encoding.Unicode.GetString(ReadSizedObject());
		}

		public uint ReadUint()
		{
			return BitConverter.ToUInt32(Read(4), 0);
		}

		public ushort ReadUshort()
		{
			return BitConverter.ToUInt16(Read(2), 0);
		}

		public ulong ReadUlong()
		{
			return BitConverter.ToUInt64(Read(8), 0);
		}

		public void WriteBooleanBlock(bool[] booleans)
		{
			if (booleans.Length > 8)
			{
				throw new Exception("Too many booleans in boolean block.");
			}

			byte block = 0;

			for (int i = 0; i < booleans.Length; i++)
			{
				if (booleans[i]) {
					block |= (byte)(1 << i);
				}
			}

			Write(block);
		}

		public bool[] ReadBooleanBlock()
		{
			byte block = Read();
			bool[] booleans = new bool[8];

			for (int i = 0; i < booleans.Length; i++)
			{
				booleans[i] = (block & (1 << i)) != 0;
			}

			return booleans;
		}

		public void Write(byte toWrite)
		{
			data.Add(toWrite);
			cursor++;
		}

		public void Write(byte[] toWrite)
		{
			data.AddRange(toWrite);
			cursor += toWrite.Length;
		}

		public void Write(ushort toWrite)
		{
			Write(BitConverter.GetBytes(toWrite));
		}
		public void Write(short toWrite)
		{
			Write(BitConverter.GetBytes(toWrite));
		}

		public void Write(int toWrite)
		{
			Write(BitConverter.GetBytes(toWrite));
		}

		public void Write(uint toWrite)
		{
			Write(BitConverter.GetBytes(toWrite));
		}

		public void Write(long toWrite)
		{
			Write(BitConverter.GetBytes(toWrite));
		}
		public void Write(ulong toWrite)
		{
			Write(BitConverter.GetBytes(toWrite));
		}

		public void Write(string toWrite)
		{
			WriteSizedObject(Encoding.Unicode.GetBytes(toWrite));
		}

		public byte[] ReadSizedObject()
		{
			int length = BitConverter.ToInt32(Read(4), 0);
			return Read(length);
		}

		public void WriteSizedObject(byte[] toWrite)
		{
			Write(BitConverter.GetBytes(toWrite.Length));
			Write(toWrite);
		}
	}
}
