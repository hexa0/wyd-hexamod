using System;

namespace HexaMod.Voice
{
	public class CircularShortBuffer
	{
		public readonly short[] buffer;
		public readonly int capacity;
		public int WriteHead
		{
			get => (int)(realWriteHead % capacity);
		}
		public int ReadHead
		{
			get => (int)(realReadHead % capacity);
		}
		public long realWriteHead;
		public long realReadHead;
		public long lastWriteSize = 1;
		public long lastReadSize = 1;

		static void CopyData(short[] source, int sourceIndex, short[] destination, int destinationIndex, int length)
		{
			Buffer.BlockCopy(source, sourceIndex * 2, destination, destinationIndex * 2, length * 2);
		}

		public CircularShortBuffer(int capacity)
		{
			if (capacity <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");
			}

			this.capacity = capacity;
			buffer = new short[capacity];
			realWriteHead = 0;
			realReadHead = 0;
		}

		public void Write(short[] data)
		{
			if (realWriteHead < 0)
			{
				realWriteHead = 0;
			}

			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			int dataLength = data.Length;
			lastWriteSize = dataLength;

			if (dataLength == 0)
			{
				return;
			}

			if (dataLength > capacity)
			{
				throw new ArgumentException("Data exceeds buffer capacity.", nameof(data));
			}

			int spaceToEnd = capacity - WriteHead;

			if (dataLength <= spaceToEnd)
			{
				CopyData(data, 0, buffer, WriteHead, dataLength);
			}
			else
			{
				CopyData(data, 0, buffer, WriteHead, spaceToEnd);

				int remainingLength = dataLength - spaceToEnd;
				CopyData(data, spaceToEnd, buffer, 0, remainingLength);
			}

			realWriteHead += dataLength;
		}

		public bool IsEnough(int count)
		{
			return (realReadHead + count) <= realWriteHead;
		}

		public short[] Read(int count)
		{
			lastReadSize = count;

			if (realReadHead < 0)
			{
				realReadHead = 0;
			}

			if (count <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");
			}

			if (count > capacity)
			{
				throw new ArgumentOutOfRangeException(nameof(count), "Count cannot exceed buffer capacity.");
			}

			short[] result = new short[count];
			int spaceToEnd = capacity - ReadHead;
			if (count <= spaceToEnd)
			{
				CopyData(buffer, ReadHead, result, 0, count);
			}
			else
			{
				CopyData(buffer, ReadHead, result, 0, spaceToEnd);
				int remainingLength = count - spaceToEnd;
				CopyData(buffer, 0, result, spaceToEnd, remainingLength);
			}

			realReadHead += count;

			return result;
		}
	}
}
