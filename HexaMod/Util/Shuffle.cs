using System;

namespace HexaMod.Util
{
	static class RandomExtensions
	{
		// https://stackoverflow.com/questions/108819/best-way-to-randomize-an-array-with-net
		public static void Shuffle<Type>(this Random random, Type[] array)
		{
			int length = array.Length;

			while (length > 1)
			{
				int key = random.Next(length--);
				Type temp = array[length];
				array[length] = array[key];
				array[key] = temp;
			}
		}
	}
}
