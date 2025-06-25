using System.Net;
using VoiceChatHost.Opus;
using System.Text;
using RNNoise.NET;
using NAudio.Wave;
using VoiceChatShared;
using VoiceChatShared.Net;
using VoiceChatShared.Enums;
using VoiceChatShared.Net.PeerConnection;

namespace VoiceChatHost
{
	public class TranscodeServer
	{
		private static DateTime start = DateTime.Now;
		private static readonly int decodeBufferSize = 4096;
		private static readonly Dictionary<ulong, float[]> decodeBuffers = [];
		private static readonly Dictionary<ulong, DecodingSetup> decoders = [];
		private static readonly Denoiser rnNoiseDenoiser = new Denoiser();
		private readonly PeerDuelProtocolConnection<HVCMessage> server;
		private RelayClient relay;
		private IPEndPoint gameEndPoint;
		private bool isSpeaking = false;
		private double lastSpeakingTime = (DateTime.Now - start).TotalSeconds;
		private ulong clientId = 0;
		private static float shortMaxValueMul = 1f / short.MaxValue;
		// private static float shortMinValueMul = 1f / short.MinValue;
		private static bool doDenoise = true;

		static int micSampleRate = 48000;
		static int micBufferMillis = 20;
		static int micChannels = 1;
		static int micBits = 16;

		internal static bool listening = false;
		internal static bool expectedToDrop = false;
		internal static WaveInEvent waveIn = new WaveInEvent
		{
			DeviceNumber = 0,
			WaveFormat = new WaveFormat(
				rate: micSampleRate,
				bits: micBits,
				channels: micChannels
			),
			BufferMilliseconds = micBufferMillis,
			NumberOfBuffers = 2
		};

		private void DisconnectFromRelay(NetMessage<HVCMessage> message, IPEndPoint from)
		{
			gameEndPoint = from;

			Console.WriteLine($"disconnect from relay");

			if (relay != null)
			{
				try
				{
					relay.Close();
				}
				catch
				{

				}
			}

			relay = null;
		}

