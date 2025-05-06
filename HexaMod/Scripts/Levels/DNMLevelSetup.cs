using System.Collections.Generic;
using NAudio.SoundFont;
using UnityEngine;

namespace HexaMapAssemblies
{
	public class DNMLevelSetup : MonoBehaviour
	{
		public GameObject gasSpawns;
		public Transform generator;
		public Transform itemSpawns;

		List<GameObject> GetChildren(GameObject parent)
		{
			List<GameObject> children = new List<GameObject>();

			foreach (var item in parent.GetComponentsInChildren<Transform>())
			{
				if (item.transform.parent == parent.transform)
				{
					children.Add(item.gameObject);
				}
			}

			return children;
		}

		void Awake()
		{
			List<GameObject> gasSpots = GetChildren(gasSpawns);

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
