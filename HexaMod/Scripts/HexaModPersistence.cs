using HexaMod.Scripts;
using UnityEngine;

namespace HexaMod
{
	public class HexaModPersistence : MonoBehaviour
	{
		public static HexaModPersistence instance;
		void Awake()
		{
			instance = this;

			DontDestroyOnLoad(gameObject);

			new GameObject("AsyncAssetLoader").AddComponent<AsyncAssetLoader>().SetParent(transform);
			new GameObject("HexaPersistentLobby").AddComponent<HexaPersistentLobby>().SetParent(transform);
			new GameObject("TabOutMute").AddComponent<TabOutMute>().SetParent(transform);
			new GameObject("PreferenceLinker").AddComponent<PreferenceLinker>().SetParent(transform);
			new GameObject("PersistentCanvas").AddComponent<PersistentCanvas>().SetParent(transform);
		}
	}
}
