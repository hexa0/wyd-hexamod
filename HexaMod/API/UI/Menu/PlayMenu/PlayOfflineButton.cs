using HexaMod.API.UI.Element.Control.TextButton;

namespace HexaMod.API.UI.Menu.PlayMenu
{
	public class PlayOfflineButton : WTextButton
	{
		void AttemptToConnect()
		{
			if (!PhotonNetwork.connected && !PhotonNetwork.connecting)
			{
				PhotonNetwork.ConnectUsingSettings($"{BuildInfo.AssemblyName}-{BuildInfo.Version}-{BuildInfo.GitHash}");
			}
		}

		public override void Shown()
		{
			base.Shown();

			if (!PhotonNetwork.offlineMode)
			{
				AttemptToConnect();
			}
		}

		public PlayOfflineButton() : base()
		{
			this.SetTextAuto(PhotonNetwork.offlineMode ? "Playing\nOffline" : "Playing\nOnline")
				.AddListener(() =>
				{
					if (!PhotonNetwork.offlineMode)
					{
						PhotonNetwork.Disconnect();
						PhotonNetwork.offlineMode = true;
					}
					else
					{
						PhotonNetwork.Disconnect();
						PhotonNetwork.offlineMode = false;
						AttemptToConnect();
					}

					SetTextAuto(PhotonNetwork.offlineMode ? "Playing\nOffline" : "Playing\nOnline");
				});
		}
	}
}
