namespace HexaMod.API.UI.Element.Control.TextButton
{
	public class MatchSettingsButton : WTextButton
	{
		public override void Update()
		{
			base.Update();

			SetInteractable(PhotonNetwork.isMasterClient || PhotonNetwork.room == null);
		}
		public MatchSettingsButton() : base()
		{
			this.SetName("matchSettings")
				.SetTextAuto("Match\nSettings");
		}
	}
}
