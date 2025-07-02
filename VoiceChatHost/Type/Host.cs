namespace VoiceChatHost.Type
{
	public class Host
	{
		public enum HostType
		{
			Transcode,
			Relay,
			TestRelayClient,
			TestTranscodeClient
		}

		public static Host GetHost(string flags)
		{
			bool verboseLogs = flags.Contains('v');

			if (flags.Contains('c'))
			{
				Console.Clear();
			}

			if (flags.Contains('t'))
			{
				return new Host(HostType.Transcode, verboseLogs);
			}
			else if (flags.Contains('r'))
			{
				return new Host(HostType.Relay, verboseLogs);
			}
			else if (flags.Contains('T'))
			{
				return new Host(HostType.TestTranscodeClient, verboseLogs);
			}
			else if (flags.Contains('R'))
			{
				return new Host(HostType.TestRelayClient, verboseLogs);
			}
			else
			{
				throw new Exception("no valid flag for HostType passed\nvalid flags:\n\tt: Transcode\n\tr: Relay\n\tT: TestTranscodeClient\n\tR: TestRelayClient");
			}
		}

		public HostType type;
		public bool verboseLogs;

		Host(HostType type, bool verboseLogs)
		{
			this.type = type;
			this.verboseLogs = verboseLogs;
		}
	}
}
