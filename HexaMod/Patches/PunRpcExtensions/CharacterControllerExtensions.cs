using HarmonyLib;
using HexaMapAssemblies;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches
{
    [HarmonyPatch(typeof(FirstPersonController))]
    internal class CharacterControllerExtensions
    {

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void CharacterControllerExtensionsPatch(ref FirstPersonController __instance)
        {
            __instance.gameObject.AddComponent<CharacterExtendedRPCBehavior>();
        }
    }
}
