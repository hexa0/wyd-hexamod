using HexaMod.Scripts.Levels.Volume;
using UnityEngine;

namespace HexaMapAssemblies
{
	public class LightingParamsVolume : VolumeBehavior
	{
		[Header("General Parameters")]

		[Tooltip("The volume's priority determines which volume's settings are applied when multiple volumes overlap.")]
		public uint priority = 0;

		[Header("Lighting Parameters")]

		[Tooltip("The ambient light color multiplier")]
		[Range(0f, 8f)]
		public float skyLightingIntensity = 1f;
		[Tooltip("The reflection light color multiplier")]
		[Range(0f, 1f)]
		public float skyReflectionsIntensity = 1f;
	}
}
