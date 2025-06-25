using System.Net;
using System.Text;
using VoiceChatHost.Type;
using VoiceChatShared;
using VoiceChatShared.Enums;
using VoiceChatShared.Net;
using VoiceChatShared.Net.PeerConnection;

namespace VoiceChatHost
{
	public class RelayServer
	{
		private PeerDuelProtocolConnection<HVCMessage> server;
		private Dictionary<string, VoiceRoom> rooms = new Dictionary<string, VoiceRoom>();

		void SetPeer(NetMessage<HVCMessage> message, IPEndPoint peer)
		{
			ushort tcpPort = BitConverter.ToUInt16(message.Body, 0);
			ushort udpPort = BitConverter.ToUInt16(message.Body, 2);

			// Find an available ID for the peer
			ulong assignedId = 0;
			while (server.tcpPeerIds.ContainsKey(assignedId) || server.udpPeerIds.ContainsKey(assignedId))
			{
				assignedId++;
			}

			server.AddTCPPeer(new IPEndPoint(peer.Address, tcpPort), assignedId);
			server.AddUDPPeer(new IPEndPoint(peer.Address, udpPort), assignedId);

			server.tcp.SendMessage(
				new NetMessage<HVCMessage>(
					HVCMessage.VoiceRoomPeerIdAllocated,
					BitConverter.GetBytes(assignedId)
				),
				peer
			);
		}

		void SetUDPPeer(NetMessage<HVCMessage> message, IPEndPoint peer)
		{
			ulong peerId = BitConverter.ToUInt64(message.Body, 0);

			server.AddUDPPeer(peer, peerId);
		}

		void VoiceRoomJoin(NetMessage<HVCMessage> message, IPEndPoint peer)
		{
			ulong peerId = server.GetPeerClientId(peer);

			foreach (var room in rooms)
			{
				if (room.Value.clients.Contains(peerId))
				{
					room.Value.RemoveClient(peerId);
				}
			}

			string roomName = Encoding.ASCII.GetString(message.Body);

			VoiceRoom roomToJoin;

			if (rooms.ContainsKey(roomName))
			{
				roomToJoin = rooms[roomName];
			}
			else
			{
				roomToJoin = new VoiceRoom(roomName, server);
				rooms.Add(roomName, roomToJoin);
			}

			roomToJoin.AddClient(peerId);
		}

		void PeerDisconnected(IPEndPoint peer)
		{
			ulong peerId = server.GetPeerClientId(peer);

			foreach (var room in rooms)
			{
				if (room.Value.clients.Contains(peerId))
				{
					room.Value.RemoveClient(peerId);
				}
			}

			server.RemovePeer(peerId);
		}

		void OnHandshake(NetMessage<HVCMessage> message, IPEndPoint from)
		{
			server.tcp.SendEventMessage(HVCMessage.Handshake, from);
		}

		private VoiceRoom GetVoiceRoomForPeer(IPEndPoint peer)
		{
			ulong peerId = server.GetPeerClientId(peer);

			foreach (var room in rooms)
			{
				if (room.Value.clients.Contains(peerId))
				{
					return room.Value;
				}
			}

			return null;
		}

		void OnFowardData(NetMessage<HVCMessage> message, IPEndPoint peer, bool reliable = true)
		{
			ulong peerId = server.GetPeerClientId(peer);
			VoiceRoom room = GetVoiceRoomForPeer(peer);

			if (room != null)
			{
				byte[] newBody = new byte[message.Body.Length + 8];
				Buffer.BlockCopy(BitConverter.GetBytes(peerId), 0, newBody, 0, 8);
				Buffer.BlockCopy(message.Body, 0, newBody, 8, message.Body.Length);

				NetMessage<HVCMessage> newMessage = new NetMessage<HVCMessage>(
					message.Type,
					newBody
				);

				room.SendToAllClientsExcept(peerId, newMessage, reliable);
			}
		}

		void OnReliableFowardData(NetMessage<HVCMessage> message, IPEndPoint peer) => OnFowardData(message, peer, true);
		void OnUnreliableFowardData(NetMessage<HVCMessage> message, IPEndPoint peer) => OnFowardData(message, peer, false);

		public RelayServer(string ip)
		{
			IPEndPoint endPoint = new IPEndPoint(
				IPAddress.Parse(ip),
				HexaVoiceChat.Ports.relay
			);

			server = new PeerDuelProtocolConnection<HVCMessage>(endPoint);
			server.Listen();

			server.OnMessage(HVCMessage.VoiceRoomAllocatePeerId, SetPeer);
			server.OnMessage(HVCMessage.VoiceRoomJoin, VoiceRoomJoin);
			server.OnDisconnect(PeerDisconnected);
			server.OnMessage(HVCMessage.Opus, OnUnreliableFowardData);
			server.OnMessage(HVCMessage.SpeakingStateUpdated, OnReliableFowardData);
			server.OnMessage(HVCMessage.Handshake, OnHandshake);

			new Thread(new ThreadStart(RoomCleanupThread)).Start();
		}

		void RoomCleanupThread()
		{
			while (true)
			{
				long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

				lock (rooms)
				{
					foreach (var room in rooms)
					{
						if (room.Value.clients.Count == 0)
						{
							Console.WriteLine($"room {room.Key} was destroyed");
							rooms.Remove(room.Key);
						}
					}
				}

				Thread.Sleep(1000);
			}
		}
	}
}
