using HexaMod.API.Util.Migration;
using UnityEngine;

namespace HexaMod.SDK.Levels.Scripts.Factory
{
	[UnityMigrationIdentifier("HexaMod.ef87975c-f94a-4bc3-af65-644fc2b67fc1")]
	public class GrabFactory : MigratableMonoBehavior
	{
		void Start()
		{
			Fork fork = gameObject.AddComponent<Fork>();
			fork.babyGate = placeLikeBabyGate;
			fork.babyCantGrab = babyCantGrab;
			fork.kineticAtStart = startFrozen;
			fork.changeName = false;
			fork.tag = startTag;
		}

		void PickUp(Transform player)
		{
			GetComponent<Fork>().Interact(player);
		}

		public bool placeLikeBabyGate = false;
		public bool babyCantGrab = false;
		public bool startFrozen = false;
		public string startTag = "Grab";
	}
}
