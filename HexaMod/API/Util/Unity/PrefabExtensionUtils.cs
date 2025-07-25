using System.Collections.Generic;
using UnityEngine;

namespace HexaMod.API.Util.Unity
{
	public static class PrefabExtensionUtils
	{
		public static readonly Dictionary<string, GameObject> customPrefabs = new Dictionary<string, GameObject>();
		public static GameObject customPrefabStorage = new GameObject("customPrefabStorage");
		public static Transform Storage => customPrefabStorage.transform;

		public static GameObject GetCachedNetworkPrefab(string prefabName)
		{
			if (!PhotonNetwork.PrefabCache.ContainsKey(prefabName))
			{
				PhotonNetwork.PrefabCache[prefabName] = (GameObject)Resources.Load(prefabName, typeof(GameObject));
			}

			return PhotonNetwork.PrefabCache[prefabName];
		}

		public static void RegisterCustomPrefab(string prefabName, GameObject prefab)
		{
			PhotonNetwork.PrefabCache[prefabName] = prefab;
			customPrefabs[prefabName] = prefab;
			prefab.transform.SetParent(customPrefabStorage.transform);
		}
	}
}
