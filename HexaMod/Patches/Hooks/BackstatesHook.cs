using HarmonyLib;
using HexaMod.UI.Util;

namespace HexaMod.Patches.Hooks
{
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
				menu.backstates[val] = menu.currentMenu;
			}
		}
	}
}