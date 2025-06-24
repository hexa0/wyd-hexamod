using HexaMod;
using System;
using UnityEngine;

public class UnityPreference<T> : Preference<T> where T : IComparable
{
	internal override object Get()
	{
		string typeName = defaultValue.GetType().Name;

		switch (typeName)
		{
			case "Boolean":
				return PlayerPrefs.GetInt(key, (bool)defaultValue ? 1 : 0) == 1;
			case "Byte":
				return (byte)PlayerPrefs.GetInt(key, (byte)defaultValue);
			case "Int32":
				return PlayerPrefs.GetInt(key, (int)defaultValue);
			case "Single":
				return PlayerPrefs.GetFloat(key, (float)defaultValue);
			case "String":
				return PlayerPrefs.GetString(key, (string)defaultValue);
			default:
				throw new SystemException($"Unsupported Type {typeName}");
		}
	}

	internal override void Set(object value)
	{
		string typeName = defaultValue.GetType().Name;

		switch (typeName)
		{
			case "Boolean":
				PlayerPrefs.SetInt(key, (bool)value ? 1 : 0);
				break;
			case "Byte":
				PlayerPrefs.SetInt(key, (int)value);
				break;
			case "Int32":
				PlayerPrefs.SetInt(key, (int)value);
				break;
			case "Single":
				PlayerPrefs.SetFloat(key, (float)value);
				break;
			case "String":
				PlayerPrefs.SetString(key, (string)value);
				break;
			default:
				throw new SystemException($"Unsupported Type {typeName}");
		}

		PreferenceLinker.TriggerUpdate(key);
	}

	public UnityPreference(string key, T defaultValue) : base(key, defaultValue) { }

	public UnityPreference<T> LinkTo(Action<T> action)
	{
		PreferenceLinker.LinkTo(key, () =>
		{
			action(Value);
		});

		return this;
	}
}