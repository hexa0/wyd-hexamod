using System.Collections.Generic;
using UnityEngine;
using HexaMod.Util;

namespace HexaMapAssemblies
{
	public class DNMLevelSetup : MonoBehaviour
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

					Transform generatorTransform = dnm.transform.Find("Generator").transform;

					generatorTransform.position = generator.position;
					generatorTransform.rotation = generator.rotation;
					generatorTransform.localScale = generator.localScale;

					gameObject.SetActive(false);
				}
			}
		}
	}
}
