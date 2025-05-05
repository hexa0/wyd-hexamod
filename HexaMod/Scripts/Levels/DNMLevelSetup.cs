using System.Collections.Generic;
using NAudio.SoundFont;
using UnityEngine;

namespace HexaMapAssemblies
{
	public class DNMLevelSetup : MonoBehaviour
	{
		public GameObject gasSpawns;
		public Transform generator;

		void Awake()
		{
			Transform[] gasTransforms = gasSpawns.GetComponentsInChildren<Transform>();
			List<GameObject> gasSpots = new List<GameObject>();

			foreach (var item in gasTransforms)
			{
				if (item.transform.parent == gasSpawns.transform)
				{
					gasSpots.Add(item.gameObject);
				}
			}

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
