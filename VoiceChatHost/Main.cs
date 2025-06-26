using VoiceChatHost.Type;
using VoiceChatHost.Windows.API;
using VoiceChatShared;

namespace VoiceChatHost
{
	public class VoiceChatHost
	{
		static TranscodeServer transcodeServer;
		static RelayServer relayServer;
		static RelayClient relayClient;

		public static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine($"no arguments specified, assuming the user wants to host a relay server");
				args = ["r", "0.0.0.0", HexaVoiceChat.Ports.relay.ToString()];
			}

			Host host = Host.GetHost(args[0]);
			HexaVoiceChat.logRecievedMessages = host.verboseLogs;

			Console.WriteLine($"start with mode \"{host.type}\" at {args[1]}");

			switch (host.type)
			{
				case Host.HostType.Transcode:
					Console.Title = $"HexaVoiceChatHost - Transcode @{args[1]}:{args[2]}";
					ApplicationIcon.Set("Assets/Windows/Icons/WithMic.ico");
					transcodeServer = new TranscodeServer(args[1], int.Parse(args[2]));
					break;
				case Host.HostType.Relay:
					Console.Title = $"HexaVoiceChatHost - Relay @{args[1]}";
					ApplicationIcon.Set("Assets/Windows/Icons/WithServer.ico");
					relayServer = new RelayServer(args[1]);
					break;
				case Host.HostType.TestTranscodeClient:
					throw new Exception($"unhandled HostType of {host.type}");
				case Host.HostType.TestRelayClient:
					relayClient = new RelayClient(args[1]);
					break;
				default:
					throw new Exception($"unhandled HostType of {host.type}");
			}
		}
	}
}