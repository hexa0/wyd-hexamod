using HarmonyLib;
using HexaMod.UI.Util;

namespace HexaMod.Patches
{
    [HarmonyPatch]
    internal class MenuControllerStatePatches
    {
        [HarmonyPatch(typeof(MenuController), "ChangeToMenu")]
        [HarmonyPrefix]
        static void TrackBackstate(ref int val)
        {
            if (!Menus.goingBack)
            {
                Menus.backstates[val] = Menus.currentMenu;
            }
        }
    }
}