		private void ConnectToRelay(NetMessage<HVCMessage> message, IPEndPoint from)
		{
			gameEndPoint = from;

			string relayIp = Encoding.ASCII.GetString(message.Body);
			IPEndPoint newRelayEndPoint = new IPEndPoint(IPAddress.Parse(relayIp), HexaVoiceChat.Ports.relay);

			Console.WriteLine($"relay changed");

			if (relay != null)
			{
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

		private void OnVoiceRoomJoin(NetMessage<HVCMessage> message, IPEndPoint from)
		{
			gameEndPoint = from;

			if (relay == null)
			{
				throw new Exception("OnVoiceRoomJoin() cannot be called when there is no RelayClient");
			}

			clientId = message.Client;

			relay.clientId = message.Client;
			relay.JoinRoom(Encoding.ASCII.GetString(message.Body));
		}

		private void OnVoiceRoomLeave(NetMessage<HVCMessage> message, IPEndPoint from)
		{
			gameEndPoint = from;

			if (relay == null)
			{
				throw new Exception("OnVoiceRoomLeave() cannot be called when there is no RelayClient");
			}

			decoders.Clear();
			decodeBuffers.Clear();
			isSpeaking = false;
		}

		private void OnOpus(ulong clientId, byte[] opusFrame, int samples, int sampleRate, int channels)
		{
			if (!decodeBuffers.ContainsKey(clientId))
			{
				decodeBuffers.Add(clientId, new float[decodeBufferSize]);
			}

			if (!decoders.ContainsKey(clientId))
			{
				decoders.Add(clientId, new DecodingSetup(sampleRate, channels));
			}

			float[] decodeBuffer = decodeBuffers[clientId];

			DecodingSetup decoder = decoders[clientId];

			decoder.sampleRate = sampleRate;
			decoder.channels = channels;
			decoder.CommitChanges();

			int decodedSampleCount = decoder.Decode(opusFrame, decodeBuffer, samples) * channels;

			Span<float> decoded = decodeBuffer.AsSpan(0, samples);

			short[] decodePCM = new short[decodedSampleCount];

			for (int i = 0; i < decodedSampleCount; i++)
			{
				decodePCM[i] = (short)(decoded[i] * short.MaxValue);
			}

			byte[] message = new byte[(decodePCM.Length * 2) + 8];

			Buffer.BlockCopy(BitConverter.GetBytes(sampleRate), 0, message, 0, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(channels), 0, message, 4, 4);
			Buffer.BlockCopy(decodePCM, 0, message, 8, decodePCM.Length * 2);

			server.udp.SendMessage(HVCMessage.PCMData, message, clientId, gameEndPoint);
		}

		private void OnSpeakingState(ulong clientId, bool speaking)
		{
			server.udp.SendMessage(HVCMessage.SpeakingStateUpdated, NetData.As(speaking), clientId, gameEndPoint);
		}

		private void OnSetListening(NetMessage<HVCMessage> message, IPEndPoint from)
		{
			bool shouldListen = message.Body[0] == 1;
			if (listening != shouldListen)
			{
				listening = shouldListen;

				if (shouldListen)
				{
					Console.WriteLine("Mic Activated");
					waveIn.StartRecording();
				}
				else
				{
					Console.WriteLine("Mic Deactivated");
					waveIn.StopRecording();
				}
			}
		}

		static float currentPeak = 0f;

		void OnMicData(object sender, WaveInEventArgs waveInEvent)
		{
			if (relay != null)
			{
				byte[] audioBuffer = new byte[waveInEvent.BytesRecorded];
				Buffer.BlockCopy(waveInEvent.Buffer, 0, audioBuffer, 0, waveInEvent.BytesRecorded);

				try
				{
					if (relay.room != null)
					{
						int sampleCount = audioBuffer.Length / 2;

						short[] pcm = new short[sampleCount];
						Buffer.BlockCopy(audioBuffer, 0, pcm, 0, audioBuffer.Length);

						float[] floatAudio = new float[sampleCount];

						for (int i = 0; i < sampleCount; i++)
						{
							floatAudio[i] = pcm[i] * shortMaxValueMul;
						}

						if (doDenoise)
						{
							rnNoiseDenoiser.Denoise(floatAudio);
						}

						currentPeak = Math.Max(floatAudio.Max(), -floatAudio.Min());
						bool shouldBeSpeaking = currentPeak > 0.01; // > 1%

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
							int frameSize = EncodingSetup.Encode(floatAudio, pcm.Length / micChannels, EncodingSetup.encodeBuffer.Length);
							Span<byte> encoded = EncodingSetup.encodeBuffer.AsSpan(0, frameSize);

							relay.SendOpus(encoded.ToArray(), pcm.Length);
						}
					}
				}
				catch (Exception e)
				{
					Console.Error.WriteLine(e);
				}
			}
			else
			{
				currentPeak = 0f;
			}
		}

		public TranscodeServer(string ip, int port = HexaVoiceChat.Ports.transcode)
		{
			EncodingSetup.channels = micChannels;
			EncodingSetup.sampleRate = micSampleRate;
			EncodingSetup.Init();

			server = new PeerDuelProtocolConnection<HVCMessage>(new IPEndPoint(
				IPAddress.Parse(ip),
				port
			));

			server.OnMessage(HVCMessage.ConnectToRelay, ConnectToRelay);
			server.OnMessage(HVCMessage.DisconnectFromRelay, DisconnectFromRelay);
			server.OnMessage(HVCMessage.VoiceRoomJoin, OnVoiceRoomJoin);
			server.OnMessage(HVCMessage.Handshake, (NetMessage<HVCMessage> message, IPEndPoint peer) =>
			{
				Console.WriteLine($"got handshake, respond to {peer.Address}:{peer.Port}");
				server.tcp.SendEventMessage(HVCMessage.Handshake, peer);
			});
			server.OnMessage(HVCMessage.SetRNNoiseEnabled, (NetMessage<HVCMessage> message, IPEndPoint from) =>
			{
				Console.WriteLine($"set rn noise: {message.Body[0]}");
				doDenoise = message.Body[0] == 1;
			});
			server.OnMessage(HVCMessage.SetListening, OnSetListening);
			server.OnMessage(HVCMessage.SetMicDeviceId, (NetMessage<HVCMessage> message, IPEndPoint from) =>
			{
				int deviceId = BitConverter.ToInt32(message.Body);
				Console.WriteLine($"set mic device: {deviceId}");
				waveIn.DeviceNumber = deviceId;
			});
			server.OnMessage(HVCMessage.SetMicBufferMillis, (NetMessage<HVCMessage> message, IPEndPoint from) =>
			{
				waveIn.BufferMilliseconds = message.Body[0];
			});
			server.OnMessage(HVCMessage.SetMicBufferCount, (NetMessage<HVCMessage> message, IPEndPoint from) =>
			{
				waveIn.NumberOfBuffers = message.Body[0];
			});
			server.OnMessage(HVCMessage.SetMicChannels, (NetMessage<HVCMessage> message, IPEndPoint from) =>
			{
				byte channels = message.Body[0];

				if (channels == micChannels)
				{
					return;
				}

				Console.WriteLine($"set mic channels: {message.Body[0]}");

				micChannels = channels;
				EncodingSetup.channels = channels;

				waveIn.WaveFormat = new WaveFormat(
					rate: micSampleRate,
					bits: micBits,
					channels: micChannels
				);

				EncodingSetup.channels = message.Body[0];
				EncodingSetup.CommitChannels();

				if (listening)
				{
					expectedToDrop = true;
					waveIn.StopRecording();
				}
			});
			server.OnMessage(HVCMessage.SetBitrate, (NetMessage<HVCMessage> message, IPEndPoint from) =>
			{
				Bitrate preset = (Bitrate)message.Body[0];
				if (Enum.IsDefined(typeof(Bitrate), message.Body[0]))
				{
					Console.WriteLine($"set bitrate: {preset}");
					EncodingSetup.bitrateKB = message.Body[0];
					EncodingSetup.CommitBitrate();
				}
				else
				{
					Console.WriteLine($"attempted to set bitrate to an invalid value of {message.Body[0]}");
				}
			});

			waveIn.DataAvailable += OnMicData;

			waveIn.RecordingStopped += delegate (object sender, StoppedEventArgs e)
			{
				if (listening)
				{
					listening = false;

					if (!expectedToDrop) // expected drops are for when the recording state needs to be reset if the wave format is changed, as such don't log a warning in those situations
					{
						expectedToDrop = false;
						Console.WriteLine("mic dropout, attempt to reconnect");
					}
					else
					{
						Console.WriteLine("Mic Reactivated");
					}

					try
					{
						waveIn.StartRecording();
					}
					catch { }

					listening = true;
				}
			};

			server.Listen();

			server.tcp.OnDisconnect(peer =>
			{
				alive = false;
			});

			new Thread(new ThreadStart(KeepAliveThread)).Start();

			Console.WriteLine("transcode server started");
		}

		static bool alive = true;

		static void KeepAliveThread()
		{
			while (alive)
			{
				Thread.Sleep(1000);
			}
		}
	}
}
