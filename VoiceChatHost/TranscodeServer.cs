using System.Net;
using HexaVoiceChatShared;
using HexaVoiceChatShared.Net;
using HexaVoiceChatShared.MessageProtocol;
using VoiceChatHost.Opus;
using static HexaVoiceChatShared.HexaVoiceChat.Protocol;
using System.Text;
using System.Diagnostics;
using RNNoise.NET;

namespace VoiceChatHost
{
	public class TranscodeServer
	{
		private static DateTime start = DateTime.Now;
		private static readonly int decodeBufferSize = 4096;
		private static readonly Dictionary<ulong, float[]> decodeBuffers = [];
		private static readonly Dictionary<ulong, DecodingSetup> decoders = [];
		private static readonly Denoiser rnNoiseDenoiser = new Denoiser();
		private readonly VoiceChatServer server;
		private RelayClient? relay;
		private IPEndPoint? gameEndPoint;
		private bool isSpeaking = false;
		private double lastSpeakingTime = (DateTime.Now - start).TotalSeconds;
		private ulong clientId = (ulong)Process.GetCurrentProcess().Id;
		private static long lastEvent = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		private static float shortMaxValueMul = 1f / short.MaxValue;
		private static float shortMinValueMul = 1f / short.MinValue;
		private static bool doDenoise = true;

		private void OnSwitchRelay(DecodedVoiceChatMessage message, IPEndPoint from)
		{
			gameEndPoint = from;

			string relayIp = Encoding.ASCII.GetString(message.body);
			IPEndPoint newRelayEndPoint = new IPEndPoint(IPAddress.Parse(relayIp), HexaVoiceChat.Ports.relay);

			Console.WriteLine($"relay set to {newRelayEndPoint}");

			if (relay != null)
			{
				try
				{
					relay.LeaveRoom();
				}
				catch
				{

				}
				try
				{
					relay.Close();
				}
				catch
				{

				}
			}

			relay = new RelayClient(relayIp);
			relay.clientId = clientId;
			relay.onOpusAction = OnOpus;
			relay.onSpeakingStateAction = OnSpeakingState;
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

			decoders.Clear();
			decodeBuffers.Clear();

			relay.LeaveRoom();
		}

		private void OnPCM(DecodedVoiceChatMessage message, IPEndPoint from)
		{
			gameEndPoint = from;

			if (relay != null)
			{
				int sampleCount = message.body.Length / 2;

				short[] pcm = new short[sampleCount];
				Buffer.BlockCopy(message.body, 0, pcm, 0, message.body.Length);

				float[] floatAudio = new float[sampleCount];

				for (int i = 0; i < sampleCount; i++)
				{
					floatAudio[i] = pcm[i] * shortMaxValueMul;
				}

				if (doDenoise)
				{
					rnNoiseDenoiser.Denoise(floatAudio);
				}

				float maxVolume = MathF.Max(floatAudio.Max(), -floatAudio.Min());
				bool shouldBeSpeaking = maxVolume > 0.01; // > 1%

				if (shouldBeSpeaking)
				{
					if (isSpeaking != shouldBeSpeaking)
					{
						isSpeaking = true;
						relay.SetSpeakingState(true);
						OnSpeakingState(clientId, true);
					}

					lastSpeakingTime = (DateTime.Now - start).TotalSeconds;
				}
				else
				{
					double now = (DateTime.Now - start).TotalSeconds;
					if (now >= lastSpeakingTime + 0.25d)
					{
						if (isSpeaking)
						{
							isSpeaking = false;
							relay.SetSpeakingState(false);
							OnSpeakingState(clientId, false);
						}
					}
				}

				if (isSpeaking)
				{
					int frameSize = EncodingSetup.encoder.Encode(floatAudio, pcm.Length, EncodingSetup.encodeBuffer, EncodingSetup.encodeBuffer.Length);
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

		private void OnKeepTranscodeAlive(DecodedVoiceChatMessage message, IPEndPoint from)
		{
			lastEvent = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		}

		private void OnHandshake(DecodedVoiceChatMessage message, IPEndPoint from)
		{
			Console.WriteLine($"got handshake, respond to {from}");
			server.SendMessage(VoiceChatMessage.BuildMessage(VoiceChatMessageType.Handshake, Encoding.ASCII.GetBytes("!")), from);
		}

		private void OnDenoise(DecodedVoiceChatMessage message, IPEndPoint from)
		{
			Console.WriteLine($"set rn noise: {message.body[0]}");
			doDenoise = message.body[0] == 1;
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
			server.OnMessage(VoiceChatMessageType.KeepTranscodeAlive, OnKeepTranscodeAlive);
			server.OnMessage(VoiceChatMessageType.Handshake, OnHandshake);
			server.OnMessage(VoiceChatMessageType.SetRNNoiseEnabled, OnDenoise);

			new Thread(new ThreadStart(KeepAliveThread)).Start();

			Console.WriteLine("transcode server started");
		}

		static void KeepAliveThread()
		{
			bool alive = true;

			while (alive)
			{
				Thread.Sleep(1000);

				long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

				if (now - lastEvent > 5)
				{
					Console.WriteLine("transcode server died of old age");
					alive = false;
				}
			}
		}
	}
}
