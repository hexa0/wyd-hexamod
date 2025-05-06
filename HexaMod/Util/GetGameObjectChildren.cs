using System.Collections.Generic;
using UnityEngine;

namespace HexaMod.Util
{
	public static class ObjectUtils
	{
		public static List<GameObject> GetChildren(GameObject parent)
		{
			List<GameObject> children = new List<GameObject>();

			foreach (var item in parent.GetComponentsInChildren<Transform>())
			{
				if (item.transform.parent == parent.transform)
				{
					children.Add(item.gameObject);
				}
			}

			return children;
		}
	}
}
