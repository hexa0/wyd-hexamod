using System.Net;
using VoiceChatShared.Enums;
using VoiceChatShared.Net;
using VoiceChatShared.Net.PeerConnection;

namespace VoiceChatHost.Type
{
	public class VoiceRoom
	{
		public string roomName;
		public List<ulong> clients = new List<ulong>();
		public PeerDuelProtocolConnection<HVCMessage> server;

		public VoiceRoom(string roomName, PeerDuelProtocolConnection<HVCMessage> server)
		{
			Console.Error.WriteLine($"room with hash {roomName} was created");

			this.roomName = roomName;
			this.server = server;
		}

		public IPEndPoint GetClient(ulong client, bool reliable = true) => server.GetPeerEndPoint(client, reliable);

		public void SendToClient(ulong client, NetMessage<HVCMessage> message, bool reliable = true)
		{
			IPEndPoint clientEndPoint = GetClient(client, reliable);
			if (clientEndPoint != null)
			{
				if (reliable)
				{
					server.tcp.SendMessage(message, clientEndPoint);
				}
				else
				{
					server.udp.SendMessage(message, clientEndPoint);
				}
			}
			else
			{
				Console.Error.WriteLine($"Failed to send message to client {client} in room {roomName}: server.GetPeerEndPoint failed to resolve an IPEndPoint.");
			}
		}

		public void SendToAllClients(NetMessage<HVCMessage> message, bool reliable = true)
		{
			foreach (var client in clients)
			{
				SendToClient(client, message, reliable);
			}
		}

		public void SendToAllClientsExcept(ulong excludedClientId, NetMessage<HVCMessage> message, bool reliable = true)
		{
			foreach (var client in clients)
			{
				if (client != excludedClientId)
				{
					SendToClient(client, message, reliable);
				}
			}
		}

		public void UpdatePeers()
		{
			byte[] peersMessage = new byte[clients.Count * 8];

			for (int i = 0; i < clients.Count; i++)
			{
				Buffer.BlockCopy(BitConverter.GetBytes(clients[i]), 0, peersMessage, i * 8, 8);
			}

			SendToAllClients(new NetMessage<HVCMessage>(
				HVCMessage.VoiceRoomPeersUpdated,
				peersMessage
			));
		}

		public void AddClient(ulong peerId)
		{
			if (!clients.Contains(peerId))
			{
				Console.WriteLine($"client {peerId} joined room with hash {roomName}");
				clients.Add(peerId);
				UpdatePeers();
			}
		}

		public void RemoveClient(ulong peerId)
		{
			Console.WriteLine($"client {peerId} left room with hash {roomName}");

			if (clients.Contains(peerId))
			{
				clients.Remove(peerId);
				UpdatePeers();
			}
			else
			{
				throw new Exception($"client {peerId} isn't apart of room {roomName} that they are trying to leave");
			}
		}
	}
}
