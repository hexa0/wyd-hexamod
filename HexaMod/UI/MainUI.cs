﻿using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using HexaMod.UI.Util;
using HexaMod.Util;
using System.Linq;
using HexaMod.ScriptableObjects;
using System.Collections.Generic;
using HexaMod.Voice;
using static HexaMod.UI.Util.Menu.Menus;
using HexaMod.UI.Class;
using HexaMod.SerializableObjects;
using UnityEngine.SceneManagement;

namespace HexaMod.UI
{
	public class MainUI : MonoBehaviour
	{
		static CharacterModelSwapper dadModelSwapper;
		static CharacterModelSwapper babyModelSwapper;

		internal static class ButtonCallbacks
		{
			public static void MatchSettingsButton()
			{
				title.menuController.ChangeToMenu(title.GetMenuId("MatchSettings"));
			}
			public static void ChangeMapButton()
			{
				title.menuController.ChangeToMenu(title.GetMenuId("ChangeMap"));
			}
			public static void TestDadButton()
			{
				HexaMod.MakeTestGame(true);
			}

			public static void TestBabyButton()
			{
				HexaMod.MakeTestGame(false);
			}

			public static void SetShirtColor(Color color, string hex)
			{
				dadModelSwapper.SetShirtColor(color);
				babyModelSwapper.SetShirtColor(color);
			}

			public static void SaveShirtColor(Color color, string hex)
			{
				PlayerPrefs.SetString("HMV2_ShirtColor", hex);
			}

			public static void SetSkinColor(Color color, string hex)
			{
				dadModelSwapper.SetSkinColor(color);
				babyModelSwapper.SetSkinColor(color);
			}

			public static void SaveSkinColor(Color color, string hex)
			{
				PlayerPrefs.SetString("HMV2_SkinColor", hex);
			}

			public static void SetDadModel(string modelName)
			{
				dadModelSwapper.SetCharacterModel(modelName);
			}

			public static void SaveDadModel(string modelName)
			{
				PlayerPrefs.SetString("HMV2_DadCharacterModel", modelName);
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
			ModLevel foundLevel = Assets.titleLevel;

			foreach (var level in Assets.levels)
			{
				if (level.levelPrefab.name == HexaMod.persistentLobby.lobbySettings.mapName)
				{
					foundLevel = level;
					break;
				}
			}

			if (foundLevel)
			{
				var gameModesCustom = title.FindMenu("GameListMenu").Find("GameCreator").Find("GameModes");

				gameModesCustom.Find("Original").Find("CreateGame").GetComponent<Button>().interactable = foundLevel.regular;
				gameModesCustom.Find("Family Gathering").Find("CreateGame").GetComponent<Button>().interactable = foundLevel.familyGathering;
				gameModesCustom.Find("The Hungry Games").Find("CreateGame").GetComponent<Button>().interactable = foundLevel.hungryGames;
				gameModesCustom.Find("The Great Dadlympics").Find("CreateGame").GetComponent<Button>().interactable = foundLevel.dadlympics;
				gameModesCustom.Find("Daddy's Nightmare").Find("CreateGame").GetComponent<Button>().interactable = foundLevel.daddysNightmare;

				title.FindMenu("Family Gathering-Host").Find("Start").GetComponent<Button>().interactable = foundLevel.familyGathering;
				title.FindMenu("HungryGames").Find("Start").GetComponent<Button>().interactable = foundLevel.hungryGames;
				title.FindMenu("Dadlympics").Find("Start").GetComponent<Button>().interactable = foundLevel.dadlympics;
				title.FindMenu("DaddysNightmare").Find("Start").GetComponent<Button>().interactable = foundLevel.daddysNightmare;

				mapInfo.text = $"{foundLevel.levelNameReadable}\n{foundLevel.levelDescriptionReadable}";

				foreach (WYDTextButton matchSettings in matchSettingButtons)
				{
					matchSettings.button.interactable = PhotonNetwork.isMasterClient || !PhotonNetwork.inRoom;
				}
			}
		}

