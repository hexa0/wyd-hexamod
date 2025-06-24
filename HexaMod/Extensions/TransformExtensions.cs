using UnityEngine;
using System.Collections.Generic;
using HexaMod.UI.Element;

public static class TransformExtensions
{
	//Breadth-first search
	public static Transform FindDeep(this Transform aParent, string aName)
	{
		Queue<Transform> queue = new Queue<Transform>();
		queue.Enqueue(aParent);
		while (queue.Count > 0)
		{
			var c = queue.Dequeue();
			if (c.name == aName)
				return c;
			foreach (Transform t in c)
				queue.Enqueue(t);
		}
		return null;
	}

	// Sane set pivot code
	public static void SetPivotPosition(this RectTransform self, Vector2 position)
	{
		RectTransform parent = self.parent as RectTransform;

		if (parent)
		{
			self.localPosition = new Vector2(
				position.x - (parent.rect.width * parent.pivot.x),
				position.y - (parent.rect.height * parent.pivot.y)
			);
		}
		else
		{
			self.localPosition = new Vector2(
				position.x - (self.rect.width * self.pivot.x),
				position.y - (self.rect.height * self.pivot.y)
			);
		}
	}

	public static void SetPivotPosition(this RectTransform self, float x, float y)
	{
		self.SetPivotPosition(new Vector2(x, y));
	}

	public static RectTransform GetParent(this RectTransform self)
	{
		return self.parent as RectTransform;
	}

	public static RectTransform ScaleWithParent(this RectTransform rectTransform)
	{
		rectTransform.offsetMin = Vector2.zero;
		rectTransform.offsetMax = Vector2.zero;
		rectTransform.anchorMin = new Vector2(0, 0);
		rectTransform.anchorMax = new Vector2(1, 1);
		rectTransform.anchoredPosition = Vector2.zero;
		rectTransform.sizeDelta = Vector2.zero;
		rectTransform.pivot = new Vector2(0.5f, 0.5f);
		return rectTransform;
	}
}