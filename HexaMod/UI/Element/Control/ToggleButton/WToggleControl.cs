using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;
using HexaMod.UI.Util;
namespace HexaMod.UI.Element.Control.ToggleButton
{
	public class WToggleControl : HexaUIElement
	{
		public Toggle control;
		public Text label;

		ButtonSoundBehavior buttonSoundBehavior;

		public WToggleControl SetState(bool state)
		{
			control.isOn = state;
			return this;
		}

		public WToggleControl LinkToPreference(Preference<bool> preference)
		{
			SetState(preference.Value)
				.AddListener(value => preference.Value = value);
			return this;
		}

		public WToggleControl SetText(string text)
		{
			label.text = text;
			return this;
		}

		public WToggleControl AddListener(UnityAction<bool> action)
		{
			control.onValueChanged.AddListener(action);
			return this;
		}

		public WToggleControl AddListeners(UnityAction<bool>[] actions)
		{
			foreach (UnityAction<bool> action in actions)
			{
				AddListener(action);
			}
			return this;
		}

		public WToggleControl ClearEvents()
		{
			control.onValueChanged = new Toggle.ToggleEvent();
			return this;
		}

		public WToggleControl() : base()
		{
			gameObject = Object.Instantiate(UITemplates.hostControlToggleTemplate.gameObject);

			control = gameObject.GetComponent<Toggle>();
			label = gameObject.GetComponentInChildren<Text>(true);
			gameObject.Find("Background").transform.localPosition = new Vector2(-130f, 0f);
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y - 10f);

			label.transform.localPosition = new Vector2(570f, -7f);
			label.raycastTarget = false;
			label.GetComponent<RectTransform>().sizeDelta = new Vector3(1000f, 1f);

			ClearEvents();

			buttonSoundBehavior = control.gameObject.AddComponent<ButtonSoundBehavior>();
			buttonSoundBehavior.ButtonUpSound = UISound.None;
			buttonSoundBehavior.ButtonDownSound = UISound.None;

			AddListener(state =>
			{
				if (IsShown)
				{
					buttonSoundBehavior.Play(state ? UISound.Yes : UISound.No);
				}
			});
		}

		public WToggleControl(string name, string text, bool active, Transform menu, Vector2 position, UnityAction<bool>[] actions) : this()
		{
			this.SetName(name)
				.SetParent(menu)
				.SetPosition(position)
				.SetText(text)
				.SetState(active)
				.AddListeners(actions);
		}
	}
}
