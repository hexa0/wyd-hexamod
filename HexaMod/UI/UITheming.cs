using static HexaMod.UI.Util.Menu;
using static HexaMod.UI.Util.Menu.Menus;
using UnityEngine.UI;
using UnityEngine;

namespace HexaMod.UI
{
	public class UITheming
	{
		public static void Init()
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
					var playerNames = title.FindMenu(hostMenu).Find("PlayerNames");
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
							text.fontSize = (int)(text.fontSize * 0.8f);
						}
					}
				}
			}
			{ // Buttons
				var originalColor = title.FindMenu("SplashMenu").Find("PlayLocal").GetComponent<Button>().colors.highlightedColor;

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
								text.fontSize = (int)(text.fontSize * 0.8f);
							}
						}
					}
				}
			}
		}
	}
}
