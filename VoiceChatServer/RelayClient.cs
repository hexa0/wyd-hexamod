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
        public Action<ulong, byte[], int>? onOpusAction;
        string? room = null;

        public void JoinRoom(string roomName)
        {
            client.SendMessage(ClientWrappedMessage.BuildMessage(
                clientId,
                VoiceChatMessageType.VoiceRoomJoin,
                Encoding.ASCII.GetBytes(roomName)
            ));

            room = roomName;
        }

        public void KeepRoomAlive()
        {
            if (room != null)
            {
                client.SendMessage(ClientWrappedMessage.BuildMessage(
                    clientId,
                    VoiceChatMessageType.VoiceRoomKeepAlive,
                    Encoding.ASCII.GetBytes(room)
                ));
            }
            else
            {
                throw new Exception("cannot call KeepRoomAlive() when we aren't connected to a room.");
            }
        }

        public void SetSpeakingState(bool speaking)
        {
            client.SendMessage(ClientWrappedMessage.BuildMessage(
                clientId,
                VoiceChatMessageType.SpeakingStateUpdated,
                speaking ? [1] : [0]
            ));
        }

        public void SendOpus(byte[] encoded, int samples)
        {
            byte[] sampleCount = BitConverter.GetBytes(samples);
            byte[] opusMessage = new byte[encoded.Length + sampleCount.Length];
            Buffer.BlockCopy(sampleCount, 0, opusMessage, 0, sampleCount.Length);
            Buffer.BlockCopy(encoded, 0, opusMessage, sampleCount.Length, encoded.Length);
            client.SendMessage(ClientWrappedMessage.BuildMessage(
                clientId,
                VoiceChatMessageType.Opus,
                opusMessage
            ));
        }

        public void LeaveRoom()
        {
            if (room != null)
            {
                client.SendMessage(ClientWrappedMessage.BuildMessage(
                    clientId,
                    VoiceChatMessageType.VoiceRoomLeave,
                    Encoding.ASCII.GetBytes(room)
                ));

                room = null;
            }
            else
            {
                throw new Exception("cannot call LeaveRoom() when we aren't connected to a room.");
            }
        }

        public void SwitchRelay(string relayIp)
        {
            if (room != null)
            {
                LeaveRoom();
            }

            client.SwitchToEndPoint(new IPEndPoint(IPAddress.Parse(relayIp), HexaVoiceChat.Ports.relay));
        }

        public RelayClient(string ip)
        {
            client = new VoiceChatClient(new IPEndPoint(
                IPAddress.Parse(ip),
                HexaVoiceChat.Ports.relay
            ));

            client.OnMessage(VoiceChatMessageType.SpeakingStateUpdated, delegate (DecodedVoiceChatMessage message, IPEndPoint from)
            {
                DecodedClientWrappedMessage clientMessage = ClientWrappedMessage.DecodeMessage(message.body);
                Console.WriteLine($"client {clientMessage.clientId}'s voice chat state updated to {clientMessage.body[0]}");
            });

            client.OnMessage(VoiceChatMessageType.Opus, delegate (DecodedVoiceChatMessage message, IPEndPoint from)
            {
                DecodedClientWrappedMessage clientMessage = ClientWrappedMessage.DecodeMessage(message.body);

                if (onOpusAction != null)
                {
                    byte[] opusFrame = new byte[clientMessage.body.Length - 4];
                    Buffer.BlockCopy(clientMessage.body, 4, opusFrame, 0, clientMessage.body.Length - 4);
                    onOpusAction.Invoke(clientMessage.clientId, opusFrame, BitConverter.ToInt32(clientMessage.body, 0));
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
