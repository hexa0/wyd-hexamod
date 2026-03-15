using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static HexaMod.API.UI.Util.Menu;

namespace HexaMod.API.UI.Element.Control.SwitchInput
{
	public class HoldArrowBehavior: MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
		public UnityEvent onPointerDown = new UnityEvent();
		public UnityEvent onPointerUp = new UnityEvent();

		public void OnPointerDown(PointerEventData eventData)
		{
			onPointerDown.Invoke();	
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			onPointerUp.Invoke();
		}
	}

	public class WHoldArrows : HexaUIElement
	{
		internal static GameObject boundsTemplate = HexaGlobal.coreBundle.LoadAsset<GameObject>("Assets/ModResources/Core/TemplateBoundingBox/switchElementBoundingBox.prefab");
		public Text label;
		public Button leftButton;
		public Button rightButton;

		public virtual void LeftArrowDown() {

		}

		public virtual void LeftArrowUp() {
			
		}

		public virtual void RightArrowDown() {

		}

		public virtual void RightArrowUp() {
			
		}

		public WHoldArrows SetText(string text)
		{
			label.text = text;
			label.supportRichText = true;
			return this;
		}

		public WHoldArrows() : base()
		{
			Transform videoOptionsMenu = WYDMenus.title.FindMenu("VideoOptionsMenu");

			gameObject = Object.Instantiate(boundsTemplate, videoOptionsMenu);
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x / 2f, rectTransform.sizeDelta.y);

			GameObject higher = Object.Instantiate(videoOptionsMenu.Find("HigherRes").gameObject, gameObject.transform, true);
			GameObject lower = Object.Instantiate(videoOptionsMenu.Find("LowerRes").gameObject, gameObject.transform, true);
			GameObject current = Object.Instantiate(videoOptionsMenu.Find("CurrentResolution").gameObject, gameObject.transform, true);

			label = current.GetComponent<Text>();
			rightButton = higher.GetComponent<Button>();
			leftButton = lower.GetComponent<Button>();

			HoldArrowBehavior leftButtonHold = leftButton.gameObject.AddComponent<HoldArrowBehavior>();
			HoldArrowBehavior rightButtonHold = rightButton.gameObject.AddComponent<HoldArrowBehavior>();

			rightButton.onClick = new Button.ButtonClickedEvent();
			leftButton.onClick = new Button.ButtonClickedEvent();

			leftButtonHold.onPointerDown.AddListener(() =>
			{
				this.LeftArrowDown();
			});

			leftButtonHold.onPointerUp.AddListener(() =>
			{
				this.LeftArrowUp();
			});

			rightButtonHold.onPointerDown.AddListener(() =>
			{
				this.RightArrowDown();
			});

			rightButtonHold.onPointerUp.AddListener(() =>
			{
				this.RightArrowUp();
			});

			label.alignment = TextAnchor.MiddleLeft;
			label.rectTransform.sizeDelta = new Vector2(2000f, 400f);
			label.rectTransform.localPosition = new Vector2(1125f, -4.2999f);

			higher.name = "higher";
			lower.name = "lower";
			current.name = "currentValue";
		}
	}
}
