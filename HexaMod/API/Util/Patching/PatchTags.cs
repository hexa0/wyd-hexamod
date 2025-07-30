using System;

namespace HexaMod.API.Util.Patching
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate, AllowMultiple = true)]
	public class VanillaPatch : Attribute
	{

	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate, AllowMultiple = true)]
	public class ModdedPatch : Attribute
	{

	}

	// not implemented yet
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate, AllowMultiple = false)]
	public class OptionalPatch : Attribute
	{
		public string Id { get; private set; }
		public string DisplayName { get; private set; }
		public string DisplayCategory { get; private set; }
		public bool OnByDefault { get; private set; }

		public OptionalPatch(string id, string displayName, string displayCategory, bool onByDefault = true)
		{
			Id = id;
			DisplayName = displayName;
			DisplayCategory = displayCategory;
			OnByDefault = onByDefault;
		}
	}
}
