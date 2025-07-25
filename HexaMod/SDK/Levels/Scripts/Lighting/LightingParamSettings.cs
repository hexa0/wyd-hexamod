using HexaMod.API.Util.Migration;
using HexaMod.API.Util.WhosYourDaddy;
using UnityEngine;

namespace HexaMod.SDK.Levels.Scripts.Lighting
{
	[CreateAssetMenu(fileName = "LightingParamSettings.asset", menuName = "HexaMod/Lighting Volume Paramaters")]
	[UnityMigrationIdentifier("HexaMod.e38cf317-9577-4fba-bdd5-7aea3d38e2b4")]
	public class LightingParamSettings : MigratableScriptableObject
	{
		[Header("General Parameters")]

		[Tooltip("The gamemode name to apply to, leave blank to be treated as the default option")]
		[SerializeField]
		public string gamemode = string.Empty;

		[Header("Lighting Parameters")]

		[Tooltip("The ambient light color multiplier")]
		[Range(0f, 8f)]
		[SerializeField]
		public float skyLightingIntensity = 1f;
		[Tooltip("The reflection light color multiplier")]
		[Range(0f, 1f)]
		[SerializeField]
		public float skyReflectionsIntensity = 1f;

		internal static LightingParamSettings GetSettings(LightingParamSettings[] settings)
		{
			if (HexaGlobal.networkManager != null)
			{
				string currentGamemodeWithoutTeam = GameModes.gameModes[HexaGlobal.networkManager.curGameMode].internalName + ":A";
				string team = HexaGlobal.networkManager.playerObj && HexaGlobal.networkManager.isDad ? "D" : "B";
				string currentGamemodeWithTeam = GameModes.gameModes[HexaGlobal.networkManager.curGameMode].internalName + $":{team}";

				for (int i = 0; i < settings.Length; i++)
				{
					if (settings[i].gamemode == currentGamemodeWithoutTeam || settings[i].gamemode == currentGamemodeWithTeam)
						return settings[i];
				}

				for (int i = 0; i < settings.Length; i++)
				{
					if (string.IsNullOrEmpty(settings[i].gamemode))
						return settings[i];
				}

				return settings.Length > 0 ? settings[0] : null;
			}
			else
			{
				for (int i = 0; i < settings.Length; i++)
				{
					if (string.IsNullOrEmpty(settings[i].gamemode))
						return settings[i];
				}

				return settings.Length > 0 ? settings[0] : null;
			}
		}
	}
}
