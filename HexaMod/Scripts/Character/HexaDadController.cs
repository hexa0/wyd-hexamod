namespace HexaMod.Scripts.Character
{
	internal class HexaDadController : HexaPlayerController
	{
		public HexaDadController()
		{
			teamSelector = "D";
			defaultShirtColor = "#E76F3D";
			defaultSkinColor = "#CC9485";
		}

		public override void Awake()
		{
			base.Awake();

			characterModelSwapper.SetDadModel();
			leftHand = transform.FindDeepChild("LeftDadHoldPos");
			rightHand = transform.FindDeepChild("DadHoldPos");
		}
	}
}
