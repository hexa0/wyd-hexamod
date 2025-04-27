using System;
using System.Net.Sockets;
using System.Net;
using HexaVoiceChatShared.MessageProtocol;

namespace HexaVoiceChatShared.Net
{
    public class VoiceChatClient : VoiceChatNetBase
    {
        public VoiceChatClient(IPEndPoint remote)
        {
            Console.WriteLine($"VoiceChatClient: Connected to port {remote.Port} at {remote.Address}");
            endPoint = remote;
            socket = new UdpClient();
            onMessageAction = delegate (DecodedVoiceChatMessage message, IPEndPoint endPoint)
            {
                if (onMessageActions.ContainsKey(message.type))
                {
                    onMessageActions[message.type].Invoke(message, endPoint);
                }
                else
                {
                    Console.WriteLine($"VoiceChatMessageType of \"{message.type}\" wasn't handled by the VoiceChatClient");
                }
            };

            Connect(true);
        }
    }
}
