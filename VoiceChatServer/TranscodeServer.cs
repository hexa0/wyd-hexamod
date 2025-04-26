using System.Net;
using HexaVoiceChatShared;
using HexaVoiceChatShared.Net;
using HexaVoiceChatShared.MessageProtocol;
using VoiceChatHost.Opus;
using static HexaVoiceChatShared.HexaVoiceChat.Protocol;

namespace VoiceChatHost
{
    public class TranscodeServer
    {
        static float[] decodeBuffer = new float[4096];
        private VoiceChatServer server;

        private void OnPCM(DecodedVoiceChatMessage message, IPEndPoint from)
        {
            short[] pcm = new short[message.body.Length / 2];
            Buffer.BlockCopy(message.body, 0, pcm, 0, message.body.Length);

            int frameSize = EncodingSetup.encoder.Encode(pcm, pcm.Length, EncodingSetup.encodeBuffer, EncodingSetup.encodeBuffer.Length);
            Span<byte> encoded = EncodingSetup.encodeBuffer.AsSpan(0, frameSize);

            int decodedSampleCount = DecodingSetup.decoder.Decode(encoded, decodeBuffer, pcm.Length);

            Span<float> decoded = decodeBuffer.AsSpan(0, decodedSampleCount);

            short[] decodePCM = new short[decodedSampleCount];

            for (int i = 0; i < decodedSampleCount; i++)
            {
                decodePCM[i] = (short)(decoded[i] * short.MaxValue);
            }

            byte[] decodeBytes = new byte[decodePCM.Length * 2];
            Buffer.BlockCopy(decodePCM, 0, decodeBytes, 0, decodePCM.Length * 2);

            server.SendMessage(VoiceChatMessage.BuildMessage(VoiceChatMessageType.PCMData, decodeBytes), from);
        }

        public TranscodeServer(string ip)
        {
            server = new VoiceChatServer(new IPEndPoint(
                IPAddress.Parse(ip),
                HexaVoiceChat.Ports.transcode
            ));

            server.OnMessage(VoiceChatMessageType.PCMData, OnPCM);

            EncodingSetup.Init();

            new Thread(new ThreadStart(KeepAliveThread)).Start();

            Console.WriteLine("transcode server started");
        }

        static void KeepAliveThread()
        {
            while (true)
            {
                Thread.Sleep(10000);
            }
        }
    }
}
