using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using HexaMod.UI.Util;
using HexaMod.Util;
using System.Linq;
using HexaMod.ScriptableObjects;
using HexaMod.Voice;
using static HexaMod.UI.Util.Menu.Menus;
using HexaMod.UI.Elements;
using HexaMod.UI.Elements.Extended;
using HexaMod.SerializableObjects;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;

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

			public static void SetBabyModel(string modelName)
			{
				babyModelSwapper.SetCharacterModel(modelName);
			}

			public static void SaveBabyModel(string modelName)
			{
				PlayerPrefs.SetString("HMV2_BabyCharacterModel", modelName);
			}

			public static void SetDadShirt(string shirtName)
			{
				dadModelSwapper.SetShirt(shirtName);
			}

			public static void SaveDadModel(string modelName)
			{
				PlayerPrefs.SetString("HMV2_DadCharacterModel", modelName);
			}

			public static void SaveDadShirt(string materialName)
			{
				PlayerPrefs.SetString("HMV2_DadShirtMaterial", materialName);
			}

			public static void ChangeLevel(string mapName)
			{
				Mod.Print($"change to {mapName}");
				PlayerPrefs.SetString("HMV2_CustomMap", mapName);

				HexaMod.textChat.SendServerMessage($"Map changed to {mapName}.");

				HexaMod.persistentLobby.lobbySettings.mapName = mapName;
				HexaMod.persistentLobby.CommitChanges();
			}
		}

		public void UpdateUIForLobbyState()
		{
			ModLevel foundLevel = Assets.defaultLevel;

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

			float height = Mathf.Clamp(Assets.levels.Count() - 1, 0f, 4f);
			float width = Mathf.Floor((Assets.levels.Count() - 1) / 5f);

			for (int i = 0; i < Assets.levels.Count(); i++)
			{
				ModLevel level = Assets.levels[i];

				float x = Mathf.Floor(i / 5f) - (width / 2f);
				float y = -(i % 5) + (height / 2f) + 0.5f;

				new WYDMapButton(level)
					.SetParent(menu.transform)
					.SetPosition(center + new Vector2(
						WYDTextButton.gap.x * x,
						WYDTextButton.gap.y * y
					))
					.AddListener(() => { ButtonCallbacks.ChangeLevel(level.levelPrefab.name); });
			}
		}

		public static Text mapInfo;
		public LoadingController loadingController;

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

			mapInfo = Instantiate(title.root.Find("Version"), title.root).GetComponent<Text>();
			mapInfo.name = "mapInfo";
			mapInfo.transform.localPosition = new Vector2(mapInfo.transform.localPosition.x, mapInfo.transform.localPosition.y * 0.8f);
			mapInfo.transform.SetParent(title.root.Find("Version"));
			mapInfo.text = "";

			{ // HexaMod Options Menu
				void MakeButton(Button originalButton, MenuUtil menu)
				{
					new WYDTextButton()
						.SetName("hexaModOptions")
						.SetParent(originalButton.transform.parent)
						.SetPosition(new Vector2(originalButton.transform.localPosition.x, originalButton.transform.localPosition.y) + new Vector2(WYDTextButton.gap.x * 0.95f, 0f))
						.SetText("Hexa Mod")
						.SetFontSize(WYDTextButton.FontSizes.Small)
						.AddListener(() =>
						{
							menu.menuController.ChangeToMenu(menu.GetMenuId("HexaModOptions"));
						});

					new WYDMatchSettingsButton()
						.SetParent(originalButton.transform.parent)
						.SetPosition(new Vector2(originalButton.transform.localPosition.x, originalButton.transform.localPosition.y) + new Vector2(WYDTextButton.gap.x * -0.95f, 0f))
						.AddListener(() =>
						{
							menu.menuController.ChangeToMenu(menu.GetMenuId("MatchSettings"));
						});
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

						new WYDBooleanControl()
							.SetName("micRnNoise")
							.SetParent(menu.transform)
							.SetPosition(200f, 0f)
							.SetText("Microphone Denoising (RNNoise)")
							.LinkToPreference("HMV2_UseRnNoise")
							.AddListener((bool value) =>
							{
								VoiceChat.SetDenoiseEnabled(value);
							}),

						new WYDSwitchInput<VoiceChat.MicrophoneDevice>()
							.SetName("microphoneDevice")
							.SetParent(menu.transform)
							.SetPosition(45f, 0f)
							.SetText("")
							.AddOptions(deviceOptions)
							.LinkToPreference("HMV2_MicrophoneDevice")
							.AddListener((WYDSwitchOption<VoiceChat.MicrophoneDevice> option) =>
							{
								VoiceChat.SetDevice(option.value);
							}),

						new WYDMicrophoneIndicator()
							.SetName("microphoneVolume")
							.SetParent(menu.transform)
							.SetPosition(45f, 0f),

						new WYDBooleanControl()
							.SetName("tabOutMute")
							.SetParent(menu.transform)
							.SetPosition(200f, 0f)
							.SetText("Mute While Tabbed Out")
							.LinkToPreference("HMV2_TabOutMute")
							.AddListener((bool value) =>
							{
								HexaMod.persistentInstance.GetComponent<TabOutMuteBehavior>().tabOutMuteEnabled = value;
							}),
						
						// UI

						new WYDBooleanControl()
							.SetName("uiRefresh")
							.SetParent(menu.transform)
							.SetPosition(200f, 0f)
							.SetText("Refreshed UI Colors (Requires Scene Reload)")
							.LinkToPreference("HMV2_DoUITheme"),

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
						new UnityAction[] {
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

				WYDTextButton backButton = new WYDTextButton(
					"back", "Back", inGame.FindMenu("OptionsMenu (1)").Find("Back").GetComponent<Button>(),
					new UnityAction[] {
						() => {
							FirstPersonController firstPersonController = HexaMod.networkManager.playerObj.GetComponent<FirstPersonController>();
							InGameMenuHelper inGameMenuHelper = HexaMod.networkManager.playerObj.GetComponentInChildren<InGameMenuHelper>();
							inGameMenuHelper.menuOn = false;
							inGame.menuController.DeactivateAll();
							firstPersonController.haltInput = false;
							inGameMenuHelper.TurnOnPlayer();
						}
					}
				);
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

				WYDTextButton testDad = new WYDTextButton()
					.SetName("testDad")
					.SetTextAuto("Test\nDad")
					.SetParent(title.FindMenu("SplashMenu"))
					.SetPosition(TopLeft + new Vector2(
						WYDTextButton.gap.x * -1,
						WYDTextButton.gap.y * 0
					))
					.AddListener(ButtonCallbacks.TestDadButton);

				WYDTextButton testBaby = new WYDTextButton()
					.SetName("testBaby")
					.SetTextAuto("Test\nBaby")
					.SetParent(title.FindMenu("SplashMenu"))
					.SetPosition(TopLeft + new Vector2(
						WYDTextButton.gap.x * -1,
						WYDTextButton.gap.y * -1
					))
					.AddListener(ButtonCallbacks.TestBabyButton);

				new WYDMatchSettingsButton()
					.SetParent(title.FindMenu("SplashMenu"))
					.SetPosition(TopLeft + new Vector2(
							WYDTextButton.gap.x * -1,
							WYDTextButton.gap.y * -2
					))
					.AddListener(ButtonCallbacks.MatchSettingsButton);

			}
			{ // Character Customization Menu
				Mod.Print("edit character customization menu");

				GameObject characterPreviewCanvas = GameObject.Find("BackendObjects").Find("MenuCamera").Find("Camera").Find("Canvas");
				dadModelSwapper = characterPreviewCanvas.Find("Dad").AddComponent<CharacterModelSwapper>();
				babyModelSwapper = characterPreviewCanvas.Find("Baby001").AddComponent<CharacterModelSwapper>();
				dadModelSwapper.initModel = PlayerPrefs.GetString("HMV2_DadCharacterModel", "default");
				dadModelSwapper.initShirt = PlayerPrefs.GetString("HMV2_DadShirtMaterial", "default");
				dadModelSwapper.initShirtColor = new Color().FromHex(GetCurrentShirtColorHex());
				dadModelSwapper.initSkinColor = new Color().FromHex(GetCurrentSkinColorHex());
				babyModelSwapper.initModel = PlayerPrefs.GetString("HMV2_BabyCharacterModel", "default");

				Transform characterCustomizationMenu = title.FindMenu("CharacterCustomizationMenu");

				Vector2 bottomLeft = new Vector2(720f, -350f);
				float gap = 15f;

				WYDSwitchOption<string>[] dadModelOptions = new WYDSwitchOption<string>[Assets.dadCharacterModels.Count + 1];
				WYDSwitchOption<Material>[] dadShirtOptions = new WYDSwitchOption<Material>[Assets.shirts.Count + 1];

				WYDSwitchOption<string>[] babyModelOptions = new WYDSwitchOption<string>[Assets.babyCharacterModels.Count + 1];

				dadModelOptions[0] = new WYDSwitchOption<string>
				{
					name = "Dad (Original)",
					value = "default"
				};

				dadShirtOptions[0] = new WYDSwitchOption<Material>
				{
					name = "Solid Color Shirt",
					value = Assets.defaultShirt.shirtMaterial
				};


				babyModelOptions[0] = new WYDSwitchOption<string>
				{
					name = "Baby (Original)",
					value = "default"
				};

				int dadCharacterDefault = 0;
				int dadShirtDefault = 0;

				int babyCharacterDefault = 0;

				for (int i = 0; i < Assets.dadCharacterModels.Count; i++)
				{
					dadModelOptions[i + 1] = new WYDSwitchOption<string>
					{
						name = Assets.dadCharacterModels[i].modelNameReadable,
						value = Assets.dadCharacterModels[i].name
					};

					if (Assets.dadCharacterModels[i].name == dadModelSwapper.initModel)
					{
						dadCharacterDefault = i + 1;
					}
				}

				for (int i = 0; i < Assets.shirts.Count; i++)
				{
					dadShirtOptions[i + 1] = new WYDSwitchOption<Material>
					{
						name = Assets.shirts[i].name,
						value = Assets.shirts[i].shirtMaterial
					};

					if (Assets.shirts[i].name == dadModelSwapper.initShirt)
					{
						dadShirtDefault = i + 1;
					}
				}


				for (int i = 0; i < Assets.babyCharacterModels.Count; i++)
				{
					babyModelOptions[i + 1] = new WYDSwitchOption<string>
					{
						name = Assets.babyCharacterModels[i].modelNameReadable,
						value = Assets.babyCharacterModels[i].name
					};

					if (Assets.babyCharacterModels[i].name == babyModelSwapper.initModel)
					{
						babyCharacterDefault = i + 1;
					}
				}

				WYDUIElement[] options = {
					new WYDHexColorInputField()
						.SetName("shirtColor")
						.SetParent(characterCustomizationMenu)
						.SetPosition(-468.4f, 0f)
						.AddChangedListener(ButtonCallbacks.SetShirtColor)
						.AddSubmitListener(ButtonCallbacks.SaveShirtColor)
						.AddSubmitListener(ButtonCallbacks.SetShirtColor)
						.SetText("Shirt Color (Hex)")
						.SetFieldText(GetCurrentShirtColorHex()),

					new WYDHexColorInputField()
						.SetName("skinColor")
						.SetParent(characterCustomizationMenu)
						.SetPosition(-468.4f, 0f)
						.AddChangedListener(ButtonCallbacks.SetSkinColor)
						.AddSubmitListener(ButtonCallbacks.SaveSkinColor)
						.AddSubmitListener(ButtonCallbacks.SetSkinColor)
						.SetText("Skin Color (Hex)")
						.SetFieldText(GetCurrentSkinColorHex()),

					new WYDSwitchInput<Material>()
						.SetName("dadShirtMaterial")
						.SetParent(characterCustomizationMenu)
						.SetPosition(-880f, 0f)
						.SetText("")
						.AddOptions(dadShirtOptions)
						.Select(dadShirtDefault)
						.AddListener(option =>
						{
							ButtonCallbacks.SetDadShirt(option.name);
							ButtonCallbacks.SaveDadShirt(option.name);
						}),

					new WYDSwitchInput<string>()
						.SetName("dadCharacterModel")
						.SetParent(characterCustomizationMenu)
						.SetPosition(-880f, 0f)
						.SetText("")
						.AddOptions(dadModelOptions)
						.Select(dadCharacterDefault)
						.AddListener(option =>
						{
							ButtonCallbacks.SetDadModel(option.value);
							ButtonCallbacks.SaveDadModel(option.value);
						}),

					new WYDSwitchInput<string>()
						.SetName("babyCharacterModel")
						.SetParent(characterCustomizationMenu)
						.SetPosition(-880f, 0f)
						.SetText("")
						.AddOptions(babyModelOptions)
						.Select(babyCharacterDefault)
						.AddListener(option =>
						{
							ButtonCallbacks.SetBabyModel(option.name);
							ButtonCallbacks.SaveBabyModel(option.name);
						}),
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

					new WYDMatchSettingsButton()
						.SetParent(menu)
						.SetPosition(790f, 445.5f)
						.AddListener(ButtonCallbacks.MatchSettingsButton);
				}
			}
			{ // Match Settings
				Mod.Print("make MatchSettings menus");

				void MakeMatchSettings(MenuUtil menuUtil, bool inGame)
				{
					GameObject menu = menuUtil.NewMenu("MatchSettings");
					int menuId = menuUtil.GetMenuId(menu.name);
					WYDTextButton backButton = WYDTextButton.MakeBackButton(menuUtil, menu.transform);
					menuUtil.menuController.startBtn[menuId] = backButton.gameObject;

					new WYDChangeMapButton(inGame)
						.SetParent(menu.transform)
						.SetPosition(backButton.gameObject.transform.localPosition + new Vector3(
							WYDTextButton.gap.x,
							0f,
							0f
						))
						.AddListener(ButtonCallbacks.ChangeMapButton);

					Vector2 bottomLeft = new Vector2(0f, 200f);
					float gap = 15f;

					LobbySettings ls = HexaMod.persistentLobby.lobbySettings;

					WYDUIElement[] options = {
						new WYDSwitchInput<ShufflePlayersMode>(
							"shufflePlayers", "", (int)ls.shufflePlayers, LobbySettings.shuffleOptions,
							menu.transform, new Vector2(45f, 0f),
							new UnityAction<WYDSwitchOption<ShufflePlayersMode>>[] {
								(WYDSwitchOption<ShufflePlayersMode> option) => {
									HexaMod.persistentLobby.lobbySettings.shufflePlayers = option.value;
									HexaMod.persistentLobby.CommitChanges();
									HexaMod.textChat.SendServerMessage($"shufflePlayers changed to {option.value}");
								}
							}
						),

						new WYDSwitchInput<SpawnLocationMode>(
							"spawnMode", "", (int)ls.spawnMode, LobbySettings.spawnOptions,
							menu.transform, new Vector2(45f, 0f),
							new UnityAction<WYDSwitchOption<SpawnLocationMode>>[] {
								delegate (WYDSwitchOption<SpawnLocationMode> option) {
									HexaMod.persistentLobby.lobbySettings.spawnMode = option.value;
									HexaMod.persistentLobby.CommitChanges();
									HexaMod.textChat.SendServerMessage($"spawnMode changed to {option.value}");
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
									HexaMod.textChat.SendServerMessage($"disablePets changed to {value}");
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
									HexaMod.textChat.SendServerMessage($"doorSounds changed to {value}");
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
									HexaMod.textChat.SendServerMessage($"ventSounds changed to {value}");
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
									HexaMod.textChat.SendServerMessage($"modernGrabbing changed to {value}");
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
									HexaMod.textChat.SendServerMessage($"allMustDie changed to {value}");
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
									HexaMod.textChat.SendServerMessage($"spectatingAllowed changed to {value}");
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
									HexaMod.textChat.SendServerMessage($"cheats changed to {value}");
								}
							}
						),

						new WYDSensitiveTextInputField()
							.SetName("relayServer")
							.SetParent(menu.transform)
							.SetPosition(455f, 0f)
							.SetText("Voice Chat Relay")
							.SetFieldText(ls.relay)
							.AddSubmitListener((string text) => {
								HexaMod.persistentLobby.lobbySettings.relay = text;
								HexaMod.persistentLobby.CommitChanges();
								HexaMod.textChat.SendServerMessage("Voice chat relay server updated.");
							})
					};

					if (inGame)
					{
						for (int i = 0; i < options.Count(); i++)
						{
							WYDUIElement control = options[i];

							if (control is WYDBooleanControl && control.gameObject.name == "cheats")
							{

								(control as WYDBooleanControl).control.interactable = false;
							}
						}
					}

					for (int i = 0; i < options.Count(); i++)
					{
						WYDUIElement control = options[i];

						control.rectTransform.localPosition = bottomLeft + new Vector2(control.rectTransform.localPosition.x, gap);
						bottomLeft.y = control.rectTransform.localPosition.y + control.rectTransform.sizeDelta.y;
					}

					// rob the title screen text because it's easier this way lmao
					GameObject titleText = Instantiate(title.FindMenu("SplashMenu").Find("Text").gameObject, menu.transform, true);
					titleText.name = "MatchSettingsActionText";
					Text titleTextComponent = titleText.GetComponent<Text>();
					titleTextComponent.text = "";
					titleTextComponent.fontSize = 25;
					titleText.AddComponent<ActionText>();
				}

				MakeMatchSettings(title, false);
				MakeMatchSettings(inGame, true);
				OnLevelsLoaded();
			}

			UpdateUIForLobbyState();
		}

		void Start()
		{
			StartCoroutine(DelayedStart());
		}

		IEnumerator DelayedStart()
		{
			yield return new WaitForSeconds(0.1f);

			if (HexaMod.persistentLobby.lobbySettingsFailed == true)
			{
				HexaMod.persistentLobby.lobbySettingsFailed = false;

				Mod.Warn("lobbySettingsFailed was true, switch to the lobby settings menus");
				title.menuController.ChangeToMenu(title.GetMenuId("MatchSettings"));
				yield return new WaitForSeconds(0.1f);
				title.FindMenu("MatchSettings").GetComponentInChildren<ActionText>().SendMessage("ActionDone", "Failed to load LobbySettings,\nPlease re-apply your settings here.");
			}
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
