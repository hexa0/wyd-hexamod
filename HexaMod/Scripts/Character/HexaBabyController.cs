namespace HexaMod.Scripts.Character
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

			characterItemInteraction.reach -= 2f;
			characterItemInteraction.pickupMask = (1 << CharacterItemInteraction.babyGrabableLayer) | (1 << CharacterItemInteraction.toyLayer);
		}
	}
}
