using Concentus;

namespace VoiceChatHost.Opus
{
	public class DecodingSetup
	{
		public int sampleRate = EncodingSetup.sampleRate;
		public int channels = EncodingSetup.channels;

		IOpusDecoder decoder = null;

		void ResetDecoder()
		{
			if (decoder == null || (sampleRate != decoder.SampleRate) || (channels != decoder.NumChannels))
			{
				decoder = OpusCodecFactory.CreateDecoder(sampleRate, channels);
			}
		}

		public void CommitChanges() => ResetDecoder();
		public void CommitChannels() => ResetDecoder();
		public void CommitSampleRate() => ResetDecoder();

		public int Decode(ReadOnlySpan<byte> in_data, Span<float> out_pcm, int frame_size, bool decode_fec = false) => decoder.Decode(in_data, out_pcm, frame_size, decode_fec);

		public DecodingSetup(int sampleRate, int channels)
		{
			this.sampleRate = sampleRate;
			this.channels = channels;

			ResetDecoder();
		}
	}
}
