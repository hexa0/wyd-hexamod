using System.Collections.Generic;
using UnityEngine;

namespace HexaMod.UI.Element.Utility
{
	public class UIElementStack : HexaUIElement
	{
		List<HexaUIElement> children = new List<HexaUIElement>();

		public float gap = 5f;

		public enum StackAlignment
		{
			TopToBottom,
			BottomToTop,
			LeftToRight,
			RightToLeft
		}

		public StackAlignment Alignment
		{
			get => m_alignment;
			set
			{
				if (m_alignment != value)
				{
					m_alignment = value;
					UpdateLayout();
				}
			}
		}

		StackAlignment m_alignment = StackAlignment.BottomToTop;

		public UIElementStack AddChild(HexaUIElement child)
		{
			children.Add(child
				.SetParent(rectTransform)
				.SetPivot(0.5f, 0.5f)
				.SetPosition(0f, 0f)
			);

			if (IsShown)
			{
				UpdateLayout();
			}

			return this;
		}

		public UIElementStack RemoveChild(HexaUIElement child)
		{
			children.Remove(child);
			child.SetParent(null);

			if (IsShown)
			{
				UpdateLayout();
			}

			return this;
		}

		public UIElementStack SetAlignment(StackAlignment alignment)
		{
			Alignment = alignment;

			return this;
		}

		public override void Shown()
		{
			base.Shown();

			UpdateLayout();
		}

		void UpdateLayout()
		{
			float x = 0f;
			float y = 0f;
			float totalWidth = 0f;
			float totalHeight = 0f;
			float contentWidth = 0f;
			float contentHeight = 0f;

			foreach (HexaUIElement child in children)
			{
				if (child.rectTransform.rect.width > totalWidth)
				{
					totalWidth = child.rectTransform.rect.width;
				}

				if (child.rectTransform.rect.height > totalHeight)
				{
					totalHeight = child.rectTransform.rect.height;
				}

				contentWidth += child.rectTransform.rect.width + gap;
				contentHeight += child.rectTransform.rect.height + gap;
			}

			contentWidth -= gap;
			contentHeight -= gap;

			switch (m_alignment)
			{
				case StackAlignment.BottomToTop:
					rectTransform.sizeDelta = new Vector2(totalWidth, contentHeight);

					foreach (HexaUIElement child in children)
					{
						child.rectTransform.pivot = new Vector2(0f, 0f);
						child.rectTransform.SetPivotPosition(new Vector2(0f, y));
						y += child.rectTransform.rect.height + gap;
					}

					break;
				case StackAlignment.TopToBottom:
					rectTransform.sizeDelta = new Vector2(totalWidth, contentHeight);
					y = contentHeight;

					foreach (HexaUIElement child in children)
					{
						child.rectTransform.pivot = new Vector2(0f, 1f);
						child.rectTransform.SetPivotPosition(new Vector2(0f, y));
						y -= child.rectTransform.rect.height + gap;
					}

					break;
				case StackAlignment.LeftToRight:
					rectTransform.sizeDelta = new Vector2(contentWidth, totalHeight);

					foreach (HexaUIElement child in children)
					{
						child.rectTransform.pivot = new Vector2(0f, 0f);
						child.rectTransform.SetPivotPosition(new Vector2(x, 0f));
						x += child.rectTransform.rect.width + gap;
					}

					break;
				case StackAlignment.RightToLeft:
					rectTransform.sizeDelta = new Vector2(contentWidth, totalHeight);
					x = contentWidth;

					foreach (HexaUIElement child in children)
					{
						child.rectTransform.pivot = new Vector2(1f, 0f);
						child.rectTransform.SetPivotPosition(new Vector2(x, 0f));
						x -= child.rectTransform.rect.width + gap;
					}

					break;
			}
		}

		public UIElementStack(float gap) : base()
		{
			gameObject = new GameObject("elementStack", typeof(RectTransform));
			this.gap = gap;
		}
	}
}
