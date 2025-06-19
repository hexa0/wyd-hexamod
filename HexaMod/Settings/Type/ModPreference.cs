using System;

public class ModPreference<T> : UnityPreference<T> where T : IComparable
{
	static readonly string prefix = "HMV2_";
	public ModPreference(string key, T defaultValue) : base(prefix + key, defaultValue) { }

	public new ModPreference<T> LinkTo(Action<T> action)
	{
		base.LinkTo(action);
		return this;
	}
}