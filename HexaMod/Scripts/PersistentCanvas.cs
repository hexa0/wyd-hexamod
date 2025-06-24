using HexaMod.UI.Element.Utility;
using UnityEngine;

namespace HexaMod
{
	public class PersistentCanvas : MonoBehaviour
	{
		public static PersistentCanvas instance;

		public Canvas canvas;

		void Awake()
		{
			instance = this;

			canvas = new GameObject($"canvas", typeof(RectTransform)).AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.sortingOrder = 1;
			canvas.transform.SetParent(transform, false);

			MenuController menuController = canvas.gameObject.AddComponent<MenuController>();
			menuController.menus = new GameObject[0];
			menuController.waitToShow = true;

			gameObject.AddComponent<IntroScript>();
		}
	}
}
