using System.Net;
using System.Text;
using HexaVoiceChatShared;
using HexaVoiceChatShared.Net;
using HexaVoiceChatShared.MessageProtocol;
using VoiceChatHost.Type;

namespace VoiceChatHost
{
	public class RelayServer
	{
		private VoiceChatServer server;
		private Dictionary<string, VoiceRoom> rooms = new Dictionary<string, VoiceRoom>();

		private void VoiceRoomJoinOrKeepAlive(DecodedVoiceChatMessage message, IPEndPoint from)
		{
			DecodedClientWrappedMessage clientMessage = ClientWrappedMessage.DecodeMessage(message.body);

			VoiceRoom room;
			string roomName = Encoding.ASCII.GetString(clientMessage.body);

			if (rooms.ContainsKey(roomName))
			{
				room = rooms[roomName];
				room.UpdateLastEvent();
				room.UpdateLastClientEvent(clientMessage.clientId);
			}
			else
			{
				room = new VoiceRoom(roomName, server);
				rooms.Add(roomName, room);
			}

			room.AddClient(clientMessage.clientId, from);
		}

		private void VoiceRoomLeave(DecodedVoiceChatMessage message, IPEndPoint from)
		{
			DecodedClientWrappedMessage clientMessage = ClientWrappedMessage.DecodeMessage(message.body);
			string roomName = Encoding.ASCII.GetString(clientMessage.body);

			if (rooms.ContainsKey(roomName))
			{
				rooms[roomName].RemoveClient(clientMessage.clientId);
			}
		}

		private void ForwardToOthersInRoom(DecodedVoiceChatMessage message, IPEndPoint from)
		{
			DecodedClientWrappedMessage clientMessage = ClientWrappedMessage.DecodeMessage(message.body);

			foreach (var room in rooms)
			{
				if (room.Value.clients.ContainsKey(clientMessage.clientId))
				{
					room.Value.SendToAllClientsExcept(
						clientMessage.clientId,
						message.raw
					);

					return;
				}
			}
		}

		private void OnHandshake(DecodedVoiceChatMessage message, IPEndPoint from)
		{
			Console.WriteLine($"got handshake, respond to {from}");
			server.SendEventMessage(HVCMessage.Handshake, from);
		}

		public RelayServer(string ip)
		{
			server = new VoiceChatServer(new IPEndPoint(
				IPAddress.Parse(ip),
				HexaVoiceChat.Ports.relay
			));

			server.OnMessage(HVCMessage.VoiceRoomJoin, VoiceRoomJoinOrKeepAlive);
			server.OnMessage(HVCMessage.VoiceRoomKeepAlive, VoiceRoomJoinOrKeepAlive);
			server.OnMessage(HVCMessage.VoiceRoomLeave, VoiceRoomLeave);
			server.OnMessage(HVCMessage.Opus, ForwardToOthersInRoom);
			server.OnMessage(HVCMessage.SpeakingStateUpdated, ForwardToOthersInRoom);
			server.OnMessage(HVCMessage.Handshake, OnHandshake);

			new Thread(new ThreadStart(RelayServerMainThread)).Start();
		}

		private void RelayServerMainThread()
		{
			while (true)
			{
				long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
				foreach (var room in rooms)
				{
					if (room.Value.clients.Count == 0 || (now - room.Value.lastEvent > 10))
					{
						Console.WriteLine($"room {room.Key} was destroyed");
						rooms.Remove(room.Key);
					}
					else
					{
						foreach (var lastEvent in room.Value.clientLastEvents)
						{
							if (now - lastEvent.Value > 3)
							{
								room.Value.RemoveClient(lastEvent.Key);
							}
						}
					}
				}
				Thread.Sleep(100);
			}
		}
	}
}
