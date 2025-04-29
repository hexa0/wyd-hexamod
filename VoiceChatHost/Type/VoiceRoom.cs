using System.Net;
using HexaVoiceChatShared.Net;
using HexaVoiceChatShared.MessageProtocol;

namespace VoiceChatHost.Type
{
    public class VoiceRoom
    {
        public string roomName;
        public Dictionary<ulong, IPEndPoint> clients = new Dictionary<ulong, IPEndPoint>();
        public Dictionary<ulong, long> clientLastEvents = new Dictionary<ulong, long>();
        public long lastEvent = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public VoiceChatServer server;

        public VoiceRoom(string roomName, VoiceChatServer server)
        {
            Console.WriteLine($"room with hash {roomName} was created");

            this.roomName = roomName;
            this.server = server;
        }

        public void UpdateLastEvent()
        {
            lastEvent = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public void UpdateLastClientEvent(ulong clientId)
        {
            if (!clientLastEvents.ContainsKey(clientId))
            {
                clientLastEvents.Add(clientId, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            }
            else
            {
                clientLastEvents[clientId] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
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

        public void SendToAllClientsExcept(ulong excludedClientId, byte[] message)
        {
            UpdateLastClientEvent(excludedClientId);

            foreach (var client in clients)
            {
                if (client.Key != excludedClientId)
                {
                    SendToClient(client.Key, message);
                }
            }
        }

        public void AddClient(ulong clientId, IPEndPoint clientEndPoint)
        {
            if (!clients.ContainsKey(clientId))
            {
                Console.WriteLine($"client {clientId} joined room with hash {roomName}");
                clients.Add(clientId, clientEndPoint);
                clientLastEvents.Add(clientId, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            }
        }

        public void RemoveClient(ulong clientId)
        {
            Console.WriteLine($"client {clientId} left room with hash {roomName}");

            if (clients.ContainsKey(clientId))
            {
                clients.Remove(clientId);
                clientLastEvents.Remove(clientId);
            }
            else
            {
                throw new Exception($"client {clientId} isn't apart of room {roomName} that they are trying to leave");
            }
        }
    }
}
