using System.Collections.Generic;


public static class DictionaryExtensions
{
	// Provides GetValueOrDefault in older C#
	public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
	{
		return dictionary.TryGetValue(key, out TValue value) ? value : defaultValue;
	}

	public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
	{
		return dictionary.TryGetValue(key, out TValue value) ? value : default;
	}
}