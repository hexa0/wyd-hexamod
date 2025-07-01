using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HexaMod.UI.Util
{
	public class MenuUtil
	{
		public Transform root;
		public MenuController menuController;

		public bool goingBack = false;
		public readonly Dictionary<int, int> backstates = new Dictionary<int, int>();

		public int CurrentMenu
		{
			get
			{
				for (int i = 0; i < menuController.menus.Length; i++)
				{
					if (menuController.menus[i].gameObject.activeSelf)
					{
						return i;
					}
				}

				return 0;
			}
		}

		public Transform FindMenu(string menuName)
		{
			for (int i = 0; i < menuController.menus.Count(); i++)
			{
				if (menuController.menus[i].name == menuName)
				{
					return menuController.menus[i].transform;
				}
			}

			throw new System.Exception($"menu \"{menuName}\" not found.");
		}

		public int GetMenuId(string menuName)
		{
			for (int i = 0; i < menuController.menus.Count(); i++)
			{
				if (menuController.menus[i].name == menuName)
				{
					return i;
				}
			}

			throw new System.Exception($"menu \"{menuName}\" not found.");
		}

		public void GoBack()
		{
			goingBack = true;

			if (backstates.ContainsKey(CurrentMenu))
			{
				menuController.ChangeToMenu(backstates[CurrentMenu]);
			}
			else
			{
				Mod.Warn($"no back state for {CurrentMenu}");
				menuController.ChangeToMenu(0);
			}

			goingBack = false;
		}

		public GameObject NewMenu(string name)
		{
			GameObject menu = new GameObject(name, typeof(RectTransform));

			menu.SetActive(false);
			menu.transform.SetParent(root, false);
			menu.GetComponent<RectTransform>().ScaleWithParent();

			int menuCount = menuController.menus.Count();

			var newMenus = new GameObject[menuCount + 1];
			var newStartButtons = new GameObject[menuCount + 1];

			for (int i = 0; i < menuCount; i++)
			{
				newMenus[i] = menuController.menus[i];
				newStartButtons[i] = menuController.startBtn[i];
			}

			newMenus[menuCount] = menu;
			newStartButtons[menuCount] = null;

			menuController.menus = newMenus;
			menuController.startBtn = newStartButtons;

			return menu;
		}
	}
	public static class Menu
	{
		public static Canvas menuCanvas;
		public static class WYDMenus
		{
			public static MenuUtil title;
			public static MenuUtil inGame;
			public static MenuUtil hexaMod = new MenuUtil()
			{
				root = PersistentCanvas.instance.canvas.transform.Find("Menus"),
				menuController = PersistentCanvas.instance.canvas.GetComponent<MenuController>()
			};

			public static bool AnyMenuOpen()
			{
				return inGame.menuController.menus[inGame.CurrentMenu].activeSelf || title.menuController.menus[title.CurrentMenu].activeSelf;
			}

			public static MenuUtil GetMenuUtilForController(MenuController controller)
			{
				if (title.menuController == controller)
				{
					return title;
				}
				else
				{
					return inGame;
				}
			}
		}

		public static readonly string[] hostMenusWithLobbyBackgrounds = { "DaddysNightmare", "Dadlympics", "HungryGames", "Family Gathering-Host" };
		public static readonly string[] hostMenus = { "DaddyWaitMenu", "BabyWaitMenu", "WaitMenu-Original", "DaddysNightmare", "Dadlympics", "HungryGames", "Family Gathering-Host" };

		public static void Init()
		{
			menuCanvas = Object.FindObjectOfType<anvasHelper>().GetComponent<Canvas>();
			menuCanvas.pixelPerfect = true; // sharper text

			WYDMenus.title = new MenuUtil();
			WYDMenus.inGame = new MenuUtil();

			WYDMenus.title.root = menuCanvas.Find("MainMenu").transform;
			WYDMenus.title.menuController = WYDMenus.title.root.GetComponent<MenuController>();
			WYDMenus.inGame.root = menuCanvas.Find("InGameMenu").transform;
			WYDMenus.inGame.menuController = WYDMenus.inGame.root.GetComponent<MenuController>();

			if (!PhotonNetwork.inRoom)
			{
				// anvasHelper (yes fucking anvas you heard that right) fucks with the block raycast stuff and sometimes it breaks the menu
				// this is an attempted patch
				// ^ nvm this is a unity explorer issue
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				menuCanvas.GetComponent<CanvasGroup>().blocksRaycasts = true;
				menuCanvas.Find("InGameElements").gameObject.SetActive(false);
			}
		}
	}
}
