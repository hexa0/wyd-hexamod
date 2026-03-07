using HexaMod.API.Voice;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.API.UI.Element.VoiceChatUI.Debug
{
	public class ShortCircularBufferDebugView : HexaUIElement
	{
		readonly RectTransform readBar;
		readonly RectTransform writeBar;
		readonly RectTransform textArea;
		readonly Text text;

		readonly CircularShortBuffer buffer;

		public override void Update()
		{
			base.Update();

			readBar.sizeDelta = new Vector2((float)buffer.lastReadSize / buffer.capacity * rectTransform.rect.width, rectTransform.sizeDelta.y / 2f);
			writeBar.sizeDelta = new Vector2((float)buffer.lastWriteSize / buffer.capacity * rectTransform.rect.width, rectTransform.sizeDelta.y / 2f);

			readBar.SetPivotPosition(new Vector2((float)buffer.ReadHead / buffer.capacity * rectTransform.rect.width, rectTransform.sizeDelta.y));
			writeBar.SetPivotPosition(new Vector2((float)buffer.WriteHead / buffer.capacity * rectTransform.rect.width, 0f));
			text.text =
				$"Read Head: {buffer.ReadHead} ({buffer.realReadHead})\n" +
				$"Write Head: {buffer.WriteHead} ({buffer.realWriteHead})\n" +
				$"Capacity: {buffer.capacity}";
		}

		public ShortCircularBufferDebugView(CircularShortBuffer buffer) : base()
		{
			gameObject = new GameObject("circularBufferDebugger", typeof(RectTransform));
			rectTransform.sizeDelta = new Vector2(500f, 100f);
			rectTransform.pivot = new Vector2(0f, 0f);

			gameObject.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);

			readBar = new GameObject("readBar", typeof(RectTransform)).GetComponent<RectTransform>();
			writeBar = new GameObject("writeBar", typeof(RectTransform)).GetComponent<RectTransform>();

			readBar.sizeDelta = new Vector2(1f, 50f);
			readBar.pivot = new Vector2(0f, 1f);
			readBar.gameObject.AddComponent<Image>().color = Color.green;
			readBar.SetParent(rectTransform, false);

			writeBar.sizeDelta = new Vector2(1f, 50f);
			writeBar.pivot = new Vector2(0f, 0f);
			writeBar.gameObject.AddComponent<Image>().color = Color.red;
			writeBar.SetParent(rectTransform, false);

			textArea = new GameObject("textArea", typeof(RectTransform)).GetComponent<RectTransform>();
			textArea.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y * 0.5f);
			textArea.pivot = new Vector2(0f, 1f);
			textArea.SetParent(rectTransform, false);
			textArea.SetPivotPosition(0f, rectTransform.sizeDelta.y);

			text = textArea.gameObject.AddComponent<Text>();
			text.resizeTextForBestFit = true;
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

			this.buffer = buffer;
		}
	}
}
