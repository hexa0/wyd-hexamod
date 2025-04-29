using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using HexaMod.UI.Util;
using HexaMod.Util;
using System.Linq;
using HexaMod.ScriptableObjects;
using System.Collections.Generic;
using HexaMod.Voice;

namespace HexaMod.UI
{
    public class MainUI : MonoBehaviour
    {
        static InputField currentShirtColorInputField;

        internal static class ButtonCallbacks
        {
            public static void MatchSettingsButton()
            {
                Menus.menuController.ChangeToMenu(Menus.FindMenu("MatchSettings"));
            }
            public static void ChangeMapButton()
            {
                Menus.menuController.ChangeToMenu(Menus.FindMenu("ChangeMap"));
            }
            public static void TestDadButton()
            {
                HexaMod.MakeTestGame(true);
            }

            public static void TestBabyButton()
            {
                HexaMod.MakeTestGame(false);
            }

            public static void SaveShirtColor(string hex)
            {
                bool isRouglyValid = (hex.Length == 7 && hex.StartsWith("#")) || (hex.Length == 6 && !hex.StartsWith("#"));

                if (!isRouglyValid)
                {
                    currentShirtColorInputField.text = GetCurrentShirtColorHex();
                }
                else
                {
                    try
                    {
                        PlayerPrefs.SetString("HMV2_ShirtColor", currentShirtColorInputField.text);
                    }
                    catch
                    {
                        currentShirtColorInputField.text = GetCurrentShirtColorHex();
                    }
                }
            }

            public static void UpdateShirtColorVisual(string hex)
            {
                bool isRouglyValid = (hex.Length == 7 && hex.StartsWith("#")) || (hex.Length == 6 && !hex.StartsWith("#"));

                if (isRouglyValid)
                {
                    try
                    {
                        GameObject characterPreviewCanvas = GameObject.Find("BackendObjects").transform.Find("MenuCamera").Find("Camera").Find("Canvas").gameObject;
                        characterPreviewCanvas.transform.Find("Dad").Find("generic_male_01.005").GetComponent<SkinnedMeshRenderer>().materials[4].color = HexToColor.GetColorFromHex(hex);
                        lastKnownGoodInputFieldText = currentShirtColorInputField.text;
                    }
                    catch
                    {
                        currentShirtColorInputField.text = lastKnownGoodInputFieldText;
                        Mod.Print(currentShirtColorInputField.text);
                    }
                }
                else
                {
                    if (hex.Length > (hex.StartsWith("#") ? 7 : 6))
                    {
                        currentShirtColorInputField.text = lastKnownGoodInputFieldText;
                    }
                }
            }

            public static void ChangeLevel(string mapName)
            {
                Mod.Print($"change to {mapName}");
                PlayerPrefs.SetString("HMV2_CustomMap", mapName);

                HexaMod.persistentLobby.lobbySettings.mapName = mapName;
                HexaMod.persistentLobby.CommitChanges();
            }
        }

        public void UpdateUIForLobbyState()
        {
            ModLevel foundLevel = Levels.titleLevel;

            foreach (var level in Levels.levels)
            {
                if (level.levelPrefab.name == HexaMod.persistentLobby.lobbySettings.mapName)
                {
                    foundLevel = level;
                    break;
                }
            }

            var gameModesCustom = Menus.gameList.Find("GameCreator").Find("GameModes");

            gameModesCustom.Find("Original").Find("CreateGame").GetComponent<Button>().interactable = foundLevel.regular;
            gameModesCustom.Find("Family Gathering").Find("CreateGame").GetComponent<Button>().interactable = foundLevel.familyGathering;
            gameModesCustom.Find("The Hungry Games").Find("CreateGame").GetComponent<Button>().interactable = foundLevel.hungryGames;
            gameModesCustom.Find("The Great Dadlympics").Find("CreateGame").GetComponent<Button>().interactable = foundLevel.dadlympics;
            gameModesCustom.Find("Daddy's Nightmare").Find("CreateGame").GetComponent<Button>().interactable = foundLevel.daddysNightmare;

            Menus.root.Find("Family Gathering-Host").Find("Start").GetComponent<Button>().interactable = foundLevel.familyGathering;
            Menus.root.Find("HungryGames").Find("Start").GetComponent<Button>().interactable = foundLevel.hungryGames;
            Menus.root.Find("Dadlympics").Find("Start").GetComponent<Button>().interactable = foundLevel.dadlympics;
            Menus.root.Find("DaddysNightmare").Find("Start").GetComponent<Button>().interactable = foundLevel.daddysNightmare;

            mapInfo.text = $"{foundLevel.levelNameReadable}\n{foundLevel.levelDescriptionReadable}";

            foreach (Button matchSettings in matchSettingButtons)
            {
                matchSettings.interactable = PhotonNetwork.isMasterClient || !PhotonNetwork.inRoom;
            }
        }

