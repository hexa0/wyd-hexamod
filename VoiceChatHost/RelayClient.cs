using HexaVoiceChatShared;
using System.Net;
using HexaVoiceChatShared.Net;
using HexaVoiceChatShared.MessageProtocol;
using System.Text;
using System.Diagnostics;
using VoiceChatHost.Opus;

namespace VoiceChatHost
{
	public class RelayClient
	{
		private readonly VoiceChatClient client;
		public ulong clientId = (ulong)Process.GetCurrentProcess().Id;
		public Action<ulong, byte[], int, int, int> onOpusAction;
		public Action<ulong, bool> onSpeakingStateAction;
		public string room = null;

		public void JoinRoom(string roomName)
		{
			client.SendClientWrappedMessage(
				clientId,
				HVCMessage.VoiceRoomJoin,
				Encoding.ASCII.GetBytes(roomName)
			);

			room = roomName;
		}

		public void KeepRoomAlive()
		{
			if (room != null)
			{
				client.SendClientWrappedMessage(
					clientId,
					HVCMessage.VoiceRoomKeepAlive,
					Encoding.ASCII.GetBytes(room)
				);
			}
			else
			{
				throw new Exception("cannot call KeepRoomAlive() when we aren't connected to a room.");
			}
		}

		public void SetSpeakingState(bool speaking)
		{
			client.SendClientWrappedMessage(
				clientId,
				HVCMessage.SpeakingStateUpdated,
				speaking ? [1] : [0]
			);
		}

		public void SendOpus(byte[] encoded, int samples)
		{
			byte[] sampleCount = BitConverter.GetBytes(samples);
			byte[] sampleRate = BitConverter.GetBytes(EncodingSetup.sampleRate);
			byte[] channels = BitConverter.GetBytes(EncodingSetup.channels);

			byte[] opusMessage = new byte[encoded.Length + sampleCount.Length + sampleRate.Length + channels.Length];
			
			Buffer.BlockCopy(sampleCount, 0, opusMessage, 0, 4);
			Buffer.BlockCopy(sampleRate, 0, opusMessage, 4, 4);
			Buffer.BlockCopy(channels, 0, opusMessage, 8, 4);
			Buffer.BlockCopy(encoded, 0, opusMessage, 12, encoded.Length);

			//Buffer.BlockCopy(sampleCount, 0, opusMessage, 0, sampleCount.Length);
			//Buffer.BlockCopy(encoded, 0, opusMessage, sampleCount.Length, encoded.Length);

			client.SendClientWrappedMessage(
				clientId,
				HVCMessage.Opus,
				opusMessage
			);
		}

		public void LeaveRoom()
		{
			if (room != null)
			{
				client.SendClientWrappedMessage(
					clientId,
					HVCMessage.VoiceRoomLeave,
					Encoding.ASCII.GetBytes(room)
				);

				room = null;
			}
			else
			{
				throw new Exception("cannot call LeaveRoom() when we aren't connected to a room.");
			}
		}

		public void Close()
		{
			client.Close();
		}

		public RelayClient(string ip)
		{
			client = new VoiceChatClient(new IPEndPoint(
				IPAddress.Parse(ip),
				HexaVoiceChat.Ports.relay
			));

			client.Connect();

			client.OnMessage(HVCMessage.SpeakingStateUpdated, delegate (DecodedVoiceChatMessage message, IPEndPoint from)
			{
				DecodedClientWrappedMessage clientMessage = ClientWrappedMessage.DecodeMessage(message.body);
				
				if (onSpeakingStateAction != null)
				{
					onSpeakingStateAction.Invoke(clientMessage.clientId, clientMessage.body[0] == 1);
				}
			});

			client.OnMessage(HVCMessage.Opus, delegate (DecodedVoiceChatMessage message, IPEndPoint from)
			{
				DecodedClientWrappedMessage clientMessage = ClientWrappedMessage.DecodeMessage(message.body);

				if (onOpusAction != null)
				{
					int samples = BitConverter.ToInt32(clientMessage.body, 0);
					int sampleRate = BitConverter.ToInt32(clientMessage.body, 4);
					int channels = BitConverter.ToInt32(clientMessage.body, 8);
					byte[] opusFrame = new byte[clientMessage.body.Length - 12];
					Buffer.BlockCopy(clientMessage.body, 12, opusFrame, 0, clientMessage.body.Length - 12);
					onOpusAction.Invoke(clientMessage.clientId, opusFrame, samples, sampleRate, channels);
				}
			});

			new Thread(new ThreadStart(RelayClientKeepAliveThread)).Start();
		}

		public void RelayClientKeepAliveThread()
		{
			while (true)
			{
				if (room != null)
				{
					KeepRoomAlive();
				}
				Thread.Sleep(1000);
			}
		}

		public void RelayClientMainTestThread()
		{
			Console.WriteLine($"using clientid {clientId}");

			JoinRoom("RelayClientTestRoom");

			int leaveCountdown = 16;
			bool wasSpeaking = false;

			while (leaveCountdown > 0)
			{
				leaveCountdown--;
				Thread.Sleep(500);
				wasSpeaking = !wasSpeaking;
				SetSpeakingState(wasSpeaking);
			}

			if (leaveCountdown == 0) {
				LeaveRoom();
			}
		}
	}
}
