using System.Collections.Generic;
using HexaMod.API.Util.Extensions;
using HexaMod.API.Util.Migration;
using UnityEngine;

namespace HexaMod.SDK.Levels.Scripts.Spawning.Item
{
	[UnityMigrationIdentifier("HexaMod.80e201e2-2465-4177-bdb3-f345f3b65671")]
	public class GasSpawn : MigratableMonoBehavior
	{
		public GameObject gasSpawns;
		public Transform generator;

		void Awake()
		{
			List<GameObject> gasSpots = ObjectUtils.GetChildren(gasSpawns);

			foreach (var child in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
			{
				if (child.name == "Daddys Nightmare")
				{
					DaddyNightmare dnm = child.GetComponent<DaddyNightmare>();
					dnm.spawnSpots = gasSpots;

					Transform generatorTransform = dnm.Find("Generator").transform;

					generatorTransform.position = generator.position;
					generatorTransform.rotation = generator.rotation;
					generatorTransform.localScale = generator.localScale;

					gameObject.SetActive(false);
				}
			}
		}
	}
}