        public void OnLevelsLoaded()
        {
            HexaMod.mainUI.loadingController.SetTaskState("LoadingMaps", false);

            UpdateUIForLobbyState();

            // Map Menu

            Mod.Print("make ChangeMap menu");

            GameObject menu = Menus.NewMenu("ChangeMap");
            int menuId = Menus.FindMenu(menu.name);
            Button backButton = Templates.MakeBackButton(menu.transform);
            Menus.menuController.startBtn[menuId] = backButton.gameObject;

            Vector2 center = new Vector2(1920f / 2f, 1080f / 2f);

            void Reset()
            {
                ButtonCallbacks.ChangeLevel("Default");
            }

            Templates.NewButton(
                "resetMapToDefault", "Default", menu.transform,
                backButton.transform.localPosition + new Vector3(
                    Templates.ButtonGap.x,
                    0,
                    0
                ),
                new UnityAction[] { Reset, Menus.GoBack }
            );

            float height = Mathf.Clamp(Levels.levels.Count() - 1, 0f, 4f);
            float width = Mathf.Floor((Levels.levels.Count() - 1) / 5f);

            for (int i = 0; i < Levels.levels.Count(); i++)
            {
                ModLevel level = Levels.levels[i];

                float y = (-(i % 5) + (height / 2f)) + 0.5f;
                float x = Mathf.Floor(i / 5f) - (width / 2f);

                void Press()
                {
                    ButtonCallbacks.ChangeLevel(level.levelPrefab.name);
                }

                Button button = Templates.NewButton(
                    "mapButton", level.levelNameReadable, menu.transform,
                    center + new Vector2(
                        Templates.ButtonGap.x * x,
                        Templates.ButtonGap.y * y
                    ),
                    new UnityAction[] { Press, Menus.GoBack }
                );

                button.GetComponentInChildren<Image>().sprite = level.levelSprite;
                button.GetComponentInChildren<Image>().color = Color.white;

                var colors = button.colors;

                colors.pressedColor = Color.white;
                colors.highlightedColor = Color.white;
                colors.normalColor = Color.white;
                colors.disabledColor = Color.white;

                button.colors = colors;
            }
        }

        public static Text mapInfo;
        public LoadingController loadingController;

        public List<Button> matchSettingButtons = new List<Button>();

