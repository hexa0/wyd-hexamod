namespace HexaMod.Scripts.Character.Controller.Character
{
	internal class HexaLuigiController : HexaPlayerController
	{
		public HexaLuigiController()
		{
			teamSelector = "L";
			defaultShirtColor = "#339933";
			defaultSkinColor = "#CC9485";
		}

		public override void Awake()
		{
			base.Awake();

			characterModelSwapper.SetDadModel();
		}

		public override void Start()
		{
			base.Start();

			JumpForce *= 1.25f;
		}
	}
}
