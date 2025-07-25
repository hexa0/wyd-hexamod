using System.Reflection;
using UnityEngine;
using Type = System.Type;

namespace HexaMod.API.Util.Unity
{
	public static class ComponentSwapper
	{
		public static void SwapComponents<OriginalType, ReplacementType>(GameObject gameObject) where OriginalType : Component where ReplacementType : Component
		{
			Type originalType = typeof(OriginalType);

			foreach (OriginalType original in gameObject.GetComponentsInChildren<OriginalType>(true))
			{
				ReplacementType newComponent = original.gameObject.AddComponent<ReplacementType>();

				if (newComponent is OriginalType)
				{
					foreach (var field in originalType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
					{
						Mod.Debug($"copy field {field.Name} to new component");
						field.SetValue(newComponent, field.GetValue(original));
					}

					foreach (Component component in gameObject.GetComponentsInChildren<Component>(true))
					{
						Type componentType = component.GetType();

						foreach (var field in componentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
						{
							if (!field.IsStatic)
							{
								object fieldValue = field.GetValue(component);
								if (fieldValue is OriginalType && fieldValue == original)
								{
									Mod.Debug($"update reference from {componentType.FullName} in field {field.Name} to new component");
									field.SetValue(component, newComponent);
								}
							}
						}
					}
				}

				Object.DestroyImmediate(original);
			}
		}
	}
}
