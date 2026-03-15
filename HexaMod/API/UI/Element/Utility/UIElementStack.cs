using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.API.UI.Element.Utility
{
	public class UIElementStack : HexaUIElement
	{
		readonly List<HexaUIElement> children = new List<HexaUIElement>();

		public float Gap
		{
			get => layoutGroup != null ? layoutGroup.spacing : m_gap;
			set
			{
				m_gap = value;
				if (layoutGroup != null)
				{
					layoutGroup.spacing = value;
				}
			}
		}

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
		float m_gap;
		HorizontalOrVerticalLayoutGroup layoutGroup;
		readonly ContentSizeFitter sizeFitter;

		public UIElementStack AddChild(HexaUIElement child)
		{
			children.Add(child
				.SetParent(rectTransform)
				.SetPivot(0.5f, 0.5f)
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

		public UIElementStack Clear()
		{
			// temporary array so we don't itterate while modifying
			HexaUIElement[] allChildren = children.ToArray();

			foreach (HexaUIElement child in allChildren)
			{
				RemoveChild(child);
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
			if (layoutGroup != null)
			{
				bool needsVertical = (m_alignment == StackAlignment.TopToBottom || m_alignment == StackAlignment.BottomToTop);
				bool isVertical = layoutGroup is VerticalLayoutGroup;

				if (needsVertical != isVertical)
				{
					Object.DestroyImmediate(layoutGroup);
					if (needsVertical)
						layoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();
					else
						layoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();
				}
			}
			else
			{
				if (m_alignment == StackAlignment.TopToBottom || m_alignment == StackAlignment.BottomToTop)
					layoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();
				else
					layoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();
			}

			layoutGroup.spacing = m_gap;
			layoutGroup.childControlWidth = false;
			layoutGroup.childControlHeight = false;
			layoutGroup.childForceExpandWidth = false;
			layoutGroup.childForceExpandHeight = false;

			switch (m_alignment)
			{
				case StackAlignment.BottomToTop:
					layoutGroup.childAlignment = TextAnchor.LowerLeft;
					for (int i = 0; i < children.Count; i++)
					{
						children[i].rectTransform.pivot = new Vector2(0f, 0f);
						children[i].rectTransform.SetAsFirstSibling();
					}
					break;
				case StackAlignment.TopToBottom:
					layoutGroup.childAlignment = TextAnchor.UpperLeft;
					for (int i = 0; i < children.Count; i++)
					{
						children[i].rectTransform.pivot = new Vector2(0f, 1f);
						children[i].rectTransform.SetAsLastSibling();
					}
					break;
				case StackAlignment.RightToLeft:
					layoutGroup.childAlignment = TextAnchor.LowerRight;
					for (int i = 0; i < children.Count; i++)
					{
						children[i].rectTransform.pivot = new Vector2(1f, 0f);
						children[i].rectTransform.SetAsFirstSibling();
					}
					break;
				case StackAlignment.LeftToRight:
					layoutGroup.childAlignment = TextAnchor.LowerLeft;
					for (int i = 0; i < children.Count; i++)
					{
						children[i].rectTransform.pivot = new Vector2(0f, 0f);
						children[i].rectTransform.SetAsLastSibling();
					}
					break;
			}
		}

		public UIElementStack(float gap) : base()
		{
			gameObject = new GameObject("elementStack", typeof(RectTransform));
			sizeFitter = gameObject.AddComponent<ContentSizeFitter>();
			sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			this.m_gap = gap;
			UpdateLayout();
		}
	}
}
