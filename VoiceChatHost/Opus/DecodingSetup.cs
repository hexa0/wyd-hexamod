using Concentus;

namespace VoiceChatHost.Opus
{
    public static class DecodingSetup
    {
        public static IOpusDecoder decoder = OpusCodecFactory.CreateDecoder(EncodingSetup.sampleRate, EncodingSetup.channels);
    }
}
