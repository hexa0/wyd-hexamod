using UnityEngine.Events;
using UnityEngine;
using System.Collections.Generic;

namespace HexaMod.UI.Element.Control.TextInputField
{
	public class WHexColorInputField : WTextInputField
	{
		internal string lastValidInput;
		internal readonly List<UnityAction<Color, string>> changedActions = new List<UnityAction<Color, string>>();
		internal readonly List<UnityAction<Color, string>> submitActions = new List<UnityAction<Color, string>>();

		private void Event(string hex, bool isSubmit)
		{
			bool isRouglyValid = (hex.Length == 7 && hex.StartsWith("#")) || (hex.Length == 6 && !hex.StartsWith("#"));

			if (isRouglyValid)
			{
				try 
				{
					Color color = new Color().FromHex(hex);

					lastValidInput = hex;
					foreach (var colorAction in isSubmit ? submitActions : changedActions)
					{
						colorAction.Invoke(color, hex);
					}
					SetFieldTextColor(color);
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
					SetFieldTextColor(new Color().FromHex(field.text));
				}
			}
		}

		public WHexColorInputField() : base()
		{
			SetCharacterLimit(7)
				.AddChangedListener((string hex) =>
				{
					Event(hex, false);
				})
				.AddSubmitListener((string hex) =>
				{
					Event(hex, true);
				});
		}

		public override void Shown()
		{
			try
			{
				SetFieldTextColor(new Color().FromHex(field.text));
			} 
			catch
			{

			}
		}

		public WHexColorInputField AddChangedListener(UnityAction<Color, string> action)
		{
			changedActions.Add(action);

			return this;
		}

		public WHexColorInputField AddChangedListeners(UnityAction<Color, string>[] actions)
		{
			foreach (UnityAction<Color, string> action in actions)
			{
				AddChangedListener(action);
			}

			return this;
		}

		public WHexColorInputField AddSubmitListener(UnityAction<Color, string> action)
		{
			submitActions.Add(action);

			return this;
		}

		public WHexColorInputField AddSubmitListeners(UnityAction<Color, string>[] actions)
		{
			foreach (UnityAction<Color, string> action in actions)
			{
				AddSubmitListener(action);
			}

			return this;
		}

		public WHexColorInputField(string name, string title, string defaultColorHex, Transform menu, Vector2 position, UnityAction<Color, string>[] changedActions, UnityAction<Color, string>[] submitActions) : this()
		{
			this.SetName(name)
				.SetParent(menu)
				.SetPosition(position)
				.AddChangedListeners(changedActions)
				.AddSubmitListeners(submitActions)
				.SetText(title)
				.SetFieldText(defaultColorHex);
		}
	}
}
