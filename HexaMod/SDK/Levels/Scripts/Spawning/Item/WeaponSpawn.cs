using HexaMod.API.Util.Extensions;
using HexaMod.API.Util.Migration;
using UnityEngine;

namespace HexaMod.SDK.Levels.Scripts.Spawning.Item
{
	[UnityMigrationIdentifier("HexaMod.f0e752cb-560f-4753-a75f-c2da824fbc32")]
	public class WeaponSpawn : MigratableMonoBehavior
	{
		public GameObject weaponSpawns;
		public GameObject powerupSpawns;
		public GameObject turretSpawns;
		public GameObject trapSpawns;

		void Awake()
		{
			HungryGamesItemSpawner hg = GameObject.Find("HungryGamesItemSpawner").GetComponent<HungryGamesItemSpawner>();

			hg.pickupsPos = ObjectUtils.GetChildren(powerupSpawns).ToArray();
			hg.weaponsPos = ObjectUtils.GetChildren(weaponSpawns).ToArray();
			hg.turretSpawnSpot = ObjectUtils.GetChildren(turretSpawns).ToArray();
			hg.trapsPos = ObjectUtils.GetChildren(trapSpawns).ToArray();
			hg.trapsPos = ObjectUtils.GetChildren(trapSpawns).ToArray();
		}
	}
}
