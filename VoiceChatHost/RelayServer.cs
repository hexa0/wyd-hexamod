using System.Net;
using System.Text;
using HexaVoiceChatShared;
using HexaVoiceChatShared.Net;
using HexaVoiceChatShared.MessageProtocol;
using VoiceChatHost.Type;
using static HexaVoiceChatShared.HexaVoiceChat.Protocol;

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
            string roomHash = Encoding.ASCII.GetString(clientMessage.body);

            if (rooms.ContainsKey(roomHash))
            {
                room = rooms[roomHash];
                room.UpdateLastEvent();
            }
            else
            {
                room = new VoiceRoom(roomHash, server);
                rooms.Add(roomHash, room);
            }

            room.AddClient(clientMessage.clientId, from);
        }

        private void VoiceRoomLeave(DecodedVoiceChatMessage message, IPEndPoint from)
        {
            DecodedClientWrappedMessage clientMessage = ClientWrappedMessage.DecodeMessage(message.body);
            string roomHash = Encoding.ASCII.GetString(clientMessage.body);

            if (rooms.ContainsKey(roomHash))
            {
                rooms[roomHash].RemoveClient(clientMessage.clientId);
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

        public RelayServer(string ip)
        {
            server = new VoiceChatServer(new IPEndPoint(
                IPAddress.Parse(ip),
                HexaVoiceChat.Ports.relay
            ));

            server.OnMessage(VoiceChatMessageType.VoiceRoomJoin, VoiceRoomJoinOrKeepAlive);
            server.OnMessage(VoiceChatMessageType.VoiceRoomKeepAlive, VoiceRoomJoinOrKeepAlive);
            server.OnMessage(VoiceChatMessageType.VoiceRoomLeave, VoiceRoomLeave);
            server.OnMessage(VoiceChatMessageType.Opus, ForwardToOthersInRoom);
            server.OnMessage(VoiceChatMessageType.SpeakingStateUpdated, ForwardToOthersInRoom);

            new Thread(new ThreadStart(RelayServerMainThread)).Start();
        }

        private void RelayServerMainThread()
        {
            while (true)
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                foreach (var room in rooms)
                {
                    if (room.Value.clients.Count == 0 || (now - room.Value.lastEvent > 3))
                    {
                        Console.WriteLine($"room {room.Key} was destroyed");
                        rooms.Remove(room.Key);
                    }
                }
                Thread.Sleep(100);
            }
        }
    }
}
