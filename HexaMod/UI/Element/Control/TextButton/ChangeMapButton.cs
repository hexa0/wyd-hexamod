using HexaMod.UI.Element.Control.TextButton;

namespace HexaMod.UI.Element.Extended
{
	public class ChangeMapButton : WTextButton
	{
		internal readonly bool inGame;

		public override void Update()
		{
			base.Update();

			SetInteractable(!inGame && (PhotonNetwork.isMasterClient || PhotonNetwork.room == null));
		}
		public ChangeMapButton(bool inGame) : base()
		{
			this.inGame = inGame;

			this.SetName("changeMap")
				.SetTextAuto("Map");
		}
	}
}
