using System;

public class Preference<T> where T : IComparable
{
	public readonly string key;
	internal readonly object defaultValue;

	public T Value
	{
		get => (T)Get();
		set => Set(value);
	}

	internal virtual object Get()
	{
		throw new NotImplementedException();
	}

	internal virtual void Set(object value)
	{
		throw new NotImplementedException();
	}

	public Preference(string key, T defaultValue)
	{
		this.key = key;
		this.defaultValue = defaultValue;
	}
}