        public void Start()
        {
            Menus.DoTheming();
            Templates.Init();
            loadingController = gameObject.AddComponent<LoadingController>().Init();

            if (HexaMod.persistentLobby.lobbySettingsChanged == null)
            {
                throw new System.Exception("HexaMod.hexaLobby.lobbySettingsChanged is null");
            }

            HexaMod.persistentLobby.lobbySettingsChanged.AddListener(delegate ()
            {
                UpdateUIForLobbyState();
            });

            HexaMod.persistentLobby.lobbySettingsChanged.AddListener(delegate ()
            {
                var oldSettings = HexaMod.persistentLobby.currentLobbySettingsEvent.oldSettings;
                var newSettings = HexaMod.persistentLobby.currentLobbySettingsEvent.newSettings;
                if (oldSettings.relay != newSettings.relay)
                {
                    VoiceChat.SetRelay(newSettings.relay);
                }

                if (VoiceChatRoomsHook.wantedRoom != null)
                {
                    VoiceChat.JoinVoiceRoom(VoiceChatRoomsHook.wantedRoom);
                    VoiceChatRoomsHook.wantedRoom = null;
                }
            });

            mapInfo = Instantiate(Menus.root.Find("Version"), Menus.root).GetComponent<Text>();
            mapInfo.name = "mapInfo";
            mapInfo.transform.localPosition = new Vector2(mapInfo.transform.localPosition.x, mapInfo.transform.localPosition.y * 0.8f);
            mapInfo.transform.SetParent(Menus.root.Find("Version"));
            mapInfo.text = "";

            { // Title Screen
                Mod.Print("edit title screen");

                Menus.root.Find("Version").GetComponent<Text>().text = HexaMod.networkManager.version;

                // booooring
                Menus.titleScreen.Find("Return To New WYD").gameObject.SetActive(false);

                // why was this disabled?
                // oh nvm it works on private lobbies 💀
                Menus.gameList.Find("JoinRandom").gameObject.SetActive(false);

                Vector2 TopLeft = new Vector2(-160f, -118f);

                Button testDad = Templates.NewButton(
                    "testDad", "Test Dad", Menus.titleScreen,
                    TopLeft + new Vector2(
                        Templates.ButtonGap.x * -1,
                        Templates.ButtonGap.y * 0
                    ),
                    new UnityAction[] { ButtonCallbacks.TestDadButton }
                );

                Button testBaby = Templates.NewButton(
                    "testBaby", "Test Baby", Menus.titleScreen,
                    TopLeft + new Vector2(
                        Templates.ButtonGap.x * -1,
                        Templates.ButtonGap.y * -1
                    ),
                    new UnityAction[] { ButtonCallbacks.TestBabyButton }
                );

                Button matchSettings = Templates.NewButton(
                    "matchSettings", "Match Settings", Menus.titleScreen,
                    TopLeft + new Vector2(
                        Templates.ButtonGap.x * -1,
                        Templates.ButtonGap.y * -2
                    ),
                    new UnityAction[] { ButtonCallbacks.MatchSettingsButton }
                );
            };
            { // Character Customization Menu
                Mod.Print("edit character customization menu");

                GameObject shirtColorInputField = Templates.NewInputField(
                    "ShirtColor", "Shirt Color (Hex)", Menus.characterCustomization,
                    new Vector2(
                        -468.4f,
                        -520f
                    ),
                    new UnityAction<string>[] { ButtonCallbacks.UpdateShirtColorVisual },
                    new UnityAction<string>[] { ButtonCallbacks.SaveShirtColor, ButtonCallbacks.UpdateShirtColorVisual }
                );

                currentShirtColorInputField = shirtColorInputField.transform.GetComponentInChildren<InputField>(true);
                currentShirtColorInputField.text = GetCurrentShirtColorHex();
                currentShirtColorInputField.characterLimit = 7;
            }
            { // Host Options
                foreach (string menuName in Menus.hostMenus)
                {
                    Transform menu = Menus.root.Find(menuName);

                    Button matchSettings = Templates.NewButton(
                        "matchSettings", "Match Settings", menu,
                        new Vector2(790f, 445.5f),
                        new UnityAction[] { ButtonCallbacks.MatchSettingsButton }
                    );

                    matchSettings.interactable = false;

                    matchSettingButtons.Add(matchSettings);
                }
            };
            { // Match Settings
                Mod.Print("make MatchSettings menu");

                GameObject menu = Menus.NewMenu("MatchSettings");
                int menuId = Menus.FindMenu(menu.name);
                Button backButton = Templates.MakeBackButton(menu.transform);
                Menus.menuController.startBtn[menuId] = backButton.gameObject;

                var changeMapButton = Templates.NewButton(
                    "changeMap", "Map", menu.transform,
                    backButton.transform.localPosition + new Vector3(
                        Templates.ButtonGap.x,
                        0,
                        0
                    ),
                    new UnityAction[] { ButtonCallbacks.ChangeMapButton }
                );

                changeMapButton.interactable = false;

                if (Levels.loadedLevels)
                {
                    changeMapButton.interactable = Levels.levels.Count > 0;
                    OnLevelsLoaded();
                }
                else
                {
                    HexaMod.mainUI.loadingController.SetTaskState("LoadingMaps", true);

                    HexaMod.asyncLevelLoader.loadCompleted.AddListener(delegate ()
                    {
                        OnLevelsLoaded();
                        changeMapButton.interactable = Levels.levels.Count > 0;
                    });
                }

                Vector2 bottomLeft = new Vector2(200f, 200f);
                float gap = 75f;

                LobbySettings ls = HexaMod.persistentLobby.lobbySettings;

                GameObject relay = Templates.NewInputField(
                    "relayServer", "Voice Chat Relay", menu.transform,
                    new Vector2(250, -60),
                    new UnityAction<string>[] { // edit
                            
                    },
                    new UnityAction<string>[] { // submit
                        delegate (string text)
                        {
                            HexaMod.persistentLobby.lobbySettings.relay = text;
                            HexaMod.persistentLobby.CommitChanges();
                        }
                    }
                );

                InputField relayField = relay.transform.GetComponentInChildren<InputField>(true);
                relayField.text = ls.relay;

                GameObject[] controls = {
                    Templates.NewControlToggle(
                        "shufflePlayers", "Shuffle Players", ls.shufflePlayers, menu.transform,
                        Vector2.zero,
                        new UnityAction<bool>[] {
                            delegate (bool value) {
                                HexaMod.persistentLobby.lobbySettings.shufflePlayers = value;
                                HexaMod.persistentLobby.CommitChanges();
                            }
                        }
                    ).gameObject,

                    Templates.NewControlToggle(
                        "disablePets", "Disable Pets", ls.disablePets, menu.transform,
                        Vector2.zero,
                        new UnityAction<bool>[] {
                            delegate (bool value) {
                                HexaMod.persistentLobby.lobbySettings.disablePets = value;
                                HexaMod.persistentLobby.CommitChanges();
                            }
                        }
                    ).gameObject,

                    Templates.NewControlToggle(
                        "doorSounds", "Door Sounds", ls.doorSounds, menu.transform,
                        Vector2.zero,
                        new UnityAction<bool>[] {
                            delegate (bool value) {
                                HexaMod.persistentLobby.lobbySettings.doorSounds = value;
                                HexaMod.persistentLobby.CommitChanges();
                            }
                        }
                    ).gameObject,

                    Templates.NewControlToggle(
                        "modernGrabbing", "Modern Grabbing", ls.modernGrabbing, menu.transform,
                        Vector2.zero,
                        new UnityAction<bool>[] {
                            delegate (bool value) {
                                HexaMod.persistentLobby.lobbySettings.modernGrabbing = value;
                                HexaMod.persistentLobby.CommitChanges();
                            }
                        }
                    ).gameObject,

                    Templates.NewControlToggle(
                        "allMustDie", "All Babies Must Die", ls.allMustDie, menu.transform,
                        Vector2.zero,
                        new UnityAction<bool>[] {
                            delegate (bool value) {
                                HexaMod.persistentLobby.lobbySettings.allMustDie = value;
                                HexaMod.persistentLobby.CommitChanges();
                            }
                        }
                    ).gameObject,

                    Templates.NewControlToggle(
                        "cheats", "Cheats", true, menu.transform,
                        Vector2.zero,
                        new UnityAction<bool>[] {
                            delegate (bool value) {
                                HexaMod.persistentLobby.lobbySettings.cheats = value;
                                HexaMod.persistentLobby.CommitChanges();
                            }
                        }
                    ).gameObject,

                    relay
                };

                for (int i = 0; i < controls.Count(); i++)
                {
                    var control = controls[i];
                    control.transform.localPosition = new Vector2(control.transform.localPosition.x, control.transform.localPosition.y) + bottomLeft + new Vector2(0f, gap * i);
                }
            }

            UpdateUIForLobbyState();
        }

