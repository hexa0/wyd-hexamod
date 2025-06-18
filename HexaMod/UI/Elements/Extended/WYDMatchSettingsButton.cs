namespace HexaMod.UI.Elements.Extended
{
	public class WYDMatchSettingsButton : WYDTextButton
	{
		public override void Update()
		{
			base.Update();

			SetInteractable(PhotonNetwork.isMasterClient || PhotonNetwork.room == null);
		}
		public WYDMatchSettingsButton() : base()
		{
			this.SetName("matchSettings")
				.SetTextAuto("Match\nSettings");
		}
	}
}
