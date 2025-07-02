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
		private readonly PeerDuelProtocolConnection<HVCMessage> server;
		private readonly Dictionary<string, VoiceRoom> rooms = [];

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

			server.AddTCPPeer(peer, assignedId);
			server.AddUDPPeer(new IPEndPoint(peer.Address, udpPort), assignedId);

			Console.WriteLine($"client {peer.Address.GetHashCode()}:(TCP {tcpPort} UDP {udpPort}) was allocated to peer id {assignedId}");

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

			if (rooms.TryGetValue(roomName, out VoiceRoom value))
			{
				roomToJoin = value;
			}
			else
			{
				roomToJoin = new(roomName, server)
				{
					server = server
				};
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
				NetMessage<HVCMessage> newMessage = new(
					message.Type,
					message.Body,
					peerId // retransmit the packet to the other clients with the correct peerId
				);

				room.SendToAllClientsExcept(peerId, newMessage, reliable);
			}
		}

		void OnReliableFowardData(NetMessage<HVCMessage> message, IPEndPoint peer) => OnFowardData(message, peer, true);
		void OnUnreliableFowardData(NetMessage<HVCMessage> message, IPEndPoint peer) => OnFowardData(message, peer, false);

		public RelayServer(string ip)
		{
			IPEndPoint endPoint = new(
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
