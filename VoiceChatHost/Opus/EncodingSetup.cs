using Concentus;
using Concentus.Enums;


namespace VoiceChatHost.Opus
{
	public static class EncodingSetup
	{
		public static int sampleRate = 48000;
		public static int channels = 1;
		public static int bitrateKB = 24;

		static IOpusEncoder encoder = null;
		public static byte[] encodeBuffer = new byte[32768];

		public static void CommitBitrate()
		{
			encoder.Bitrate = bitrateKB * 1000;
		}

		static void ResetEncoder()
		{
			encoder = OpusCodecFactory.CreateEncoder(sampleRate, channels, OpusApplication.OPUS_APPLICATION_VOIP);
			encoder.Bitrate = bitrateKB * 1000;
			encoder.UseVBR = true;
			encoder.Complexity = 10;
			encoder.SignalType = OpusSignal.OPUS_SIGNAL_VOICE;
		}

		public static void CommitChannels() => ResetEncoder();
		public static void CommitSampleRate() => ResetEncoder();

		public static int Encode(ReadOnlySpan<float> in_pcm, int frame_size, int max_data_bytes) => encoder.Encode(in_pcm, frame_size, encodeBuffer, max_data_bytes);

		public static void Init()
		{
			ResetEncoder();
			Console.WriteLine($"Opus Encoder Version: {encoder.GetVersionString()}");
		}
	}
}
