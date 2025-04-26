using HexaVoiceChatShared;
using System.Net;
using HexaVoiceChatShared.Net;
using HexaVoiceChatShared.MessageProtocol;
using static HexaVoiceChatShared.HexaVoiceChat.Protocol;
using System.Text;
using System.Diagnostics;

namespace VoiceChatHost
{
    public class RelayClient
    {
        private readonly VoiceChatClient client;
        public ulong clientId = (ulong)Process.GetCurrentProcess().Id;
        string room = "test";

        public void MakeRoom(string roomName)
        {
            client.SendMessage(ClientWrappedMessage.BuildMessage(
                clientId,
                VoiceChatMessageType.VoiceRoomJoin,
                Encoding.ASCII.GetBytes(roomName)
            ));
        }

        public void KeepRoomAlive(string roomName)
        {
            client.SendMessage(ClientWrappedMessage.BuildMessage(
                clientId,
                VoiceChatMessageType.VoiceRoomKeepAlive,
                Encoding.ASCII.GetBytes(roomName)
            ));
        }

        public void SetSpeakingState(bool speaking)
        {
            client.SendMessage(ClientWrappedMessage.BuildMessage(
                clientId,
                VoiceChatMessageType.SpeakingStateUpdated,
                speaking ? [1] : [0]
            ));
        }

        public void LeaveRoom(string roomName)
        {
            client.SendMessage(ClientWrappedMessage.BuildMessage(
                clientId,
                VoiceChatMessageType.VoiceRoomLeave,
                Encoding.ASCII.GetBytes(roomName)
            ));
        }

        int leaveCountdown = 8;
        bool wasSpeaking = false;

        public RelayClient(string ip)
        {
            Console.WriteLine($"using clientid {clientId}");

            client = new VoiceChatClient(new IPEndPoint(
                IPAddress.Parse(ip),
                HexaVoiceChat.Ports.relay
            ));

            client.OnMessage(VoiceChatMessageType.SpeakingStateUpdated, delegate (DecodedVoiceChatMessage message, IPEndPoint from)
            {
                DecodedClientWrappedMessage clientMessage = ClientWrappedMessage.DecodeMessage(message.body);
                Console.WriteLine($"client {clientMessage.clientId}'s voice chat state updated to {clientMessage.body[0]}");
            });

            MakeRoom(room);

            new Thread(new ThreadStart(RelayClientMainThread)).Start();
        }

        private void RelayClientMainThread()
        {
            while (leaveCountdown > 0)
            {
                leaveCountdown--;
                Thread.Sleep(1000);
                KeepRoomAlive(room);
                wasSpeaking = !wasSpeaking;
                SetSpeakingState(wasSpeaking);
            }

            if (leaveCountdown == 0) {
                LeaveRoom(room);
            }
        }
    }
}
