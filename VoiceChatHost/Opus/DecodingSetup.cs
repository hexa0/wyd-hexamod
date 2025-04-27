using Concentus;

namespace VoiceChatHost.Opus
{
    public class DecodingSetup
    {
        public IOpusDecoder decoder = OpusCodecFactory.CreateDecoder(EncodingSetup.sampleRate, EncodingSetup.channels);
    }
}