        public static string GetCurrentShirtColorHex()
        {
            return PlayerPrefs.GetString("HMV2_ShirtColor", "#E76F3D");
        }

        bool wasMasterClient = false;
        public void Update()
        {
            if (PhotonNetwork.isMasterClient != wasMasterClient)
            {
                wasMasterClient = PhotonNetwork.isMasterClient;
                UpdateUIForLobbyState();
            }

            if (loadingController.GetTaskState("RoomCreate"))
            {
                if (PhotonNetwork.room != null)
                {
                    loadingController.SetTaskState("RoomCreate", false);

                    if (VoiceChatRoomsHook.wantedRoom != null)
                    {
                        VoiceChat.JoinVoiceRoom(VoiceChatRoomsHook.wantedRoom);
                        VoiceChatRoomsHook.wantedRoom = null;
                    }
                }
            }

            if (loadingController.GetTaskState("RoomJoin"))
            {
                if (PhotonNetwork.room != null)
                {
                    loadingController.SetTaskState("RoomJoin", false);
                }
            }

            if (loadingController.GetTaskState("LobbyJoin"))
            {
                if (PhotonNetwork.insideLobby && PhotonNetwork.lobby.Name == HexaMod.networkManager.curLobby)
                {
                    loadingController.SetTaskState("LobbyJoin", false);
                }
            }

            if (loadingController.GetTaskState("PhotonConnect"))
            {
                if (PhotonNetwork.connectedAndReady)
                {
                    loadingController.SetTaskState("PhotonConnect", false);
                }
            }

            if (loadingController.GetTaskState("RoomLookForOrCreateTag"))
            {
                if (PhotonNetwork.inRoom && PhotonNetwork.room != null && PhotonNetwork.room.Name.StartsWith(HexaMod.networkManager.curTag))
                {
                    loadingController.SetTaskState("RoomLookForOrCreateTag", false);
                }
            }
        }

        private static string lastKnownGoodInputFieldText = GetCurrentShirtColorHex();
    }
}
