using System.Net;
using System.Text;
using VoiceChatHost.Opus;
using VoiceChatShared;
using VoiceChatShared.Enums;
using VoiceChatShared.Net.PeerConnection;

namespace VoiceChatHost
{
	public class RelayClient
	{
		private readonly PeerDuelProtocolConnection<HVCMessage> client;
		RelayConnectionState m_state = RelayConnectionState.Disconnected;
		public RelayConnectionState State
		{
			get => m_state;
			set {
				if (m_state != value)
				{
					Console.WriteLine($"RelayClient: {value}");
					m_state = value;
				}
			}
		}
		public ulong clientId = 0;
		public Action<ulong, byte[], int, int, int> onOpusAction;
		public Action<ulong, bool> onSpeakingStateAction;
		public Action<ulong> onAllocatedIdAction;
		public string room = null;

		public void JoinRoom(string roomName)
		{
			room = roomName;

			if (State == RelayConnectionState.Connected)
			{
				client.tcp.SendMessage(
					HVCMessage.VoiceRoomJoin,
					Encoding.ASCII.GetBytes(roomName)
				);
			}
		}

		public void SetSpeakingState(bool speaking)
		{
			if (client.tcp.Disposed) { return; }
			client.tcp.SendMessage(
				HVCMessage.SpeakingStateUpdated,
				speaking ? [1] : [0]
			);
		}

		public void SendOpus(byte[] encoded, int samples)
		{
			if (client.udp.Disposed) { return; }
			byte[] sampleCount = BitConverter.GetBytes(samples);
			byte[] sampleRate = BitConverter.GetBytes(EncodingSetup.sampleRate);
			byte[] channels = BitConverter.GetBytes(EncodingSetup.channels);

			byte[] opusMessage = new byte[encoded.Length + sampleCount.Length + sampleRate.Length + channels.Length];
			
			Buffer.BlockCopy(sampleCount, 0, opusMessage, 0, 4);
			Buffer.BlockCopy(sampleRate, 0, opusMessage, 4, 4);
			Buffer.BlockCopy(channels, 0, opusMessage, 8, 4);
			Buffer.BlockCopy(encoded, 0, opusMessage, 12, encoded.Length);

			client.udp.SendMessage(
				HVCMessage.Opus,
				opusMessage
			);
		}

		public void Close()
		{
			State = RelayConnectionState.Closed;
			client.Close();
		}

		public void TryToConnect()
		{
			State = RelayConnectionState.Connecting;
			client.Connect();
		}

		public RelayClient(string ip)
		{
			client = new PeerDuelProtocolConnection<HVCMessage>(new IPEndPoint(
				IPAddress.Parse(ip),
				HexaVoiceChat.Ports.relay
			));

			client.OnDisconnect(peer =>
			{
				if (State != RelayConnectionState.Closed)
				{
					State = RelayConnectionState.Failed;
				}
			});

			client.OnMessage(HVCMessage.VoiceRoomPeersUpdated, (message, peer) =>
			{

			});

			client.OnMessage(HVCMessage.Opus, (message, peer) =>
			{
				int samples = BitConverter.ToInt32(message.Body, 0);
				int sampleRate = BitConverter.ToInt32(message.Body, 4);
				int channels = BitConverter.ToInt32(message.Body, 8);
				byte[] opusFrame = new byte[message.Body.Length - 12];
				Buffer.BlockCopy(message.Body, 12, opusFrame, 0, message.Body.Length - 12);
				onOpusAction.Invoke(
					message.Client,
					opusFrame,
					samples,
					sampleRate,
					channels
				);
			});

			client.OnMessage(HVCMessage.SpeakingStateUpdated, (message, peer) =>
			{
				onSpeakingStateAction.Invoke(
					message.Client,
					message.Body[0] == 1
				);
			});

			TryToConnect();

			new Thread(new ThreadStart(ConnectThread)).Start();
		}

		void ConnectThread()
		{
			const int maxBackoffStart = 100; // start with 0.1 seconds
			const int maxBackoff = 10000; // wait up to 10 seconds
			int backoff = maxBackoffStart;
			int attempts = 1;

			while (State != RelayConnectionState.Closed)
			{
				switch (State)
				{
					case RelayConnectionState.Failed:
						Console.WriteLine($"RelayClient: Attempting to reconnect (attempt {attempts})");
						TryToConnect();

						backoff = Math.Min(backoff * 2, maxBackoff);
						attempts++;
						break;
					case RelayConnectionState.Connecting:
						if (client.tcp.Connected)
						{
							State = RelayConnectionState.AllocatingId;

							try
							{
								client.tcp.Once(HVCMessage.VoiceRoomPeerIdAllocated, (message, peer) => {
									if (message.Body.Length == 8)
									{
										clientId = BitConverter.ToUInt64(message.Body, 0);
										Console.WriteLine($"RelayClient: Allocated to peer ID {clientId} by server.");

										onAllocatedIdAction.Invoke(clientId);
										State = RelayConnectionState.Connected;

										if (room != null)
										{
											JoinRoom(room);
										}
									}
									else
									{
										State = RelayConnectionState.Failed;
										throw new ArgumentException("RelayClient: Invalid VoiceRoomPeerIdAllocated message received.");
									}
								});

								byte[] portData = new byte[4];

								Buffer.BlockCopy(BitConverter.GetBytes((ushort)client.tcp.ClientEndPoint.Port), 0, portData, 0, 2);
								Buffer.BlockCopy(BitConverter.GetBytes((ushort)client.udp.ClientEndPoint.Port), 0, portData, 2, 2);

								client.tcp.SendMessage(HVCMessage.VoiceRoomAllocatePeerId, portData);
							}
							catch (Exception ex)
							{
								State = RelayConnectionState.Failed;
								Console.WriteLine($"RelayClient: Failed to allocate peer ID {ex.Message}.");
							}
						}
						break;
					case RelayConnectionState.Connected:
						backoff = maxBackoffStart;
						attempts = 1;
						break;
				}

				Thread.Sleep(backoff);
			}
		}
	}
}
