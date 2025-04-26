using System.Net;
using HexaVoiceChatShared;
using HexaVoiceChatShared.Net;
using HexaVoiceChatShared.MessageProtocol;
using VoiceChatHost.Opus;
using static HexaVoiceChatShared.HexaVoiceChat.Protocol;
using System.Text;
using System.Diagnostics;

namespace VoiceChatHost
{
    public class TranscodeServer
    {
        private static readonly int decodeBufferSize = 4096;
        private static readonly Dictionary<ulong, float[]> decodeBuffers = [];
        private readonly VoiceChatServer server;
        private RelayClient? relay;
        private IPEndPoint gameEndPoint;
        private bool isSpeaking = false;
        private ulong clientId = (ulong)Process.GetCurrentProcess().Id;

        private void OnSwitchRelay(DecodedVoiceChatMessage message, IPEndPoint from)
        {
            gameEndPoint = from;

            string relayIp = Encoding.ASCII.GetString(message.body);
            IPEndPoint newRelayEndPoint = new IPEndPoint(IPAddress.Parse(relayIp), HexaVoiceChat.Ports.relay);

            Console.WriteLine($"relay set to {newRelayEndPoint}");

            if (relay == null)
            {
                relay = new RelayClient(relayIp);
                relay.clientId = clientId;
                relay.onOpusAction = OnOpus;
            }
            else
            {
                relay.SwitchRelay(relayIp);
            }
        }

        private void OnVoiceRoomJoin(DecodedVoiceChatMessage message, IPEndPoint from)
        {
            gameEndPoint = from;

            DecodedClientWrappedMessage clientMessage = ClientWrappedMessage.DecodeMessage(message.body);

            clientId = clientMessage.clientId;

            if (relay == null)
            {
                throw new Exception("OnVoiceRoomJoin() cannot be called when there is no RelayClient");
            }

            relay.clientId = clientMessage.clientId;
            relay.JoinRoom(Encoding.ASCII.GetString(clientMessage.body));
        }

        private void OnPCM(DecodedVoiceChatMessage message, IPEndPoint from)
        {
            gameEndPoint = from;

            short[] pcm = new short[message.body.Length / 2];
            Buffer.BlockCopy(message.body, 0, pcm, 0, message.body.Length);

            float maxVolume = (float)pcm.Max() / short.MaxValue;
            bool shouldBeSpeaking = maxVolume > 0.01; // > 1%
            if (isSpeaking != shouldBeSpeaking)
            {
                isSpeaking = shouldBeSpeaking;
                if (relay != null)
                {
                    relay.SetSpeakingState(isSpeaking);
                }
            }

            int frameSize = EncodingSetup.encoder.Encode(pcm, pcm.Length, EncodingSetup.encodeBuffer, EncodingSetup.encodeBuffer.Length);
            Span<byte> encoded = EncodingSetup.encodeBuffer.AsSpan(0, frameSize);

            if (relay != null)
            {
                relay.SendOpus(encoded.ToArray(), pcm.Length);
            }
        }

        private void OnOpus(ulong clientId, byte[] opusFrame, int sampleCount)
        {
            if (!decodeBuffers.ContainsKey(clientId))
            {
                decodeBuffers.Add(clientId, new float[decodeBufferSize]);
            }

            float[] decodeBuffer = decodeBuffers[clientId];

            int decodedSampleCount = DecodingSetup.decoder.Decode(opusFrame, decodeBuffer, sampleCount);

            Span<float> decoded = decodeBuffer.AsSpan(0, decodedSampleCount);

            short[] decodePCM = new short[decodedSampleCount];

            for (int i = 0; i < decodedSampleCount; i++)
            {
                decodePCM[i] = (short)(decoded[i] * short.MaxValue);
            }

            byte[] decodeBytes = new byte[decodePCM.Length * 2];
            Buffer.BlockCopy(decodePCM, 0, decodeBytes, 0, decodePCM.Length * 2);

            server.SendMessage(ClientWrappedMessage.BuildMessage(clientId, VoiceChatMessageType.PCMData, decodeBytes), gameEndPoint);
        }

        public TranscodeServer(string ip)
        {
            EncodingSetup.Init();

            server = new VoiceChatServer(new IPEndPoint(
                IPAddress.Parse(ip),
                HexaVoiceChat.Ports.transcode
            ));

            server.OnMessage(VoiceChatMessageType.PCMData, OnPCM);
            server.OnMessage(VoiceChatMessageType.SwitchRelay, OnSwitchRelay);
            server.OnMessage(VoiceChatMessageType.VoiceRoomJoin, OnVoiceRoomJoin);

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
