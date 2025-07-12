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

		public LightingParamSettings[] settings = new LightingParamSettings[] {};
	}
}
