using HexaMod.Util;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HexaMod.UI.Util
{
    public static class Templates
    {
        public static Button buttonTemplate;
        public static GameObject textInputFieldTemplate;
        public static Toggle hostControlToggleTemplate;
        public static Vector2 ButtonGap = new Vector2(320f, 168f);

        public static void Init()
        {
            Mod.Print("init ui template references");

            GameObject playLocalButton = Menus.titleScreen.Find("PlayLocal").gameObject;
            GameObject LobbyNameOriginal = Menus.online.Find("LobbyName").gameObject;
            GameObject SetSpectateOriginal = Menus.root.Find(GameModes.named["familyGathering"].hostMenuName).Find("SetSpectate").gameObject;

            if (!buttonTemplate)
            {
                buttonTemplate = Object.Instantiate(playLocalButton).GetComponent<Button>();
                buttonTemplate.name = "buttonTemplate";
                buttonTemplate.GetComponentInChildren<Text>(true).text = "buttonTemplate";
                buttonTemplate.onClick = new Button.ButtonClickedEvent();

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

            playLocalButton.GetComponent<Button>().interactable = false;
        }

        public static Button NewButton(string name, string text, Transform menu, Vector2 position, UnityAction[] actions)
        {
            Button button = Object.Instantiate(buttonTemplate.gameObject, menu).GetComponent<Button>();
            button.name = name;
            button.transform.localPosition = position;
            button.transform.GetChild(0).GetComponent<Text>().text = text;

            button.onClick = new Button.ButtonClickedEvent();
            foreach (var action in actions)
            {
                button.onClick.AddListener(action);
            }

            button.gameObject.SetActive(true);

            return button;
        }

        public static Button MakeBackButton(Transform menu, string backMenu = null)
        {
            return NewButton(
                "backButton", "Back", menu.transform,
                new Vector2(170f, 90f),
                new UnityAction[] {
                    delegate ()
                    {
                        if (backMenu != null)
                        {
                            Menus.menuController.ChangeToMenu(Menus.FindMenu(backMenu));
                        }
                        else
                        {
                            Menus.GoBack();
                        }
                    }
                }
            );
        }

        public static GameObject NewInputField(string name, string title, Transform menu, Vector2 position, UnityAction<string>[] changedActions, UnityAction<string>[] submitActions)
        {
            GameObject inputFieldRoot = Object.Instantiate(textInputFieldTemplate, menu);
            inputFieldRoot.name = name;
            inputFieldRoot.transform.localPosition = position;

            InputField field = inputFieldRoot.transform.GetChild(0).GetComponent<InputField>();
            Text text = inputFieldRoot.transform.GetChild(1).GetComponent<Text>();

            text.text = title;

            field.onValueChanged = new InputField.OnChangeEvent();
            foreach (var action in changedActions)
            {
                field.onValueChanged.AddListener(action);
            }

            field.onEndEdit = new InputField.SubmitEvent();
            foreach (var action in submitActions)
            {
                field.onEndEdit.AddListener(action);
            }

            inputFieldRoot.SetActive(true);

            return inputFieldRoot;
        }

        public static Toggle NewControlToggle(string name, string text, bool active, Transform menu, Vector2 position, UnityAction<bool>[] actions)
        {
            Toggle control = Object.Instantiate(hostControlToggleTemplate.gameObject, menu).GetComponent<Toggle>();
            control.name = name;
            control.transform.localPosition = position;
            control.transform.GetComponentInChildren<Text>(true).text = text;

            control.onValueChanged = new Toggle.ToggleEvent();
            control.isOn = active;

            foreach (var action in actions)
            {
                control.onValueChanged.AddListener(action);
            }

            control.gameObject.SetActive(true);

            return control;
        }
    }
}
