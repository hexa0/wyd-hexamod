using HarmonyLib;
using HexaMod.API.UI.Util;
using HexaMod.API.Util.Patching;

namespace HexaMod.Patches.Hooks
{
	[ModdedPatch]
	[HarmonyPatch]
	internal class BackstatesHook
	{
		[HarmonyPatch(typeof(MenuController), "ChangeToMenu")]
		[HarmonyPrefix]
		static void TrackBackstate(ref MenuController __instance, ref int val)
		{
			MenuUtil menu = Menu.WYDMenus.GetMenuUtilForController(__instance);
			if (!menu.goingBack)
			{
				menu.backstates[val] = menu.CurrentMenu;
			}
		}
	}
}