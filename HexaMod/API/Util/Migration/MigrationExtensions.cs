using System;
using UnityEngine;

namespace HexaMod.API.Util.Migration
{
	public static class MigrationExtensions
	{
		public static void BeforeMigrateSerialize(this ISerializationCallbackReceiver obj, Action<string> setStoredTypeId)
		{
			if (Attribute.GetCustomAttribute(obj.GetType(), typeof(UnityMigrationIdentifier)) is UnityMigrationIdentifier attribute)
			{
				setStoredTypeId.Invoke(attribute.Id);
			}
			else
			{
				Mod.Fatal($"{obj.GetType().FullName} doesn't have a UnityMigrationIdentifier, FIX THIS!!!!!");
				setStoredTypeId.Invoke(obj.GetType().FullName);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
		public static void AfterMigrateSerialize(this ISerializationCallbackReceiver obj)
		{
			
		}
	}
}
