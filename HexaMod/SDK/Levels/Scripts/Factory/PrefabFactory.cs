using HexaMod.API.Util.Migration;
using UnityEngine;

namespace HexaMod.SDK.Levels.Scripts.Factory
{
	[UnityMigrationIdentifier("HexaMod.d29bd2e3-bbdf-4f71-87c3-a191822d8469")]
	public class PrefabFactory : MigratableMonoBehavior
	{
		void Start()
		{
			var instantiated = Instantiate((GameObject)Resources.Load(PrefabName, typeof(GameObject)), transform.position, transform.rotation);
			instantiated.name = instantiated.name.Replace("(Clone)", "");

			if (Assets.loadedLevelInstance)
			{
				instantiated.transform.SetParent(Assets.loadedLevelInstance);
			}

			GlobalPhotonFactory.Register(instantiated);

			Destroy(gameObject);
		}

		public string PrefabName = "Baby Gate";
	}
}
