using Concentus;
using Concentus.Enums;


namespace VoiceChatHost.Opus
{
    public static class EncodingSetup
    {
        public static int sampleRate = 48000;
        public static int channels = 1;
        private static int bitrateKB = 64;

        public static IOpusEncoder encoder = OpusCodecFactory.CreateEncoder(sampleRate, channels, OpusApplication.OPUS_APPLICATION_VOIP);
        public static byte[] encodeBuffer = new byte[16384];

        public static void Init()
        {
            encoder.Bitrate = bitrateKB * 1000;
            encoder.UseVBR = true;
            encoder.Complexity = 10;
            encoder.SignalType = OpusSignal.OPUS_SIGNAL_VOICE;
            Console.WriteLine($"Opus Encoder Version: {encoder.GetVersionString()}");
        }
    }
}
