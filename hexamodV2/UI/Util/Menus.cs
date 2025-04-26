using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.UI.Util
{
    public static class Menus
    {
        public static Transform root;
        public static Canvas menuCanvas;
        public static MenuController menuController;

        public static Transform titleScreen;
        public static Transform characterCustomization;
        public static Transform online;
        public static Transform gameList;

        public static bool goingBack = false;
        public static Dictionary<int, int> backstates = new Dictionary<int, int>();

        public static string[] hostMenusWithLobbyBackgrounds = { "DaddysNightmare", "Dadlympics", "HungryGames", "Family Gathering-Host" };
        public static string[] hostMenus = { "DaddyWaitMenu", "BabyWaitMenu", "WaitMenu-Original", "DaddysNightmare", "Dadlympics", "HungryGames", "Family Gathering-Host" };
        public static int currentMenu
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

        public static void Init()
        {
            PhotonNetworkManager networkManager = GameObject.Find("NetworkManager").GetComponent<PhotonNetworkManager>();

            Mod.Print("init menu references");

            root = GameObject.Find("MainMenu").transform;
            menuCanvas = root.parent.GetComponent<Canvas>();
            menuController = root.GetComponent<MenuController>();
            backstates.Clear();

            titleScreen = root.Find("SplashMenu");
            characterCustomization = root.Find("CharacterCustomizationMenu");
            online = root.Find("OnlineMenu");
            gameList = root.Find("GameListMenu");

            if (!PhotonNetwork.inRoom)
            {
                // anvasHelper (yes fucking anvas you heard that right) fucks with the block raycast stuff and sometimes it breaks the menu
                // this is an attempted patch
                // ^ nvm this is a unity explorer issue
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                menuCanvas.GetComponent<CanvasGroup>().blocksRaycasts = true;
                menuCanvas.transform.Find("InGameElements").gameObject.SetActive(false);
            }
        }

        public static int FindMenu(string menuName)
        {
            for (int i = 0; i < menuController.menus.Count(); i++)
            {
                if (menuController.menus[i].name == menuName)
                {
                    return i;
                }
            }

            throw new System.Exception("menu not found.");
        }

        public static void GoBack()
        {
            goingBack = true;

            if (backstates.ContainsKey(currentMenu))
            {
                // Mod.Print($"go back from {currentMenu} to {backstates[currentMenu]}");
                menuController.ChangeToMenu(backstates[currentMenu]);
            }
            else
            {
                Mod.Warn($"no back state for {currentMenu}");
                menuController.ChangeToMenu(0);
            }

            goingBack = false;
        }

        public static GameObject NewMenu(string name)
        {
            GameObject menu = new GameObject(name, typeof(RectTransform));

            menu.SetActive(false);
            menu.transform.SetParent(root);
            menu.transform.position = Vector3.zero;
            menu.transform.rotation = Quaternion.identity;
            menu.transform.localScale = Vector3.one;

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

        public static void DoTheming()
        {
            var spriteInputField128 = HexaMod.coreBundle.LoadAsset<Sprite>("Assets/ModResources/Core/Sprite/InputField128.png");
            var spriteButton = HexaMod.coreBundle.LoadAsset<Sprite>("Assets/ModResources/Core/Sprite/Button.png");

            Mod.Print("do theming");
            { // Lobby Join Button
                var gameJoiner = HexaMod.networkManager.gameJoiner;
                var joinButton = gameJoiner.GetComponentInChildren<Button>();
                var joinImage = gameJoiner.GetComponentInChildren<Image>();

                var newColors = joinButton.colors;
                newColors.highlightedColor = new Color(1f, 1f, 1f);
                newColors.normalColor = new Color(255f / 255f, 173f / 255f, 168f / 255f);
                newColors.disabledColor = new Color(newColors.normalColor.r, newColors.normalColor.g, newColors.normalColor.b, 0.5f);

                joinButton.colors = newColors;
                joinImage.sprite = spriteButton;
                joinImage.color = Color.white;
            }
            { // Lobby BG
                foreach (var hostMenu in hostMenusWithLobbyBackgrounds)
                {
                    var playerNames = root.Find(hostMenu).Find("PlayerNames");
                    foreach (var image in playerNames.GetComponentsInChildren<Image>(true))
                    {
                        image.sprite = spriteInputField128;
                        image.color = new Color(15f / 255f, 13f / 255f, 13f / 255f, 0.5f);
                    }

                }
            }
            { // Toggles
                foreach (var toggleComponent in menuCanvas.GetComponentsInChildren<Toggle>(true))
                {
                    GameObject background = toggleComponent.transform.GetChild(0).gameObject;
                    GameObject check;

                    if (background)
                    {
                        var image = background.GetComponent<Image>();
                        image.sprite = spriteInputField128;
                        image.color = new Color(1f, 1f, 1f, 0.9f);
                        check = background.transform.GetChild(0).gameObject;

                        if (check)
                        {

                        }
                    }

                    var newColors = toggleComponent.colors;
                    newColors.highlightedColor = new Color(1f, 1f, 1f);
                    newColors.normalColor = new Color(255f / 255f, 173f / 255f, 168f / 255f);
                    newColors.disabledColor = new Color(newColors.normalColor.r, newColors.normalColor.g, newColors.normalColor.b, 0.5f);
                    newColors.pressedColor = new Color(255f / 255f, 173f / 255f, 168f / 255f);

                    toggleComponent.colors = newColors;
                }
            }
            { // Scrollbars
                foreach (var scrollbarComponent in menuCanvas.GetComponentsInChildren<Scrollbar>(true))
                {
                    var image = scrollbarComponent.GetComponent<Image>();

                    if (image)
                    {
                        var newColors = scrollbarComponent.colors;
                        newColors.highlightedColor = new Color(1f, 1f, 1f);
                        newColors.normalColor = new Color(255f / 255f, 173f / 255f, 168f / 255f);
                        newColors.disabledColor = new Color(newColors.normalColor.r, newColors.normalColor.g, newColors.normalColor.b, 0.5f);

                        scrollbarComponent.colors = newColors;

                        image.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
                        // image.sprite = spriteInputField128;
                    }
                }
            }
            { // ScrollRects
                foreach (var scrollRectComponent in menuCanvas.GetComponentsInChildren<ScrollRect>(true))
                {
                    var image = scrollRectComponent.GetComponent<Image>();

                    if (image)
                    {
                        image.color = new Color(15f / 255f, 13f / 255f, 13f / 255f, 0.9f);
                        image.sprite = spriteInputField128;
                        image.type = Image.Type.Sliced;
                    }
                }
            }
            { // Input Fields
                foreach (var inputFieldComponent in menuCanvas.GetComponentsInChildren<InputField>(true))
                {
                    var image = inputFieldComponent.GetComponent<Image>();

                    if (image)
                    {
                        var newColors = inputFieldComponent.colors;
                        newColors.highlightedColor = new Color(51f / 255f, 29f / 255f, 33f / 255f);
                        newColors.normalColor = new Color(15f / 255f, 13f / 255f, 13f / 255f);
                        newColors.disabledColor = new Color(newColors.normalColor.r, newColors.normalColor.g, newColors.normalColor.b, 0.5f);

                        inputFieldComponent.colors = newColors;

                        image.color = new Color(1f, 1f, 1f, 0.9f);
                        image.sprite = spriteInputField128;
                        image.type = Image.Type.Sliced;

                        foreach (var text in inputFieldComponent.GetComponentsInChildren<Text>(true))
                        {
                            text.color = new Color(1f, 1f, 1f);
                            // text.font = HexaModMain.bundle.LoadAsset<Font>("Assets/ModResources/Font/osd.ttf");
                            text.fontSize = (int)(text.fontSize * 0.8f);
                        }
                    }
                }
            }
            { // Buttons
                var originalColor = titleScreen.Find("PlayLocal").GetComponent<Button>().colors.highlightedColor;

                foreach (var buttonComponent in menuCanvas.GetComponentsInChildren<Button>(true))
                {
                    var ourColor = buttonComponent.colors.highlightedColor;

                    if (ourColor.r == originalColor.r && ourColor.g == originalColor.g && ourColor.b == originalColor.b)
                    {
                        var image = buttonComponent.GetComponent<Image>();

                        if (image)
                        {
                            var newColors = buttonComponent.colors;
                            newColors.highlightedColor = new Color(51f / 255f, 29f / 255f, 33f / 255f);
                            newColors.normalColor = new Color(15f / 255f, 13f / 255f, 13f / 255f);
                            newColors.disabledColor = new Color(newColors.normalColor.r, newColors.normalColor.g, newColors.normalColor.b, 0.5f);

                            buttonComponent.colors = newColors;

                            image.color = new Color(1f, 1f, 1f, 0.9f);
                            image.sprite = spriteButton;

                            foreach (var text in buttonComponent.GetComponentsInChildren<Text>(true))
                            {
                                text.color = new Color(1f, 1f, 1f);
                                // text.font = HexaModMain.bundle.LoadAsset<Font>("Assets/ModResources/Font/osd.ttf");
                                text.fontSize = (int)(text.fontSize * 0.8f);
                            }
                        }
                    }
                }
            }
        }
    }
}
