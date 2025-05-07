using UnityEngine.Events;
using UnityEngine;
using HexaMod.Util;

namespace HexaMod.UI.Class
{
	public class WYDHexColorInputField : WYDTextInputField
	{
		private string lastValidInput;

		private void Event(string hex, UnityAction<Color, string>[] actions)
		{
			bool isRouglyValid = (hex.Length == 7 && hex.StartsWith("#")) || (hex.Length == 6 && !hex.StartsWith("#"));

			if (isRouglyValid)
			{
				Color color = Color.white;

				try 
				{
					color = HexToColor.GetColorFromHex(hex);

					lastValidInput = hex;
					foreach (var colorAction in actions)
					{
						colorAction.Invoke(color, hex);
					}
				}
				catch 
				{
					field.text = lastValidInput;
				}
			}
			else
			{
				if (hex.Length > (hex.StartsWith("#") ? 7 : 6))
				{
					field.text = lastValidInput;
				}
			}
		}

		public WYDHexColorInputField(string name, string title, string defaultColorHex, Transform menu, Vector2 position, UnityAction<Color, string>[] changedActions, UnityAction<Color, string>[] submitActions)
			: base(
				  name, title, defaultColorHex, menu, position,
				  new UnityAction<string>[0],
				  new UnityAction<string>[0]
			)
		{
			field.characterLimit = 7;

			field.onValueChanged.AddListener((string hex) => {
				Event(hex, changedActions);
			});

			field.onEndEdit.AddListener((string hex) => {
				Event(hex, submitActions);
			});

			Init();
		}
	}
}
