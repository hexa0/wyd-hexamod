using HexaMod.UI.Menus;
using HexaMod.UI.Util;
using UnityEngine;

namespace HexaMod.UI
{
	public static class HexaMenus
	{
		public static StartupScreen startupScreen;
		public static LoadingOverlay loadingOverlay;
		public static FadeOverlay fadeOverlay;
		public static VoiceChatDebugOverlay voiceChatDebugOverlay;
		public static MenuUtil util;
		public static AudioSource uiAudioSource;

		public static void Init()
		{
			uiAudioSource = PersistentCanvas.instance.canvas.gameObject.AddComponent<AudioSource>();
			uiAudioSource.volume = 0.1f;

			startupScreen = new StartupScreen()
				.SetParent(PersistentCanvas.instance.canvas.transform)
				.ScaleWithParent();

			fadeOverlay = new FadeOverlay()
				.SetParent(PersistentCanvas.instance.canvas.transform)
				.ScaleWithParent();

			loadingOverlay = new LoadingOverlay()
				.SetParent(PersistentCanvas.instance.canvas.transform)
				.ScaleWithParent();

			voiceChatDebugOverlay = new VoiceChatDebugOverlay()
				.SetParent(PersistentCanvas.instance.canvas.transform)
				.ScaleWithParent();

			RectTransform menuRoot = new GameObject("menuRoot", typeof(RectTransform)).GetComponent<RectTransform>();
			menuRoot.SetParent(PersistentCanvas.instance.canvas.transform, false);
			menuRoot.position = Vector3.zero;

			util = new MenuUtil()
			{
				root = menuRoot,
				menuController = PersistentCanvas.instance.canvas.GetComponent<MenuController>()
			};
		}
	}
}
