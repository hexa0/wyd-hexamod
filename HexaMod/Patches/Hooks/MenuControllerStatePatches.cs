using HarmonyLib;
using HexaMod.UI.Util;

namespace HexaMod.Patches
{
	[HarmonyPatch]
	internal class MenuControllerStatePatches
	{
		[HarmonyPatch(typeof(MenuController), "ChangeToMenu")]
		[HarmonyPrefix]
		static void TrackBackstate(ref MenuController __instance, ref int val)
		{
			MenuUtil menu = Menu.Menus.GetMenuUtilForController(__instance);
			if (!menu.goingBack)
			{
				menu.backstates[val] = menu.currentMenu;
			}
		}
	}
}