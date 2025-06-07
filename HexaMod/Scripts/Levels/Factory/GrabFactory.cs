using UnityEngine;

namespace HexaMapAssemblies
{
	public class GrabFactory : MonoBehaviour
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
