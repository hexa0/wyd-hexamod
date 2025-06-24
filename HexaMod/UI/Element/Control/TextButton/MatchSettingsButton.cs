using HexaMod.UI.Element.Control.TextButton;

namespace HexaMod.UI.Element.Extended
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