		public void OnLevelsLoaded()
		{
			HexaMod.mainUI.loadingController.SetTaskState("LoadingMaps", false);

			UpdateUIForLobbyState();

			// Map Menu

			Mod.Print("make ChangeMap menu");

			GameObject menu = title.NewMenu("ChangeMap");
			int menuId = title.GetMenuId(menu.name);
			WYDTextButton backButton = WYDTextButton.MakeBackButton(title, menu.transform);
			title.menuController.startBtn[menuId] = backButton.gameObject;

			Vector2 center = new Vector2(1920f / 2f, 1080f / 2f);

			//void Reset()
			//{
			//	ButtonCallbacks.ChangeLevel(Assets.titleName);
			//}

			//new WYDTextButton(
			//	"resetMapToDefault", "Default", menu.transform,
			//	backButton.gameObject.transform.localPosition + new Vector3(
			//		WYDTextButton.gap.x,
			//		0,
			//		0
			//	),
			//	new UnityAction[] { Reset, title.GoBack }
			//);

			float height = Mathf.Clamp(Assets.levels.Count() - 1, 0f, 4f);
			float width = Mathf.Floor((Assets.levels.Count() - 1) / 5f);

			for (int i = 0; i < Assets.levels.Count(); i++)
			{
				ModLevel level = Assets.levels[i];

				float y = (-(i % 5) + (height / 2f)) + 0.5f;
				float x = Mathf.Floor(i / 5f) - (width / 2f);

				void Press()
				{
					ButtonCallbacks.ChangeLevel(level.levelPrefab.name);
				}

				WYDTextButton mapButton = new WYDTextButton(
					"mapButton", level.levelNameReadable, menu.transform,
					center + new Vector2(
						WYDTextButton.gap.x * x,
						WYDTextButton.gap.y * y
					),
					new UnityAction[] { Press, title.GoBack }
				);

				mapButton.image.sprite = level.levelSprite;
				mapButton.image.color = Color.white;

				var colors = mapButton.button.colors;

				colors.pressedColor = Color.white;
				colors.highlightedColor = Color.white;
				colors.normalColor = Color.white;
				colors.disabledColor = Color.white;

				mapButton.button.colors = colors;
			}
		}

		public static Text mapInfo;
		public LoadingController loadingController;

		public List<WYDTextButton> matchSettingButtons = new List<WYDTextButton>();

