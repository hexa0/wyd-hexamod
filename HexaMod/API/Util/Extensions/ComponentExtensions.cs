using UnityEngine;

public static class ComponentExtensions
{
	public static Component Find(this Component aObject, string aName)
	{
		return aObject.transform.Find(aName);
	}

	public static Component FindDeep(this Component aObject, string aName)
	{
		return aObject.transform.FindDeep(aName);
	}
}