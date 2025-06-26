using HexaMod.UI.Element;
using HexaMod.Util;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.UI.Util
{
	public static class UITemplates
	{
		public static Button buttonTemplate;
		public static GameObject textInputFieldTemplate;
		public static Toggle hostControlToggleTemplate;

		public static void Init()
		{
			GameObject playLocalButton = Menu.WYDMenus.title.FindMenu("SplashMenu").Find("PlayLocal").gameObject;
			GameObject LobbyNameOriginal = Menu.WYDMenus.title.FindMenu("OnlineMenu").Find("LobbyName").gameObject;
			GameObject SetSpectateOriginal = Menu.WYDMenus.title.FindMenu(GameModes.named["familyGathering"].hostMenuName).Find("SetSpectate").gameObject;

			LobbyNameOriginal.transform.GetChild(0).GetComponent<InputField>().characterLimit = 32;

			if (!buttonTemplate)
			{
				buttonTemplate = Object.Instantiate(playLocalButton).GetComponent<Button>();
				buttonTemplate.name = "buttonTemplate";
				buttonTemplate.GetComponentInChildren<Text>(true).text = "buttonTemplate";

				buttonTemplate.gameObject.SetActive(false);
			}

			if (!textInputFieldTemplate)
			{
				textInputFieldTemplate = Object.Instantiate(LobbyNameOriginal);
				textInputFieldTemplate.name = "textInputFieldTemplate";

				InputField field = textInputFieldTemplate.transform.GetChild(0).GetComponent<InputField>();
				Text text = textInputFieldTemplate.transform.GetChild(1).GetComponent<Text>();

				field.onValueChanged = new InputField.OnChangeEvent();
				field.onEndEdit = new InputField.SubmitEvent();

				field.characterLimit = 0;
				field.text = "";
				field.name = "InputField";
				text.text = "textInputFieldTemplate";
				text.name = "Title";

				textInputFieldTemplate.SetActive(false);
			}

			if (!hostControlToggleTemplate)
			{
				hostControlToggleTemplate = Object.Instantiate(SetSpectateOriginal).GetComponent<Toggle>();
				hostControlToggleTemplate.name = "hostControlToggleTemplate";
				hostControlToggleTemplate.transform.GetComponentInChildren<Text>(true).text = "hostControlToggleTemplate";
				hostControlToggleTemplate.onValueChanged = new Toggle.ToggleEvent();
				Object.DestroyImmediate(hostControlToggleTemplate.GetComponent<HostControl>());
				Object.DestroyImmediate(hostControlToggleTemplate.GetComponent<PhotonView>());
				hostControlToggleTemplate.interactable = true;

				hostControlToggleTemplate.gameObject.SetActive(false);
			}
		}
	}
}
