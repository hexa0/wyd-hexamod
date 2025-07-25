using System;
using System.Reflection;
using HexaMod.API.Util.WhosYourDaddy;

namespace HexaMod.SDK.Levels.Scripts.System
{
	#pragma warning disable CS0649
	[Serializable]
	struct GenericGamemodeSelector<ValueType>
	{
		public string gamemode;
		public ValueType value;
	}

	public class GamemodeSelector<ValueType, ArrayType> where ArrayType : new()
	{
		readonly FieldInfo gamemodeField = typeof(ArrayType).GetField("gamemode", BindingFlags.Instance | BindingFlags.Public);
		readonly FieldInfo valueField = typeof(ArrayType).GetField("value", BindingFlags.Instance | BindingFlags.Public);

		public void Serialize(out string[] keys, out ValueType[] values, ArrayType[] editorArray)
		{
			keys = new string[editorArray.Length];
			values = new ValueType[editorArray.Length];

			for (int i = 0; i < editorArray.Length; i++)
			{
				ArrayType editorOption = editorArray[i];

				keys[i] = (string)gamemodeField.GetValue(editorOption);
				values[i] = (ValueType)valueField.GetValue(editorOption);
			}
		}

		public void Deserialize(string[] keys, ValueType[] values, out ArrayType[] editorArray)
		{
			if (keys == null || values == null || keys.Length != values.Length)
			{
				Mod.Fatal("GamemodeSelector.Deserialize: Input arrays are null or have mismatched lengths.");
				editorArray = new ArrayType[0];
				return;
			}

			editorArray = new ArrayType[keys.Length];

			for (int i = 0; i < editorArray.Length; i++)
			{
				editorArray[i] = new ArrayType();
				object editorOption = editorArray[i];

				gamemodeField.SetValue(editorOption, keys[i]);
				valueField.SetValue(editorOption, values[i]);

				editorArray[i] = (ArrayType)editorOption;
			}
		}

		public ValueType Select(ArrayType[] rawOptions)
		{
			GenericGamemodeSelector<ValueType>[] options = new GenericGamemodeSelector<ValueType>[rawOptions.Length];

			for (int i = 0; i < rawOptions.Length; i++)
			{
				ArrayType rawOption = rawOptions[i];

				options[i] = new GenericGamemodeSelector<ValueType>()
				{
					gamemode = (string)gamemodeField.GetValue(rawOption),
					value = (ValueType)valueField.GetValue(rawOption)
				};
			}

			string gameMode = "title";
			string team = "A";

			if (PhotonNetwork.inRoom)
			{
				if (HexaGlobal.networkManager.playerObj)
				{
					gameMode = GameModes.gameModes[HexaGlobal.networkManager.curGameMode].internalName;
				}

				team = HexaGlobal.networkManager.isDad ? "D" : "B";
			}

			// valid strings to check for
			// these are checked in order of priority
			string[] validStrings = new string[]
				{
				$"{gameMode}:{team}",
				$"{gameMode}:A",
				$"default:{team}",
				$"default:A",
			};

			for (int stringIndex = 0; stringIndex < validStrings.Length; stringIndex++)
			{
				for (int optionIndex = 0; optionIndex < options.Length; optionIndex++)
				{
					if (options[optionIndex].gamemode == validStrings[stringIndex])
					{
						return options[optionIndex].value;
					}
				}
			}

			Mod.Warn($"GamemodeSelector found no valid {typeof(ArrayType).Name} found for gameMode:{gameMode} and team:{team}, please fix your level by including a default value, checked strings:{string.Join("\n", validStrings)}");

			return options.Length > 0 ? options[0].value : default;
		}

		public GamemodeSelector()
		{
			if (gamemodeField == null || valueField == null)
			{
				throw new Exception($"GamemodeSelector: Fields 'gamemode' or 'value' not found on type {typeof(ArrayType).Name}");
			}
		}
	}
}