namespace HexaMod.Scripts.Character.Controller.Character
{
	internal class HexaBabyController : HexaPlayerController
	{
		public HexaBabyController()
		{
			teamSelector = "B";
			defaultSkinColor = "#CDA7A4";
		}

		public override void Awake()
		{
			base.Awake();

			characterModelSwapper.SetBabyModel();
			leftHand = transform.FindDeepChild("LeftBabyHoldPos");
			rightHand = transform.FindDeepChild("BabyHoldPos");
		}

		public override void Start()
		{
			base.Start();

			characterInteraction.reach -= 2f;
			characterInteraction.pickupMask = 1 << CharacterInteraction.babyGrabableLayer | 1 << CharacterInteraction.toyLayer;
		}
	}
}
