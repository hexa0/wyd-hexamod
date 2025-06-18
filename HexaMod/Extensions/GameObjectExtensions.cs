using UnityEngine;

public static class GameObjectExtensions
{
	public static GameObject Find(this GameObject aObject, string aName)
	{
		return aObject.transform.Find(aName).gameObject;
	}

	public static GameObject FindDeep(this GameObject aObject, string aName)
	{
		return aObject.transform.FindDeep(aName).gameObject;
	}

	public static GameObject SetParent(this GameObject aObject, Transform aParent)
	{
		aObject.transform.SetParent(aParent);

		return aObject;
	}

	public static GameObject SetParent(this GameObject aObject, GameObject aParent)
	{
		aObject.transform.SetParent(aParent.transform);

		return aObject;
	}
}