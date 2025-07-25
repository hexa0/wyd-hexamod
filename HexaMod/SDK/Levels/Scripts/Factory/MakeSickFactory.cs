using HexaMod.API.Util.Migration;
using UnityEngine;

namespace HexaMod.SDK.Levels.Scripts.Factory
{
	[UnityMigrationIdentifier("HexaMod.8a432151-1daf-421a-b84a-2af8ae605994")]
	public class MakeSickFactory : MigratableMonoBehavior
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

			Destroy(this);
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
