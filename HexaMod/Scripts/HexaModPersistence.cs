using UnityEngine;

namespace HexaMod
{
	public class HexaModPersistence : MonoBehaviour
	{
		void Awake()
		{
			HexaMod.asyncAssetLoader = gameObject.AddComponent<AsyncAssetLoader>();
			HexaMod.persistentLobby = gameObject.AddComponent<HexaPersistentLobby>();
			HexaMod.tabOutMute = gameObject.AddComponent<TabOutMute>();
			HexaMod.preferenceLinker = gameObject.AddComponent<PreferenceLinker>();
		}
	}
}
