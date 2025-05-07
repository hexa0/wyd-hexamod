using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches
{
	[HarmonyPatch(typeof(NetworkMovement))]
	internal class SharpCharacterReplication
	{
		[HarmonyPatch("SyncedMovement")]
		[HarmonyPrefix]
		static bool SharpCharacterReplicationPatch(ref NetworkMovement __instance)
		{
			var privateFields = Traverse.Create(__instance);

			var syncTime = privateFields.Field<float>("syncTime");
			var syncEndPosition = privateFields.Field<Vector3>("syncEndPosition");
			var syncEndRotation = privateFields.Field<Quaternion>("syncEndRotation");

			syncTime.Value += Time.deltaTime;
			__instance.transform.position = Vector3.Lerp(__instance.transform.position, syncEndPosition.Value, Mathf.Min(Time.smoothDeltaTime * 60f, 1f));
			__instance.transform.rotation = Quaternion.Lerp(__instance.transform.rotation, syncEndRotation.Value, Mathf.Min(Time.smoothDeltaTime * 60f, 1f));

			return false;
		}
	}
}
