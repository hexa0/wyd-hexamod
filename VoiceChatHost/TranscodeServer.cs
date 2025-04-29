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
        private static DateTime start = DateTime.Now;
        private static readonly int decodeBufferSize = 4096;
        private static readonly Dictionary<ulong, float[]> decodeBuffers = [];
        private static readonly Dictionary<ulong, DecodingSetup> decoders = [];
        private readonly VoiceChatServer server;
        private RelayClient? relay;
        private IPEndPoint? gameEndPoint;
        private bool isSpeaking = false;
        private double lastSpeakingTime = (DateTime.Now - start).TotalSeconds;
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
                relay.onSpeakingStateAction = OnSpeakingState;
            }
            else
            {
                relay.SwitchRelay(relayIp);
            }
        }

        private void OnVoiceRoomJoin(DecodedVoiceChatMessage message, IPEndPoint from)
        {
            gameEndPoint = from;

            if (relay == null)
            {
                throw new Exception("OnVoiceRoomJoin() cannot be called when there is no RelayClient");
            }

            DecodedClientWrappedMessage clientMessage = ClientWrappedMessage.DecodeMessage(message.body);

            clientId = clientMessage.clientId;

            relay.clientId = clientMessage.clientId;
            relay.JoinRoom(Encoding.ASCII.GetString(clientMessage.body));
        }

        private void OnVoiceRoomLeave(DecodedVoiceChatMessage message, IPEndPoint from)
        {
            gameEndPoint = from;

            if (relay == null)
            {
                throw new Exception("OnVoiceRoomLeave() cannot be called when there is no RelayClient");
            }

            relay.LeaveRoom();
        }

        private void OnPCM(DecodedVoiceChatMessage message, IPEndPoint from)
        {
            gameEndPoint = from;

            if (relay != null)
            {
                short[] pcm = new short[message.body.Length / 2];
                Buffer.BlockCopy(message.body, 0, pcm, 0, message.body.Length);

                float maxVolume = MathF.Max((float)pcm.Max() / short.MaxValue, (float)pcm.Min() / short.MinValue);
                bool shouldBeSpeaking = maxVolume > 0.005; // > 0.5%

                if (shouldBeSpeaking)
                {
                    if (isSpeaking != shouldBeSpeaking)
                    {
                        isSpeaking = true;
                        relay.SetSpeakingState(true);
                    }

                    lastSpeakingTime = (DateTime.Now - start).TotalSeconds;
                }
                else
                {
                    double now = (DateTime.Now - start).TotalSeconds;
                    if (now > lastSpeakingTime + 0.1d)
                    {
                        isSpeaking = false;
                        relay.SetSpeakingState(false);
                    }
                }

                if (isSpeaking)
                {
                    lastSpeakingTime = (DateTime.Now - start).TotalSeconds;
                    int frameSize = EncodingSetup.encoder.Encode(pcm, pcm.Length, EncodingSetup.encodeBuffer, EncodingSetup.encodeBuffer.Length);
                    Span<byte> encoded = EncodingSetup.encodeBuffer.AsSpan(0, frameSize);

                    relay.SendOpus(encoded.ToArray(), pcm.Length);
                }
            }
        }

        private void OnOpus(ulong clientId, byte[] opusFrame, int frameSize)
        {
            if (!decodeBuffers.ContainsKey(clientId))
            {
                decodeBuffers.Add(clientId, new float[decodeBufferSize]);
            }

            if (!decoders.ContainsKey(clientId))
            {
                decoders.Add(clientId, new DecodingSetup());
            }

            float[] decodeBuffer = decodeBuffers[clientId];

            int decodedSampleCount = decoders[clientId].decoder.Decode(opusFrame, decodeBuffer, frameSize);

            Span<float> decoded = decodeBuffer.AsSpan(0, frameSize);

            short[] decodePCM = new short[decodedSampleCount];

            for (int i = 0; i < decodedSampleCount; i++)
            {
                decodePCM[i] = (short)(decoded[i] * short.MaxValue);
            }

            byte[] decodeBytes = new byte[decodePCM.Length * 2];
            Buffer.BlockCopy(decodePCM, 0, decodeBytes, 0, decodePCM.Length * 2);

            server.SendMessage(ClientWrappedMessage.BuildMessage(clientId, VoiceChatMessageType.PCMData, decodeBytes), gameEndPoint);
        }

        private void OnSpeakingState(ulong clientId, bool speaking)
        {
            server.SendMessage(ClientWrappedMessage.BuildMessage(clientId, VoiceChatMessageType.SpeakingStateUpdated, [speaking ? (byte)1 : (byte)0]), gameEndPoint);
        }

        public TranscodeServer(string ip, int port = HexaVoiceChat.Ports.transcode)
        {
            EncodingSetup.Init();

            server = new VoiceChatServer(new IPEndPoint(
                IPAddress.Parse(ip),
                port
            ));

            server.OnMessage(VoiceChatMessageType.PCMData, OnPCM);
            server.OnMessage(VoiceChatMessageType.SwitchRelay, OnSwitchRelay);
            server.OnMessage(VoiceChatMessageType.VoiceRoomJoin, OnVoiceRoomJoin);
            server.OnMessage(VoiceChatMessageType.VoiceRoomLeave, OnVoiceRoomLeave);

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
