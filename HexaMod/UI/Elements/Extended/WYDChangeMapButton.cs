namespace HexaMod.UI.Elements.Extended
{
	public class WYDChangeMapButton : WYDTextButton
	{
		internal readonly bool inGame;

		public override void Update()
		{
			base.Update();

			SetInteractable(!inGame && (PhotonNetwork.isMasterClient || PhotonNetwork.room == null));
		}
		public WYDChangeMapButton(bool inGame) : base()
		{
			this.inGame = inGame;

			this.SetName("changeMap")
				.SetTextAuto("Map");
		}
	}
}
