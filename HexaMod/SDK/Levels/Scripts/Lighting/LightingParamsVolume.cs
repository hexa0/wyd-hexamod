using HexaMod.API.Util.Migration;
using HexaMod.API.Util.Volume;
using UnityEngine;

namespace HexaMod.SDK.Levels.Scripts.Lighting
{
	[UnityMigrationIdentifier("HexaMod.af09dcef-4779-4c8d-839b-a345e3b06792")]
	public class LightingParamsVolume : VolumeBehavior
	{
		[Header("General Parameters")]

		[Tooltip("The volume's priority determines which volume's settings are applied when multiple volumes overlap.")]
		public uint priority = 0;

		[Header("Lighting Parameters")]

		public LightingParamSettings[] settings = new LightingParamSettings[] {};
	}
}
