using UnityEngine;

namespace HexaMapAssemblies
{
	public class PickUpFactory : MonoBehaviour
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
		}

		public bool babyCannotGrab = false;
		public bool dadBod = false;
		public bool isDildo = false;
		public bool isTrophy = false;
		public string choreDoer = "";
		public string player = "BabyCam";
	}
}
