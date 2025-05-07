namespace HexaVoiceChatShared.Net
{
	internal class FragmentQueue
	{
		public byte[] data = new byte[1024 * 4096];
		public int dataOffest = 0;
	}
}