		public void Init()
		{
			if (PlayerPrefs.GetInt("HMV2_DoUITheme", 1) == 1)
			{
				UITheming.Init();
			}
			UITemplates.Init();
			loadingController = gameObject.AddComponent<LoadingController>().Init();

			if (HexaMod.persistentLobby.lobbySettingsChanged == null)
			{
				throw new System.Exception("HexaMod.hexaLobby.lobbySettingsChanged is null");
			}

			HexaMod.persistentLobby.lobbySettingsChanged.AddListener(delegate ()
			{
				if (Assets.StaticAssets.didCache)
				{
					Assets.AttemptToLoadCurrentLevel();
				}

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

			mapInfo = Instantiate(title.root.Find("Version"), title.root).GetComponent<Text>();
			mapInfo.name = "mapInfo";
			mapInfo.transform.localPosition = new Vector2(mapInfo.transform.localPosition.x, mapInfo.transform.localPosition.y * 0.8f);
			mapInfo.transform.SetParent(title.root.Find("Version"));
			mapInfo.text = "";

			{ // HexaMod Options Menu
				void MakeButton(Button originalButton, MenuUtil menu)
				{
					originalButton.gameObject.SetActive(false);
					WYDTextButton hexaModOptions = new WYDTextButton(
						"hexaModOptions", "HexaMod", originalButton,
						new UnityAction[]
						{
							delegate ()
							{
								menu.menuController.ChangeToMenu(menu.GetMenuId("HexaModOptions"));
							}
						}
					);
					hexaModOptions.label.fontSize = (int)WYDTextButton.FontSizes.Small;
				}

				HexaMod.persistentInstance.GetComponent<TabOutMuteBehavior>().tabOutMuteEnabled = PlayerPrefs.GetInt("HMV2_TabOutMute", 1) == 1;

				void MakeMenu(GameObject menu, MenuUtil menuUtil)
				{
					menuUtil.menuController.startBtn[menuUtil.GetMenuId(menu.name)] = WYDTextButton.MakeBackButton(menuUtil, menu.transform).gameObject;

					Vector2 bottomLeft = new Vector2(0f, 200f);
					float gap = 15f;

					LobbySettings ls = HexaMod.persistentLobby.lobbySettings;

					VoiceChat.SetDenoiseEnabled(PlayerPrefs.GetInt("HMV2_UseRnNoise", 0) == 1);

					var devices = VoiceChat.GetDevices();
					WYDSwitchOption<VoiceChat.MicrophoneDevice>[] deviceOptions = new WYDSwitchOption<VoiceChat.MicrophoneDevice>[devices.Length];

					for (int i = 0; i < devices.Length; i++)
					{
						deviceOptions[i] = new WYDSwitchOption<VoiceChat.MicrophoneDevice>()
						{
							name = $"({i + 1}/{devices.Length}) : {devices[i].capabilities.ProductName}",
							value = devices[i]
						};
					}

					WYDUIElement[] options = {
						// Audio

						new WYDBooleanControl(
							"micRnNoise", "Microphone Denoising (RNNoise)", PlayerPrefs.GetInt("HMV2_UseRnNoise", 0) == 1, menu.transform,
							new Vector2(200f, 0f),
							new UnityAction<bool>[] {
								delegate (bool value) {
									PlayerPrefs.SetInt("HMV2_UseRnNoise", value ? 1 : 0);
									VoiceChat.SetDenoiseEnabled(value);
								}
							}
						),

						new WYDSwitchInput<VoiceChat.MicrophoneDevice>(
							"microphoneDevice", "", Mathf.Min(PlayerPrefs.GetInt("HMV2_MicrophoneDevice", 0), deviceOptions.Length - 1), deviceOptions,
							menu.transform, new Vector2(45f, 0f),
							new UnityAction<WYDSwitchOption<VoiceChat.MicrophoneDevice>>[] {
								delegate (WYDSwitchOption<VoiceChat.MicrophoneDevice> option) {
									PlayerPrefs.SetInt("HMV2_MicrophoneDevice", option.value.deviceId);
									VoiceChat.SetDevice(option.value);

								}
							}
						),

						new WYDMicrophoneIndicator(
							"microphoneVolume",
							menu.transform, new Vector2(45f, 0f)
						),

						new WYDBooleanControl(
							"tabOutMute", "Mute While Tabbed Out", PlayerPrefs.GetInt("HMV2_TabOutMute", 1) == 1, menu.transform,
							new Vector2(200f, 0f),
							new UnityAction<bool>[] {
								delegate (bool value) {
									PlayerPrefs.SetInt("HMV2_TabOutMute", value ? 1 : 0);
									HexaMod.persistentInstance.GetComponent<TabOutMuteBehavior>().tabOutMuteEnabled = value;
								}
							}
						),
						
						// UI

						new WYDBooleanControl(
							"uiRefresh", "Refreshed UI Colors (Requires Scene Reload)", PlayerPrefs.GetInt("HMV2_DoUITheme", 1) == 1, menu.transform,
							new Vector2(200f, 0f),
							new UnityAction<bool>[] {
								delegate (bool value) {
									PlayerPrefs.SetInt("HMV2_DoUITheme", value ? 1 : 0);
								}
							}
						),

					};

					for (int i = 0; i < options.Count(); i++)
					{
						var control = options[i];

						control.rectTransform.localPosition = bottomLeft + new Vector2(control.rectTransform.localPosition.x, gap);
						bottomLeft.y = control.rectTransform.localPosition.y + control.rectTransform.sizeDelta.y;
					}
				}

				MakeButton(title.FindMenu("OptionsMenu").Find("SplitScreenOptions").GetComponent<Button>(), title);
				MakeButton(inGame.FindMenu("OptionsMenu (1)").Find("SplitScreenOptions").GetComponent<Button>(), inGame);
				MakeMenu(title.NewMenu("HexaModOptions"), title);
				MakeMenu(inGame.NewMenu("HexaModOptions"), inGame);
			}
			{ // Return To Lobby Override
				if (PhotonNetwork.isMasterClient)
				{
					WYDTextButton hexaModOptions = new WYDTextButton(
					"returnToLobby", "Return To\nLobby", inGame.FindMenu("OptionsMenu (1)").Find("ReturnToMenu").GetComponent<Button>(),
					new UnityAction[]
					{
						() => {
							if (PhotonNetwork.room != null)
							{
								HexaMod.hexaLobby.ReturnToLobby();
							}
							else
							{
								// something is very wrong??
								// just try and reload the scene??
								SceneManager.LoadScene(1);
							}
						}
					}
				);
				}
			}
			{ // Title Screen
				Mod.Print("edit title screen");

				title.root.Find("Version").GetComponent<Text>().text = HexaMod.networkManager.version;

				// booooring
				title.FindMenu("SplashMenu").Find("Return To New WYD").gameObject.SetActive(false);

				// why was this disabled?
				// oh nvm it works on private lobbies 💀
				title.FindMenu("GameListMenu").Find("JoinRandom").gameObject.SetActive(false);

				// we replace all of the match settings with our own menu so hide the originals
				title.FindMenu("Family Gathering-Host").Find("AlternateCharacters (2)").gameObject.SetActive(false);
				title.FindMenu("Family Gathering-Host").Find("SetSpectate").gameObject.SetActive(false);
				title.FindMenu("HungryGames").Find("SetSpectate (1)").gameObject.SetActive(false);

				Vector2 TopLeft = new Vector2(-160f, -118f);

				WYDTextButton testDad = new WYDTextButton(
					"testDad", "Test\nDad", title.FindMenu("SplashMenu"),
					TopLeft + new Vector2(
						WYDTextButton.gap.x * -1,
						WYDTextButton.gap.y * 0
					),
					new UnityAction[] { ButtonCallbacks.TestDadButton }
				);

				WYDTextButton testBaby = new WYDTextButton(
					"testBaby", "Test\nBaby", title.FindMenu("SplashMenu"),
					TopLeft + new Vector2(
						WYDTextButton.gap.x * -1,
						WYDTextButton.gap.y * -1
					),
					new UnityAction[] { ButtonCallbacks.TestBabyButton }
				);

				WYDTextButton matchSettings = new WYDTextButton(
					"matchSettings", "Match\nSettings", title.FindMenu("SplashMenu"),
					TopLeft + new Vector2(
						WYDTextButton.gap.x * -1,
						WYDTextButton.gap.y * -2
					),
					new UnityAction[] { ButtonCallbacks.MatchSettingsButton }
				);
			}
			{ // Character Customization Menu
				Mod.Print("edit character customization menu");

				GameObject characterPreviewCanvas = GameObject.Find("BackendObjects").transform.Find("MenuCamera").Find("Camera").Find("Canvas").gameObject;
				dadModelSwapper = characterPreviewCanvas.transform.Find("Dad").gameObject.AddComponent<CharacterModelSwapper>();
				babyModelSwapper = characterPreviewCanvas.transform.Find("Baby001").gameObject.AddComponent<CharacterModelSwapper>();
				dadModelSwapper.initModel = PlayerPrefs.GetString("HMV2_DadCharacterModel", "default");
				dadModelSwapper.initShirtColor = HexToColor.GetColorFromHex(GetCurrentShirtColorHex());
				dadModelSwapper.initSkinColor = HexToColor.GetColorFromHex(GetCurrentSkinColorHex());
				babyModelSwapper.initModel = PlayerPrefs.GetString("HMV2_BabyCharacterModel", "default");
				babyModelSwapper.initShirtColor = HexToColor.GetColorFromHex(GetCurrentShirtColorHex());
				babyModelSwapper.initSkinColor = HexToColor.GetColorFromHex(GetCurrentSkinColorHex());

				Transform characterCustomizationMenu = title.FindMenu("CharacterCustomizationMenu");

				Vector2 bottomLeft = new Vector2(720f, -350f);
				float gap = 15f;

				WYDSwitchOption<string>[] dadModelOptions = new WYDSwitchOption<string>[Assets.dadCharacterModels.Count + 1];
				WYDSwitchOption<string>[] babyModelOptions = new WYDSwitchOption<string>[Assets.babyCharacterModels.Count + 1];

				dadModelOptions[0] = new WYDSwitchOption<string>
				{
					name = "Dad (Original)",
					value = "default"
				};

				babyModelOptions[0] = new WYDSwitchOption<string>
				{
					name = "Baby (Original)",
					value = "default"
				};

				int dadCharacterDefault = 0;
				int babyCharacterDefault = 0;

				for (int i = 0; i < Assets.dadCharacterModels.Count; i++)
				{
					dadModelOptions[i + 1] = new WYDSwitchOption<string>
					{
						name = Assets.dadCharacterModels[i].modelNameReadable,
						value = Assets.dadCharacterModels[i].modelNameReadable
					};

					if (Assets.dadCharacterModels[i].modelNameReadable == dadModelSwapper.initModel)
					{
						dadCharacterDefault = i + 1;
					}
				}

				for (int i = 0; i < Assets.babyCharacterModels.Count; i++)
				{
					babyModelOptions[i + 1] = new WYDSwitchOption<string>
					{
						name = Assets.babyCharacterModels[i].modelNameReadable,
						value = Assets.babyCharacterModels[i].modelNameReadable
					};

					if (Assets.babyCharacterModels[i].modelNameReadable == babyModelSwapper.initModel)
					{
						babyCharacterDefault = i + 1;
					}
				}

				WYDUIElement[] options = {
					new WYDHexColorInputField(
						"ShirtColor", "Shirt Color (Hex)", GetCurrentShirtColorHex(), characterCustomizationMenu,
						new Vector2(
							-468.4f,
							0f
						),
						new UnityAction<Color, string>[] { ButtonCallbacks.SetShirtColor },
						new UnityAction<Color, string>[] { ButtonCallbacks.SaveShirtColor, ButtonCallbacks.SetShirtColor }
					),
					new WYDHexColorInputField(
						"ShirtColor", "Skin Color (Hex)", GetCurrentSkinColorHex(), characterCustomizationMenu,
						new Vector2(
							-468.4f,
							0f
						),
						new UnityAction<Color, string>[] { ButtonCallbacks.SetSkinColor },
						new UnityAction<Color, string>[] { ButtonCallbacks.SaveSkinColor, ButtonCallbacks.SetSkinColor }
					),
					new WYDSwitchInput<string>(
						"DadCharacterModel", "", dadCharacterDefault,
						dadModelOptions,
						characterCustomizationMenu,
						new Vector2(
							-880,
							0f
						),
						new UnityAction<WYDSwitchOption<string>>[] { (WYDSwitchOption<string> option) => {
							ButtonCallbacks.SetDadModel(option.value);
							ButtonCallbacks.SaveDadModel(option.value);

						}}
					),
					new WYDSwitchInput<string>(
						"BabyCharacterModel", "", babyCharacterDefault,
						babyModelOptions,
						characterCustomizationMenu,
						new Vector2(
							-880,
							0f
						),
						new UnityAction<WYDSwitchOption<string>>[] { (WYDSwitchOption<string> option) => {
							// ButtonCallbacks.SetBabyModel(option.value);
							// ButtonCallbacks.SaveBabyModel(option.value);
						}}
					),
				};

				for (int i = 0; i < options.Count(); i++)
				{
					var control = options[i];

					control.rectTransform.localPosition = bottomLeft + new Vector2(control.rectTransform.localPosition.x, gap);
					bottomLeft.y = control.rectTransform.localPosition.y + control.rectTransform.sizeDelta.y;
				}
			}
			{ // Host Options
				foreach (string menuName in Menu.hostMenus)
				{
					Transform menu = title.FindMenu(menuName);

					WYDTextButton matchSettings = new WYDTextButton(
						"matchSettings", "Match\nSettings", menu,
						new Vector2(790f, 445.5f),
						new UnityAction[] { ButtonCallbacks.MatchSettingsButton }
					);

					matchSettings.button.interactable = false;

					matchSettingButtons.Add(matchSettings);
				}
			}
			{ // Match Settings
				Mod.Print("make MatchSettings menu");

				GameObject menu = title.NewMenu("MatchSettings");
				int menuId = title.GetMenuId(menu.name);
				WYDTextButton backButton = WYDTextButton.MakeBackButton(title, menu.transform);
				title.menuController.startBtn[menuId] = backButton.gameObject;

				WYDTextButton changeMapButton = new WYDTextButton(
					"changeMap", "Map", menu.transform,
					backButton.gameObject.transform.localPosition + new Vector3(
						WYDTextButton.gap.x,
						0,
						0
					),
					new UnityAction[] { ButtonCallbacks.ChangeMapButton }
				);

				changeMapButton.button.interactable = Assets.levels.Count > 0;
				OnLevelsLoaded();

				Vector2 bottomLeft = new Vector2(0f, 200f);
				float gap = 15f;

				LobbySettings ls = HexaMod.persistentLobby.lobbySettings;

				WYDUIElement[] options = {
					new WYDSwitchInput<ShufflePlayersMode>(
						"shufflePlayers", "", (int)ls.shufflePlayers, LobbySettings.shuffleOptions,
						menu.transform, new Vector2(45f, 0f),
						new UnityAction<WYDSwitchOption<ShufflePlayersMode>>[] {
							delegate (WYDSwitchOption<ShufflePlayersMode> option) {
								HexaMod.persistentLobby.lobbySettings.shufflePlayers = option.value;
								HexaMod.persistentLobby.CommitChanges();
							}
						}
					),

					// TODO: move this into a map specific settings menu
					new WYDBooleanControl(
						"disablePets", "Disable House Pets", ls.disablePets, menu.transform,
						new Vector2(200f, 0f),
						new UnityAction<bool>[] {
							delegate (bool value) {
								HexaMod.persistentLobby.lobbySettings.disablePets = value;
								HexaMod.persistentLobby.CommitChanges();
							}
						}
					),

					new WYDBooleanControl(
						"doorSounds", "Door Interact Sounds", ls.doorSounds, menu.transform,
						new Vector2(200f, 0f),
						new UnityAction<bool>[] {
							delegate (bool value) {
								HexaMod.persistentLobby.lobbySettings.doorSounds = value;
								HexaMod.persistentLobby.CommitChanges();
							}
						}
					),

					new WYDBooleanControl(
						"ventSounds", "Vent Interact Sounds", ls.ventSounds, menu.transform,
						new Vector2(200f, 0f),
						new UnityAction<bool>[] {
							delegate (bool value) {
								HexaMod.persistentLobby.lobbySettings.ventSounds = value;
								HexaMod.persistentLobby.CommitChanges();
							}
						}
					),

					new WYDBooleanControl(
						"modernGrabbing", "Modern Grabbing", ls.modernGrabbing, menu.transform,
						new Vector2(200f, 0f),
						new UnityAction<bool>[] {
							delegate (bool value) {
								HexaMod.persistentLobby.lobbySettings.modernGrabbing = value;
								HexaMod.persistentLobby.CommitChanges();
							}
						}
					),

					new WYDBooleanControl(
						"allMustDie", "All Babies Must Die", ls.allMustDie, menu.transform,
						new Vector2(200f, 0f),
						new UnityAction<bool>[] {
							delegate (bool value) {
								HexaMod.persistentLobby.lobbySettings.allMustDie = value;
								HexaMod.persistentLobby.CommitChanges();
							}
						}
					),

					new WYDBooleanControl(
						"spectatingAllowed", "Spectating Allowed", ls.allowSpectating, menu.transform,
						new Vector2(200f, 0f),
						new UnityAction<bool>[] {
							delegate (bool value) {
								HexaMod.persistentLobby.lobbySettings.allowSpectating = value;
								HexaMod.persistentLobby.CommitChanges();
							}
						}
					),

					new WYDBooleanControl(
						"cheats", "Cheats", ls.cheats, menu.transform,
						new Vector2(200f, 0f),
						new UnityAction<bool>[] {
							delegate (bool value) {
								HexaMod.persistentLobby.lobbySettings.cheats = value;
								HexaMod.persistentLobby.CommitChanges();
							}
						}
					),

					new WYDTextInputField(
						"relayServer", "Voice Chat Relay", ls.relay, menu.transform,
						new Vector2(455f, 0f),
						new UnityAction<string>[] { // edit
							
						},
						new UnityAction<string>[] { // submit
							delegate (string text)
							{
								HexaMod.persistentLobby.lobbySettings.relay = text;
								HexaMod.persistentLobby.CommitChanges();
							}
						}
					),
				};

				for (int i = 0; i < options.Count(); i++)
				{
					var control = options[i];

					control.rectTransform.localPosition = bottomLeft + new Vector2(control.rectTransform.localPosition.x, gap);
					bottomLeft.y = control.rectTransform.localPosition.y + control.rectTransform.sizeDelta.y;
				}
			}

			UpdateUIForLobbyState();
		}

		public static string GetCurrentShirtColorHex()
		{
			return PlayerPrefs.GetString("HMV2_ShirtColor", "#E76F3D");
		}

		public static string GetCurrentSkinColorHex()
		{
			// todo: make this the actual default color code
			return PlayerPrefs.GetString("HMV2_SkinColor", "#CC9485");
		}

		public static string GetCurrentDadModel()
		{
			// todo: make this the actual default color code
			return PlayerPrefs.GetString("HMV2_DadCharacterModel", "default");
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
	}
}
