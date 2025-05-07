using UnityEngine;

namespace HexaMapAssemblies
{
	public class MakeSickFactory : MonoBehaviour
	{
		void Start()
		{
			var sick = gameObject.AddComponent<MakeSick>();
			sick.sicknessFactor = sicknessFactor;
			sick.soundObj = soundObj;
			sick.emptyObj = emptyObj;
			sick.challengeId = challengeId;
			sick.destruct = destruct;
			sick.empty = !infiniteUses;

			if (isDrink)
			{
				sick.tag = "Drink";
			}
			else
			{
				sick.tag = "Eat";
			}
		}

		public float sicknessFactor = 0.1f;
		public GameObject soundObj;
		public bool destruct = false;
		public bool infiniteUses = false;
		public GameObject emptyObj;
		public int challengeId = 0;

		public bool isBleach;
		public bool isTrash;
		public bool isDrink = true;
	}
}
