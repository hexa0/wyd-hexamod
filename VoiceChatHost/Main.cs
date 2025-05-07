using HexaVoiceChatShared;
using VoiceChatHost.Type;

namespace VoiceChatHost
{
	public class VoiceChatHost
	{
		public static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				throw new ArgumentException("not enough arguments");
			}

			Host host = Host.GetHost(args[0]);
			HexaVoiceChat.logRecievedMessages = host.verboseLogs;

			Console.WriteLine($"start with mode \"{host.type}\" at {args[1]}");

			switch (host.type)
			{
				case Host.HostType.Transcode:
					new TranscodeServer(args[1], int.Parse(args[2]));
					break;
				case Host.HostType.Relay:
					new RelayServer(args[1]);
					break;
				case Host.HostType.TestTranscodeClient:
					throw new Exception($"unhandled HostType of {host.type}");
				case Host.HostType.TestRelayClient:
					RelayClient client = new RelayClient(args[1]);
					new Thread(new ThreadStart(client.RelayClientMainTestThread)).Start();
					break;
				default:
					throw new Exception($"unhandled HostType of {host.type}");
			}
		}
	}
}