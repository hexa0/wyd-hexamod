using UnityEngine;
using HexaMod.Util;


namespace HexaMapAssemblies
{
	public class HGLevelSetup : MonoBehaviour
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
