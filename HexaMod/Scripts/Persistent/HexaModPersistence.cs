using System.Collections;
using UnityEngine;

namespace HexaMod.Scripts.Persistent
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
			new GameObject("Music").AddComponent<PersistentMusic>().SetParent(transform);

			StartCoroutine(ForceNativeResolution());
		}

		public static void RunForceNativeResolution()
		{
			instance.StartCoroutine(instance.ForceNativeResolution());
		}

		private IEnumerator ForceNativeResolution()
		{
			Resolution nativeRes = Screen.currentResolution;

			if (nativeRes.width <= 0 || nativeRes.height <= 0)
			{
				Mod.Warn("Screen.currentResolution is 0x0, assuming 1080p 60fps as a fallback");
				nativeRes.width = 1920;
				nativeRes.height = 1080;
				nativeRes.refreshRate = 60;
			}


			Mod.Print($"using {nativeRes.width}x{nativeRes.height} as resolution");

			Screen.SetResolution(64, 64, false);
			yield return null;
			Screen.SetResolution(nativeRes.width, nativeRes.height, true);
			// GL.Viewport(new Rect(0, 0, nativeRes.width, nativeRes.height));
			yield return null;
			UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
		}
	}
}
