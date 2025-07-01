using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using HexaMod.UI.Util;
using System.Linq;
using HexaMod.ScriptableObjects;
using HexaMod.Voice;
using static HexaMod.UI.Util.Menu.WYDMenus;
using HexaMod.UI.Element;
using HexaMod.UI.Element.Extended;
using HexaMod.SerializableObjects;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;
using HexaMod.Settings;
using System;
using HexaMod.UI.Element.Control;
using HexaMod.UI.Element.Control.TextButton;
using HexaMod.UI.Element.Control.SwitchInput;
using HexaMod.UI.Element.Control.ToggleButton;
using HexaMod.UI.Element.Control.TextInputField;
using HexaMod.UI.Element.VoiceChatUI;
using VoiceChatShared.Enums;
using HexaMod.UI.Element.Label;
using HexaMod.Scripts;
using HexaMod.UI.Element.Utility;
using HexaMod.Scripts.CustomCharacterModels;

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
				HexaGlobal.MakeTestGame(true);
			}

			public static void TestBabyButton()
			{
				HexaGlobal.MakeTestGame(false);
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
				PlayerPrefs.SetString("HMV2_CustomMap", mapName);

				HexaGlobal.textChat.SendServerMessage($"Map changed to {mapName}.");

				HexaPersistentLobby.instance.lobbySettings.mapName = mapName;
				HexaPersistentLobby.instance.CommitChanges();
			}
		}

		public void UpdateUIForLobbyState()
		{
			ModLevel foundLevel = Assets.defaultLevel;

			foreach (var level in Assets.levels)
			{
				if (level.levelPrefab.name == HexaPersistentLobby.instance.lobbySettings.mapName)
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

				mapInfo.SetText($"{foundLevel.levelNameReadable}\n{foundLevel.levelDescriptionReadable}");
			}
		}

		public void OnLevelsLoaded()
		{
			HexaMenus.loadingOverlay.controller.SetTaskState("LoadingMaps", false);

			UpdateUIForLobbyState();

			// Map Menu

			GameObject menu = title.NewMenu("ChangeMap");
			int menuId = title.GetMenuId(menu.name);
			UIElementStack bottomBarStack = new UIElementStack(WTextButton.padding.x)
				.SetParent(menu.transform)
				.SetName("bottomBarStack")
				.SetAnchors(0.5f, 0f)
				.SetPivot(0.5f, 0f)
				.SetAnchorPosition(0f, WTextButton.padding.y)
				.SetAlignment(UIElementStack.StackAlignment.LeftToRight);

			WTextButton backButton = WTextButton.MakeBackButton(title, menu.transform);
			title.menuController.startBtn[menuId] = backButton.gameObject;

			bottomBarStack.AddChild(backButton);

			float height = Mathf.Clamp(Assets.levels.Count() - 1, 0f, 4f);
			float width = Mathf.Floor((Assets.levels.Count() - 1) / 5f);

			for (int i = 0; i < Assets.levels.Count(); i++)
			{
				ModLevel level = Assets.levels[i];

				float x = Mathf.Floor(i / 5f) - (width / 2f);
				float y = -(i % 5) + (height / 2f) + 0.5f;

				new MapButton(level)
					.SetParent(menu.transform)
					.SetPosition(
						WTextButton.gap.x * x,
						WTextButton.gap.y * y
					)
					.AddListener(() => { ButtonCallbacks.ChangeLevel(level.levelPrefab.name); });
			}
		}

		public static WLabel mapInfo;

		public void Init()
		{
			if (PlayerPrefs.GetInt("HMV2_DoUITheme", 1) == 1)
			{
				UITheming.Init();
			}

			UITemplates.Init();

			if (HexaPersistentLobby.instance.lobbySettingsChanged == null)
			{
				throw new Exception("HexaMod.hexaLobby.lobbySettingsChanged is null");
			}

			HexaPersistentLobby.instance.lobbySettingsChanged.AddListener(delegate ()
			{
				if (Assets.StaticAssets.didCache)
				{
					Assets.AttemptToLoadCurrentLevel();
				}

				UpdateUIForLobbyState();
			});

			RectTransform version = title.root.Find("Version").gameObject.GetComponent<RectTransform>();
			version.gameObject.SetActive(false);

			//Text versionText = version.GetComponent<Text>();

			//version.ScaleWithParent();
			//versionText.alignment = TextAnchor.LowerLeft;
			//versionText.fontSize = (int)(versionText.fontSize * 0.5f);
			//version.sizeDelta = new Vector2(versionText.fontSize * -2f, versionText.fontSize * -2f);

			//mapInfo = Instantiate(version, title.root).GetComponent<Text>();
			//mapInfo.name = "mapInfo";
			//mapInfo.text = "";
			//mapInfo.alignment = TextAnchor.LowerRight;

			//version.SetParent(title.FindMenu("SplashMenu"), true);

			{ // HexaMod Options Menu
				void MakeButton(Button originalButton, MenuUtil menu)
				{
					new WTextButton()
						.SetName("hexaModOptions")
						.SetParent(originalButton.transform.parent)
						.SetPosition(new Vector2(originalButton.transform.localPosition.x, originalButton.transform.localPosition.y) + new Vector2(WTextButton.gap.x * 0.95f, 0f))
						.SetText("Hexa Mod")
						.SetFontSize(WTextButton.FontSizes.Small)
						.AddListener(() =>
						{
							menu.menuController.ChangeToMenu(menu.GetMenuId("HexaModOptions"));
						});

					new MatchSettingsButton()
						.SetParent(originalButton.transform.parent)
						.SetPosition(new Vector2(originalButton.transform.localPosition.x, originalButton.transform.localPosition.y) + new Vector2(WTextButton.gap.x * -0.95f, 0f))
						.AddListener(() =>
						{
							menu.menuController.ChangeToMenu(menu.GetMenuId("MatchSettings"));
						});
				}

				void MakeMenu(GameObject menu, MenuUtil menuUtil)
				{
					UIElementStack bottomBarStack = new UIElementStack(WTextButton.padding.x)
						.SetParent(menu.transform)
						.SetName("bottomBarStack")
						.SetAnchors(0.5f, 0f)
						.SetPivot(0.5f, 0f)
						.SetAnchorPosition(0f, WTextButton.padding.y)
						.SetAlignment(UIElementStack.StackAlignment.LeftToRight);

					WTextButton backButton = WTextButton.MakeBackButton(menuUtil, menu.transform);
					menuUtil.menuController.startBtn[menuUtil.GetMenuId(menu.name)] = backButton.gameObject;

					bottomBarStack.AddChild(backButton);

					var devices = VoiceChat.GetDevices();
					WSwitchOption<VoiceChat.MicrophoneDevice>[] deviceOptions = new WSwitchOption<VoiceChat.MicrophoneDevice>[devices.Length];

					for (int i = 0; i < devices.Length; i++)
					{
						deviceOptions[i] = new WSwitchOption<VoiceChat.MicrophoneDevice>()
						{
							name = $"({i + 1}/{devices.Length}) : {devices[i].capabilities.ProductName}",
							value = devices[i]
						};
					}

					WSwitchOption<int>[] audioBitrateOptions = new WSwitchOption<int>[Enum.GetValues(typeof(Bitrate)).Length];

					for (int i = 0; i < audioBitrateOptions.Length; i++)
					{
						int value = (byte)Enum.GetValues(typeof(Bitrate)).GetValue(i);

						audioBitrateOptions[i] = new WSwitchOption<int>()
						{
							name = Enum.GetName(typeof(Bitrate), value).Replace("Bitrate_Preset_", ""),
							value = value
						};
					}

					UIElementStack stack = new UIElementStack(WTextButton.padding.y)
						.SetParent(menu.transform)
						.SetAnchors(0.5f, 0f)
						.SetPivot(0.5f, 0f)
						.SetAnchorPosition(0f, WTextButton.padding.y + WTextButton.defaultSize.y + WTextButton.padding.y)
						.SetAlignment(UIElementStack.StackAlignment.BottomToTop);

					stack.AddChild(
						new WToggleControl()
							.SetName("micRnNoise")
							.SetText("Microphone Denoising (RNNoise)")
							.LinkToPreference(VoiceChat.denoisingEnabled)
					);

					stack.AddChild(
						new WToggleControl()
							.SetName("voiceChatDebugOverlayEnabled")
							.SetText("Voice Chat Debug Overlay")
							.LinkToPreference(VoiceChat.debugOverlayEnabled)
					);

					stack.AddChild(
						new WSwitchInput<int>()
							.SetName("microphoneBitrate")
							.SetText("")
							.AddOptions(audioBitrateOptions)
							.LinkToPreference(VoiceChat.microphoneBitrate)
					);

					stack.AddChild(
						new WSwitchInput<VoiceChat.MicrophoneDevice>()
							.SetName("microphoneDevice")
							.SetText("")
							.AddOptions(deviceOptions)
							.LinkToPreference(VoiceChat.microphoneDeviceId)
					);

					stack.AddChild(
						new MicrophoneIndicator()
					);

					stack.AddChild(
						new WToggleControl()
							.SetName("tabOutMute")
							.SetText("Mute While Tabbed Out")
							.LinkToPreference(HexaModPreferences.tabOutMute)
					);

					stack.AddChild(
						new WToggleControl()
							.SetName("uiRefresh")
							.SetText("Refreshed UI Colors (Requires Scene Reload)")
							.LinkToPreference(HexaModPreferences.doUItheme)
					);
				}

				MakeButton(title.FindMenu("OptionsMenu").Find("SplitScreenOptions").GetComponent<Button>(), title);
				MakeButton(inGame.FindMenu("OptionsMenu (1)").Find("SplitScreenOptions").GetComponent<Button>(), inGame);
				MakeMenu(title.NewMenu("HexaModOptions"), title);
				MakeMenu(inGame.NewMenu("HexaModOptions"), inGame);
			}
			{ // Return To Lobby Override
				if (PhotonNetwork.isMasterClient)
				{
					WTextButton hexaModOptions = new WTextButton(
						"returnToLobby", "Return To\nLobby", inGame.FindMenu("OptionsMenu (1)").Find("ReturnToMenu").GetComponent<Button>(),
						new UnityAction[] {
							() => {
								if (PhotonNetwork.room != null)
								{
									HexaGlobal.hexaLobby.ReturnToLobby();
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

					hexaModOptions.SetButtonSound(UISound.Back);
				}

				WTextButton backButton = new WTextButton(
					"back", "Back", inGame.FindMenu("OptionsMenu (1)").Find("Back").GetComponent<Button>(),
					new UnityAction[] {
						() => {
							FirstPersonController firstPersonController = HexaGlobal.networkManager.playerObj.GetComponent<FirstPersonController>();
							InGameMenuHelper inGameMenuHelper = HexaGlobal.networkManager.playerObj.GetComponentInChildren<InGameMenuHelper>();
							inGameMenuHelper.menuOn = false;
							inGame.menuController.DeactivateAll();
							firstPersonController.haltInput = false;
							inGameMenuHelper.TurnOnPlayer();
						}
					}
				);
			}
			{ // Title Screen
				Transform splashMenu = title.FindMenu("SplashMenu");

				// booooring
				splashMenu.Find("Return To New WYD").gameObject.SetActive(false);

				UIElementStack NewRow(string name)
				{
					return new UIElementStack(WTextButton.padding.x)
						.SetName(name)
						.SetPivot(0f, 0f)
						.SetPosition(0f, 0f)
						.SetAlignment(UIElementStack.StackAlignment.LeftToRight);
				}

				UIElementStack topRow = NewRow("top");
				UIElementStack middleRow = NewRow("middle");
				UIElementStack bottomRow = NewRow("bottom");

				 topRow.AddChild(new WTextButton()
					.SetName("testDad")
					.SetTextAuto("Test\nDad")
					.SetParent(title.FindMenu("SplashMenu"))
					.AddListener(ButtonCallbacks.TestDadButton));

				topRow.AddChild(new HexaUIElement(splashMenu.Find("PlayLocal").gameObject));
				topRow.AddChild(new HexaUIElement(splashMenu.Find("PlayOnline").gameObject));

				middleRow.AddChild(new WTextButton()
					.SetName("testBaby")
					.SetTextAuto("Test\nBaby")
					.SetParent(title.FindMenu("SplashMenu"))
					.AddListener(ButtonCallbacks.TestBabyButton));

				middleRow.AddChild(new HexaUIElement(splashMenu.Find("Challenges").gameObject));
				middleRow.AddChild(new HexaUIElement(splashMenu.Find("CharacterCusomization").gameObject));

				bottomRow.AddChild(new MatchSettingsButton()
					.SetParent(title.FindMenu("SplashMenu"))
					.AddListener(ButtonCallbacks.MatchSettingsButton));

				bottomRow.AddChild(new HexaUIElement(splashMenu.Find("Options").gameObject));
				bottomRow.AddChild(new HexaUIElement(splashMenu.Find("Quit").gameObject));

				UIElementStack titleStack = new UIElementStack(WTextButton.padding.y)
					.SetParent(splashMenu)
					.SetName("titleStack")
					.SetAnchors(0.5f, 0f)
					.SetPivot(0.5f, 0f)
					.SetAnchorPosition(0f, WTextButton.padding.y)
					.SetAlignment(UIElementStack.StackAlignment.TopToBottom);

				titleStack.AddChild(topRow);
				titleStack.AddChild(middleRow);
				titleStack.AddChild(bottomRow);

				int infoFontSize = 8;

				new WLabel()
					.SetName("versionText")
					.SetParent(splashMenu)
					.ScaleWithParent()
					.Resize(infoFontSize * -2f, infoFontSize * -2f)
					.SetText($"{Mod.GAME_VERSION.Substring(1)} (Game)\n{BuildInfo.Version} ({BuildInfo.GitHash}) (HexaMod)")
					.SetTextAligment(TextAnchor.LowerLeft)
					.SetTextFontSize(infoFontSize);

				mapInfo = new WLabel()
					.SetName("mapInfoText")
					.SetParent(title.root)
					.ScaleWithParent()
					.Resize(infoFontSize * -2f, infoFontSize * -2f)
					.SetText("")
					.SetTextAligment(TextAnchor.LowerRight)
					.SetTextFontSize(infoFontSize);

			}
			{ // Play Online Menu

			}
			{ // Character Customization Menu
				GameObject characterPreviewCanvas = GameObject.Find("BackendObjects").Find("MenuCamera").Find("Camera").Find("Canvas");
				dadModelSwapper = characterPreviewCanvas.Find("Dad").AddComponent<CharacterModelSwapper>();
				babyModelSwapper = characterPreviewCanvas.Find("Baby001").AddComponent<CharacterModelSwapper>();
				dadModelSwapper.initModel = PlayerPrefs.GetString("HMV2_DadCharacterModel", "default");
				dadModelSwapper.initShirt = PlayerPrefs.GetString("HMV2_DadShirtMaterial", "default");
				dadModelSwapper.initShirtColor = new Color().FromHex(GetCurrentShirtColorHex());
				dadModelSwapper.initSkinColor = new Color().FromHex(GetCurrentSkinColorHex());
				babyModelSwapper.initModel = PlayerPrefs.GetString("HMV2_BabyCharacterModel", "default");
				babyModelSwapper.initSkinColor = new Color().FromHex(GetCurrentBabySkinColorHex());

				Transform characterCanvas = GameObject.Find("BackendObjects").transform.FindDeep("Canvas");

				characterCanvas.Find("Dad").localPosition = new Vector3(250f, -500f, 0f);
				characterCanvas.Find("Baby001").localPosition = new Vector3(-250f, -100f, 0f);

				RectTransform characterCustomizationMenu = title.FindMenu("CharacterCustomizationMenu") as RectTransform;
				characterCustomizationMenu.ScaleWithParent();

				Vector2 bottomLeft = new Vector2(720f, -350f);

				WSwitchOption<string>[] dadModelOptions = new WSwitchOption<string>[Assets.dadCharacterModels.Count + 1];
				WSwitchOption<Material>[] dadShirtOptions = new WSwitchOption<Material>[Assets.shirts.Count + 1];

				WSwitchOption<string>[] babyModelOptions = new WSwitchOption<string>[Assets.babyCharacterModels.Count + 1];

				dadModelOptions[0] = new WSwitchOption<string>
				{
					name = "Dad (Original)",
					value = "default"
				};

				dadShirtOptions[0] = new WSwitchOption<Material>
				{
					name = "Solid Color Shirt",
					value = Assets.defaultShirt.shirtMaterial
				};


				babyModelOptions[0] = new WSwitchOption<string>
				{
					name = "Baby (Original)",
					value = "default"
				};

				int dadCharacterDefault = 0;
				int dadShirtDefault = 0;

				int babyCharacterDefault = 0;

				for (int i = 0; i < Assets.dadCharacterModels.Count; i++)
				{
					dadModelOptions[i + 1] = new WSwitchOption<string>
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
					dadShirtOptions[i + 1] = new WSwitchOption<Material>
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
					babyModelOptions[i + 1] = new WSwitchOption<string>
					{
						name = Assets.babyCharacterModels[i].modelNameReadable,
						value = Assets.babyCharacterModels[i].name
					};

					if (Assets.babyCharacterModels[i].name == babyModelSwapper.initModel)
					{
						babyCharacterDefault = i + 1;
					}
				}

				UIElementStack dadStack = new UIElementStack(WTextButton.padding.y)
					.SetParent(characterCustomizationMenu)
					.SetName("dadStack")
					.SetPivot(1f, 0.5f)
					.SetAnchors(1f, 0.5f)
					.SetAnchorPosition(-200f, 0f)
					.SetAlignment(UIElementStack.StackAlignment.BottomToTop);

				dadStack.AddChild(new WHexColorInputField()
					.SetName("shirtColor")
					.AddChangedListener((color, hex) => {
						dadModelSwapper.SetShirtColor(color);
						babyModelSwapper.SetShirtColor(color);
					})
					.AddSubmitListener((color, hex) =>
					{
						PlayerPrefs.SetString("HMV2_ShirtColor", hex);
					})
					.SetText("Shirt Color (Hex)")
					.SetFieldText(GetCurrentShirtColorHex()));

				dadStack.AddChild(new WHexColorInputField()
					.SetName("dadSkinColor")
					.AddChangedListener((color, hex) => {
						dadModelSwapper.SetSkinColor(color);
					})
					.AddSubmitListener((color, hex) =>
					{
						PlayerPrefs.SetString("HMV2_SkinColor", hex);
					})
					.SetText("Skin Color (Hex)")
					.SetFieldText(GetCurrentSkinColorHex()));

				dadStack.AddChild(new WSwitchInput<Material>()
					.SetName("dadShirtMaterial")
					.SetText("")
					.AddOptions(dadShirtOptions)
					.Select(dadShirtDefault)
					.AddListener(option =>
					{
						ButtonCallbacks.SetDadShirt(option.name);
						ButtonCallbacks.SaveDadShirt(option.name);
					}));

				dadStack.AddChild(new WSwitchInput<string>()
					.SetName("dadCharacterModel")
					.SetText("")
					.AddOptions(dadModelOptions)
					.Select(dadCharacterDefault)
					.AddListener(option =>
					{
						ButtonCallbacks.SetDadModel(option.value);
						ButtonCallbacks.SaveDadModel(option.value);
					}));

				UIElementStack babyStack = new UIElementStack(WTextButton.padding.y)
					.SetParent(characterCustomizationMenu)
					.SetName("babyStack")
					.SetPivot(0f, 0.5f)
					.SetAnchors(0f, 0.5f)
					.SetAnchorPosition(200f, 0f)
					.SetAlignment(UIElementStack.StackAlignment.BottomToTop);

				babyStack.AddChild(new WHexColorInputField()
					.SetName("babySkinColor")
					.AddChangedListener((color, hex) => {
						babyModelSwapper.SetSkinColor(color);
					})
					.AddSubmitListener((color, hex) =>
					{
						PlayerPrefs.SetString("HMV2_BabySkinColor", hex);
					})
					.SetText("Skin Color (Hex)")
					.SetFieldText(GetCurrentBabySkinColorHex()));

				babyStack.AddChild(new WSwitchInput<string>()
					.SetName("babyCharacterModel")
					.SetText("")
					.AddOptions(babyModelOptions)
					.Select(babyCharacterDefault)
					.AddListener(option =>
					{
						ButtonCallbacks.SetBabyModel(option.value);
						ButtonCallbacks.SaveBabyModel(option.value);
					}));
			}
			{ // Host Menus
				// we replace all of the match settings with our own menu so hide the originals
				title.FindMenu("Family Gathering-Host").Find("AlternateCharacters (2)").gameObject.SetActive(false);
				title.FindMenu("Family Gathering-Host").Find("SetSpectate").gameObject.SetActive(false);
				title.FindMenu("HungryGames").Find("SetSpectate (1)").gameObject.SetActive(false);

				foreach (string menuName in Util.Menu.hostMenus)
				{
					Transform menu = title.FindMenu(menuName);

					new MatchSettingsButton()
						.SetParent(menu)
						.SetPosition(790f, 445.5f)
						.AddListener(ButtonCallbacks.MatchSettingsButton);
				}
			}
			{ // Match Settings
				void MakeMatchSettings(MenuUtil menuUtil, bool inGame)
				{
					GameObject menu = menuUtil.NewMenu("MatchSettings");
					int menuId = menuUtil.GetMenuId(menu.name);

					WActionLabel errorMessage = new WActionLabel()
						.SetName("matchSettingsErrorMessage")
						.SetParent(menu.transform)
						.ScaleWithParent()
						.Resize(-200f, 0f);

					errorMessage
						.SetText("")
						.SetTextAligment(TextAnchor.MiddleCenter)
						.SetTextFontSize(WUIGlobals.Fonts.Sizes.MenuError);

					UIElementStack bottomBarStack = new UIElementStack(WTextButton.padding.x)
						.SetParent(menu.transform)
						.SetName("bottomBarStack")
						.SetAnchors(0.5f, 0f)
						.SetPivot(0.5f, 0f)
						.SetAnchorPosition(0f, WTextButton.padding.y)
						.SetAlignment(UIElementStack.StackAlignment.LeftToRight);

					WTextButton backButton = WTextButton.MakeBackButton(menuUtil, menu.transform);
					menuUtil.menuController.startBtn[menuId] = backButton.gameObject;

					bottomBarStack.AddChild(backButton);
					bottomBarStack.AddChild(new ChangeMapButton(inGame)
						.SetParent(menu.transform)
						.SetPivot(0f, 0f)
						.SetPosition(backButton.rectTransform.localPosition + new Vector3(
							backButton.rectTransform.sizeDelta.x + WTextButton.padding.x,
							0f,
							0f
						))
						.AddListener(ButtonCallbacks.ChangeMapButton));

					Vector2 bottomLeft = new Vector2(0f, 200f);

					LobbySettings ls = HexaPersistentLobby.instance.lobbySettings;

					UIElementStack stack = new UIElementStack(WTextButton.padding.y)
						.SetParent(menu.transform)
						.SetName("optionsStack")
						.SetAnchors(0.5f, 0f)
						.SetPivot(0.5f, 0f)
						.SetAnchorPosition(0f, WTextButton.padding.y + WTextButton.defaultSize.y + WTextButton.padding.y)
						.SetAlignment(UIElementStack.StackAlignment.BottomToTop);

					stack.AddChild(new WSwitchInput<ShufflePlayersMode>()
						.SetName("shufflePlayers")
						.SetText("")
						.AddOptions(LobbySettings.shuffleOptions)
						.SetOption((int)ls.shufflePlayers)
						.AddListener(option =>
						{
							HexaPersistentLobby.instance.lobbySettings.shufflePlayers = option.value;
							HexaPersistentLobby.instance.CommitChanges();
							HexaGlobal.textChat.SendServerMessage($"shufflePlayers changed to {option.value}");
						}));

					stack.AddChild(new WSwitchInput<SpawnLocationMode>()
						.SetName("spawnMode")
						.SetText("")
						.AddOptions(LobbySettings.spawnOptions)
						.SetOption((int)ls.spawnMode)
						.AddListener(option =>
						{
							HexaPersistentLobby.instance.lobbySettings.spawnMode = option.value;
							HexaPersistentLobby.instance.CommitChanges();
							HexaGlobal.textChat.SendServerMessage($"spawnMode changed to {option.value}");
						}));

					// TODO: move this into a map specific settings menu
					stack.AddChild(new WToggleControl()
						.SetName("disablePets")
						.SetText("Disable House Pets")
						.SetState(ls.disablePets)
						.AddListener(value =>
						{
							HexaPersistentLobby.instance.lobbySettings.disablePets = value;
							HexaPersistentLobby.instance.CommitChanges();
							HexaGlobal.textChat.SendServerMessage($"disablePets changed to {value}");
						}));

					stack.AddChild(new WToggleControl()
						.SetName("doorSounds")
						.SetText("Door Interact Sounds")
						.SetState(ls.doorSounds)
						.AddListener(value =>
						{
							HexaPersistentLobby.instance.lobbySettings.doorSounds = value;
							HexaPersistentLobby.instance.CommitChanges();
							HexaGlobal.textChat.SendServerMessage($"doorSounds changed to {value}");
						}));

					stack.AddChild(new WToggleControl()
						.SetName("ventSounds")
						.SetText("Vent Interact Sounds")
						.SetState(ls.ventSounds)
						.AddListener(value =>
						{
							HexaPersistentLobby.instance.lobbySettings.ventSounds = value;
							HexaPersistentLobby.instance.CommitChanges();
							HexaGlobal.textChat.SendServerMessage($"ventSounds changed to {value}");
						}));

					stack.AddChild(new WToggleControl()
						.SetName("modernGrabbing")
						.SetText("Modern Grabbing")
						.SetState(ls.modernGrabbing)
						.AddListener(value =>
						{
							HexaPersistentLobby.instance.lobbySettings.modernGrabbing = value;
							HexaPersistentLobby.instance.CommitChanges();
							HexaGlobal.textChat.SendServerMessage($"modernGrabbing changed to {value}");
						}));

					stack.AddChild(new WToggleControl()
						.SetName("allMustDie")
						.SetText("All Babies Must Die")
						.SetState(ls.allMustDie)
						.AddListener(value =>
						{
							HexaPersistentLobby.instance.lobbySettings.allMustDie = value;
							HexaPersistentLobby.instance.CommitChanges();
							HexaGlobal.textChat.SendServerMessage($"allMustDie changed to {value}");
						}));

					stack.AddChild(new WToggleControl()
						.SetName("spectatingAllowed")
						.SetText("Spectating Allowed")
						.SetState(ls.allowSpectating)
						.AddListener(value =>
						{
							HexaPersistentLobby.instance.lobbySettings.allowSpectating = value;
							HexaPersistentLobby.instance.CommitChanges();
							HexaGlobal.textChat.SendServerMessage($"spectatingAllowed changed to {value}");
						}));

					stack.AddChild(new WToggleControl()
						.SetName("cheats")
						.SetText("Cheats")
						.SetState(ls.cheats)
						.SetInteractable(!inGame && (PhotonNetwork.isMasterClient || !PhotonNetwork.inRoom))
						.AddListener(value =>
						{
							HexaPersistentLobby.instance.lobbySettings.cheats = value;
							HexaPersistentLobby.instance.CommitChanges();
							HexaGlobal.textChat.SendServerMessage($"cheats changed to {value}");
						}));

					stack.AddChild(new WSensitiveTextInputField()
						.SetName("relayServer")
						.SetText("Voice Chat Relay")
						.SetFieldText(ls.relay)
						.AddSubmitListener(text => {
							HexaPersistentLobby.instance.lobbySettings.relay = text;
							HexaPersistentLobby.instance.CommitChanges();
							HexaGlobal.textChat.SendServerMessage("Voice chat relay server updated.");
						}));
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

			if (HexaPersistentLobby.instance.lobbySettingsFailed == true)
			{
				HexaPersistentLobby.instance.lobbySettingsFailed = false;

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

		public static string GetCurrentBabySkinColorHex()
		{
			// todo: make this the actual default color code
			return PlayerPrefs.GetString("HMV2_BabySkinColor", "#CDA7A4");
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

			if (HexaMenus.loadingOverlay.controller.GetTaskState("RoomCreate"))
			{
				if (PhotonNetwork.room != null)
				{
					HexaMenus.loadingOverlay.controller.SetTaskState("RoomCreate", false);
				}
			}

			if (HexaMenus.loadingOverlay.controller.GetTaskState("RoomJoin"))
			{
				if (PhotonNetwork.room != null)
				{
					HexaMenus.loadingOverlay.controller.SetTaskState("RoomJoin", false);
				}
			}

			if (HexaMenus.loadingOverlay.controller.GetTaskState("LobbyJoin"))
			{
				if (PhotonNetwork.insideLobby && PhotonNetwork.lobby.Name == HexaGlobal.networkManager.curLobby)
				{
					HexaMenus.loadingOverlay.controller.SetTaskState("LobbyJoin", false);
				}
			}

			if (HexaMenus.loadingOverlay.controller.GetTaskState("PhotonConnect"))
			{
				if (PhotonNetwork.connectedAndReady || PhotonNetwork.connectionStateDetailed == ClientState.PeerCreated)
				{
					HexaMenus.loadingOverlay.controller.SetTaskState("PhotonConnect", false);
				}
			}

			if (HexaMenus.loadingOverlay.controller.GetTaskState("RoomLookForOrCreateTag"))
			{
				if (PhotonNetwork.inRoom && PhotonNetwork.room != null && PhotonNetwork.room.Name.StartsWith(HexaGlobal.networkManager.curTag))
				{
					HexaMenus.loadingOverlay.controller.SetTaskState("RoomLookForOrCreateTag", false);
				}
			}
		}
	}
}
