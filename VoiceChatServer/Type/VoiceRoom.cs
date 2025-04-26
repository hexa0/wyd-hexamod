using System.Net;
using HexaVoiceChatShared.Net;
using HexaVoiceChatShared.MessageProtocol;

namespace VoiceChatHost.Type
{
    public class VoiceRoom
    {
        public string roomHash;
        public Dictionary<ulong, IPEndPoint> clients = new Dictionary<ulong, IPEndPoint>();
        public long lastEvent = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public VoiceChatServer server;

        public VoiceRoom(string roomHash, VoiceChatServer server)
        {
            Console.WriteLine($"room with hash {roomHash} was created");

            this.roomHash = roomHash;
            this.server = server;
        }

        public void UpdateLastEvent()
        {
            lastEvent = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public void SendToClient(ulong client, byte[] message)
        {
            UpdateLastEvent();
            server.SendMessage(message, clients[client]);
        }

        public void SendToAllClients(byte[] message)
        {
            foreach (var client in clients)
            {
                SendToClient(client.Key, message);
            }
        }

        public void SendToAllClientsExcept(ulong excludedClient, byte[] message)
        {
            foreach (var client in clients)
            {
                if (client.Key != excludedClient)
                {
                    SendToClient(client.Key, message);
                }
            }
        }

        public void AddClient(ulong clientId, IPEndPoint clientEndPoint)
        {
            if (!clients.ContainsKey(clientId))
            {
                Console.WriteLine($"client {clientId} joined room with hash {roomHash}");
                clients.Add(clientId, clientEndPoint);
            }
        }

        public void RemoveClient(ulong clientId)
        {
            Console.WriteLine($"client {clientId} left room with hash {roomHash}");

            if (clients.ContainsKey(clientId))
            {
                clients.Remove(clientId);
            }
            else
            {
                throw new Exception($"client {clientId} isn't apart of room {roomHash} that they are trying to leave");
            }
        }
    }
}
