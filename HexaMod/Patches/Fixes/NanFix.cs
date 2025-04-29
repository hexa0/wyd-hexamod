using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches
{
    [HarmonyPatch(typeof(FirstPersonController))]
    internal class NanFix
    {
        private static int memorySize = 120;
        private static List<Vector3> characterPositionMemory = new List<Vector3>();
        private static List<Quaternion> characterRotationMemory = new List<Quaternion>();
        private static List<Vector3> cameraPositionMemory = new List<Vector3>();
        private static List<Quaternion> cameraRotationMemory = new List<Quaternion>();
        public static bool fixingNaN = false;

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void Start(ref FirstPersonController __instance)
        {
            fixingNaN = false;
            characterPositionMemory.Clear();
            characterRotationMemory.Clear();
            cameraPositionMemory.Clear();
            cameraRotationMemory.Clear();
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void Update(ref FirstPersonController __instance)
        {
            PhotonView netView = __instance.GetComponent<PhotonView>();

            if (netView.isMine && !fixingNaN)
            {
                var privateFields = Traverse.Create(__instance);
                var m_Camera = privateFields.Field<Camera>("m_Camera");

                characterPositionMemory.Add(__instance.transform.position);
                characterRotationMemory.Add(__instance.transform.rotation);
                cameraPositionMemory.Add(m_Camera.Value.transform.position);
                cameraRotationMemory.Add(m_Camera.Value.transform.rotation);

                if (characterPositionMemory.Count > memorySize)
                {
                    characterPositionMemory.RemoveAt(0);
                    characterRotationMemory.RemoveAt(0);
                    cameraPositionMemory.RemoveAt(0);
                    cameraRotationMemory.RemoveAt(0);
                }

                // checking this against float.NaN doesn't work for some dumb reason
                if (__instance.transform.position.y.ToString() == "NaN")
                {
                    Mod.Warn("we're in the NaN zone???");

                    // __instance.enabled = false;
                    NaNFixBehavior nanFixBehavior = __instance.gameObject.AddComponent<NaNFixBehavior>();
                    nanFixBehavior.firstPersonController = __instance;
                    nanFixBehavior.characterPosition = characterPositionMemory[0];
                    nanFixBehavior.characterRotation = characterRotationMemory[0];
                    nanFixBehavior.cameraPosition = cameraPositionMemory[0];
                    nanFixBehavior.cameraRotation = cameraRotationMemory[0];

                    __instance.GetComponent<PhotonView>().RPC("FixNan", PhotonTargets.Others, new object[] { nanFixBehavior.characterPosition, nanFixBehavior.characterRotation, nanFixBehavior.cameraPosition, nanFixBehavior.cameraRotation });
                }
            }
        }
    }
}
