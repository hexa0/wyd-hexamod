using HexaMod.API.Util.Migration;

namespace HexaMod.SDK.Levels.Scripts.Factory
{
	[UnityMigrationIdentifier("HexaMod.ca2ebc1d-8ae4-4130-ae22-1e1d87d77e79")]
	public class PickUpFactory : MigratableMonoBehavior
	{
		void Start()
		{
			if (dadBod)
			{
				gameObject.layer = 0;
			}
			else if (babyCannotGrab)
			{
				gameObject.layer = 11;
			}
			else
			{
				gameObject.layer = 26;
			}

			var pickup = gameObject.AddComponent<PickUp>();
			pickup.isDildo = isDildo;
			pickup.isTrophy = isTrophy;
			pickup.choreDoer = choreDoer;
			pickup.holding = "";
			pickup.lastHolder = "";
			pickup.player = player;

			Destroy(this);
		}

		public bool babyCannotGrab = false;
		public bool dadBod = false;
		public bool isDildo = false;
		public bool isTrophy = false;
		public string choreDoer = "";
		public string player = "BabyCam";
	}
}
